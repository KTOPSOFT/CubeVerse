using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MalbersAnimations.Controller
{
    /// <summary>Use this Script's Transform as the Respawn Point</summary>
    [AddComponentMenu("Malbers/Animal Controller/Respawner")]
    public class MRespawner : MonoBehaviour
    {
        public static MRespawner instance;

        #region Respawn
        [Tooltip("Animal Prefab to Spawn"), FormerlySerializedAs("playerPrefab")]
        public GameObject player;

        //[ContextMenuItem("Set Default", "SetDefaultRespawnPoint")]
        //public Vector3Reference RespawnPoint;
        public StateID RespawnState;
        public FloatReference RespawnTime = new(4f);
        [Tooltip("If True: it will destroy the MainPlayer GameObject and Respawn a new One")]
        public BoolReference DestroyAfterRespawn = new(true);
        [Tooltip("The Respawner will be kept between scenes")]
        public BoolReference m_DontDestroyOnLoad = new(true);

        [Tooltip("Restart Scene After Death")]
        public BoolReference RestartScene = new();

        /// <summary>Active Player Animal GameObject</summary>
        private GameObject InstantiatedPlayer;
        /// <summary>Active Player Animal</summary>
        private MAnimal activeAnimal;
        /// <summary>Old Player Animal GameObject</summary>
        private GameObject oldPlayer;
        #endregion

        [FormerlySerializedAs("OnRestartGame")]
        public GameObjectEvent OnRespawned = new();



        private bool Respawned;

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            FindMainAnimal();
        }

        public virtual void SetPlayer(GameObject go) => player = go;

        void OnEnable()
        {
            if (!isActiveAndEnabled) return;

            if (instance == null)
            {
                instance = this;
                transform.parent = null;
                if (m_DontDestroyOnLoad) DontDestroyOnLoad(gameObject);
                //gameObject.name = gameObject.name + " Instance";
                SceneManager.sceneLoaded += OnLevelFinishedLoading;
                FindMainAnimal();
            }
            else
            {

            }
        }


        private void OnDisable()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnLevelFinishedLoading;

                if (activeAnimal != null)
                    activeAnimal.OnStateChange.RemoveListener(OnCharacterDead);  //Listen to the Animal changes of states

                instance = null;
            }
        }

        public void ResetScene()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            Respawned = false;
        }

        public void ResetRespawner(GameObject newPlayer)
        {
            Respawned = false;

            if (activeAnimal != null)
                activeAnimal.OnStateChange.RemoveListener(OnCharacterDead);  //Listen to the Animal changes of states

            SetPlayer(newPlayer);

            if (player == null)
            {
                activeAnimal = MAnimal.MainAnimal;
                if (activeAnimal) player = activeAnimal.gameObject;
            }

            if (player != null)
            {
                if (player.IsPrefab())
                {
                    InstantiateNewPlayer();
                }
                else
                {
                    if (player.TryGetComponent(out activeAnimal))
                    {
                        //Debug.Log("activeAnimal = " + activeAnimal);

                        activeAnimal.OnStateChange.AddListener(OnCharacterDead);        //Listen to the Animal changes of states
                        activeAnimal.OverrideStartState = RespawnState;
                        activeAnimal.SetMainPlayer();
                        Respawned = true;
                    }
                }
            }
        }

        /// <summary>Finds the Main Animal used as Player on the Active Scene</summary>
        public virtual void FindMainAnimal()
        {
            if (Respawned) return; //meaning the animal was already respawned.

            if (player == null)
            {
                activeAnimal = MAnimal.MainAnimal;
                if (activeAnimal) player = activeAnimal.gameObject;
            }

            if (player != null)
            {
                if (player.IsPrefab())
                {
                    InstantiateNewPlayer();
                }
                else
                {
                    if (player.TryGetComponent(out activeAnimal))
                    {
                        SceneAnimal();
                    }
                }
            }

            if (player != null && activeAnimal != null) //Make sure Death is not disabling stuffs
            {
                //make sure the Death does not disable all things... since where reusing the same animal

                var DeathState = activeAnimal.State_Get<Death>();

                if (DeathState)
                {
                    DeathState.disableAnimal = false;
                    DeathState.DisableAllComponents = false;
                    DeathState.DisableInternalColliders = false;
                    DeathState.DisableMainCollider = false;
                }
            }

            //else
            //{
            //    Debug.LogWarning("[Respawner Removed]. There's no Character assigned", this);
            //    Destroy(gameObject); //Destroy This GO since is already a Spawner in the scene
            //}
        }

        private void SceneAnimal()
        {
            activeAnimal.OnStateChange.AddListener(OnCharacterDead);        //Listen to the Animal changes of states
            activeAnimal.Teleport_Internal(transform.position);             //Move the Animal to is Start Position
            activeAnimal.transform.rotation = (transform.rotation);         //Move the Animal to is Start Position
            activeAnimal.OverrideStartState = RespawnState;
            activeAnimal.InputSource?.Enable(true);         //Enable the Input for the Player
            if (activeAnimal.MainCollider) activeAnimal.MainCollider.enabled = (true);
            activeAnimal.SetMainPlayer();
            activeAnimal.Anim.Rebind();

            var allCompo = activeAnimal.GetComponentsInChildren<IRestart>();
            foreach (var item in allCompo) item.Restart();

            Respawned = true;
        }

        /// <summary>Listen to the Animal States</summary>
        public void OnCharacterDead(int StateID)
        {
            if (!Respawned) return;

            if (StateID == StateEnum.Death)                      //Means Death
            {
                oldPlayer = InstantiatedPlayer;                  //Store the old player IMPORTANT

                activeAnimal.OnStateChange.RemoveListener(OnCharacterDead);        //Remove listener from the Animal

                if (player != null)
                {
                    if (player.IsPrefab())         //If the Player is a Prefab then then instantiate it on the created scene
                    {
                        this.Delay_Action(RespawnTime, () =>
                         {
                             DestroyDeathPlayer();
                             this.Delay_Action(() => InstantiateNewPlayer()); //Instantiate next frame
                         }
                        );
                    }
                    else
                    {
                        if (RestartScene.Value)
                        {
                            this.Delay_Action(RespawnTime, () => ResetScene());
                        }
                        else
                        {
                            this.Delay_Action(RespawnTime, () =>
                            {
                                SceneAnimal();

                                if (!activeAnimal.enabled)
                                    activeAnimal.enabled = true;
                                else
                                    activeAnimal.ResetController();

                                //activeAnimal.Anim.Rebind(); //Reset the Animator (THIS BREAK THE MODE BEHAVIOURS)
                            }
                            );
                        }
                    }
                }
            }
        }

        void DestroyDeathPlayer()
        {
            if (oldPlayer != null)
            {
                if (DestroyAfterRespawn)
                    Destroy(oldPlayer);
                else
                    DestroyAllComponents(oldPlayer);
            }
        }

        void InstantiateNewPlayer()
        {
            // Debug.Log("InstantiateNewPlayer");
            InstantiatedPlayer = Instantiate(player, transform.position, transform.rotation);
            activeAnimal = InstantiatedPlayer.GetComponent<MAnimal>();
            activeAnimal.OverrideStartState = RespawnState;
            activeAnimal.OnStateChange.AddListener(OnCharacterDead);
            OnRespawned.Invoke(InstantiatedPlayer);
            activeAnimal.SetMainPlayer();
            Respawned = true;
        }


        /// <summary>Destroy all the components on  Animal and leaves the mesh and bones</summary>
        private void DestroyAllComponents(GameObject target)
        {
            if (!target) return;

            var components = target.GetComponentsInChildren<MonoBehaviour>();
            foreach (var comp in components) Destroy(comp);
            var colliders = target.GetComponentsInChildren<Collider>();
            if (colliders != null)
            {
                foreach (var col in colliders) Destroy(col);
            }
            var rb = target.GetComponentInChildren<Rigidbody>();
            if (rb != null) Destroy(rb);
            var anim = target.GetComponentInChildren<Animator>();
            if (anim != null) Destroy(anim);
        }


        public virtual void Respawn()
        {
            if (player != null)
            {
                if (player.IsPrefab())         //If the Player is a Prefab then then instantiate it on the created scene
                {
                    DestroyDeathPlayer();
                    this.Delay_Action(() => InstantiateNewPlayer()); //Instantiate next frame
                }
                else
                {
                    if (RestartScene.Value)
                    {
                        ResetScene();
                    }
                    else
                    {
                        SceneAnimal();

                        if (!activeAnimal.enabled)
                            activeAnimal.enabled = true;
                        else
                            activeAnimal.ResetController();
                    }
                }
            }
        }
    }
}