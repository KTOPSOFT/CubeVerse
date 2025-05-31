using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Unity [Tools] Utilities")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/ui/unity-utils")]
    public class UnityUtils : MonoBehaviour
    {

        /// <summary>Instantiate a GameObject chosen randomly from a GameObjectList in the position of this gameObject</summary>
        public void InstantiateRandom(GameObjectList value)
        {
            GameObject randomObject = value.list[Random.Range(0, value.list.Count)];
            Instantiate(randomObject, transform.position, transform.rotation);

        }

        /// <summary>Instantiate a GameObject chosen randomly from a GameObjectList in the position of this gameObject and it also  parent it </summary>
        public void InstantiateRandomAndParent(GameObjectList value)
        {
            GameObject randomObject = value.list[Random.Range(0, value.list.Count)];
            Instantiate(randomObject, transform.position, transform.rotation, transform);
        }


        public virtual void PauseEditor()
        {
            Debug.Log("Pause Editor", this);
            Debug.Break();
        }




        /// <summary>Multiply the Local Scale by a value</summary>
        public virtual void Scale_By_Float(float scale) => transform.localScale = Vector3.one * scale;


        private AudioSource[] audios;
        /// <summary>Ugly way to stop all audiosources on the scenes</summary>
        public virtual void PauseAllAudio(bool pause)
        {
            if (!enabled) return;

            audios ??= FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

            if (pause)
            {
                foreach (var audio in audios)
                {
                    if (audio.isPlaying) audio.Pause();
                }
            }
            else
            {
                foreach (var audio in audios) if (audio != null) audio.UnPause();
            }
        }


        public void AddRigiBody()
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();

            // Lock all constraints on the Rigidbody
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;

        }
        public void DestroyRigidbody()
        {
            if (gameObject.TryGetComponent<Rigidbody>(out var rb))
                Destroy(rb);
        }


        public void IsKinematicRigidbody(GameObject gameObjectWithRigidBody)
        {
            gameObjectWithRigidBody.GetComponent<Rigidbody>().isKinematic = true;
        }

        public void IsNotKinematicRigidbody(GameObject gameObjectWithRigidBody)
        {
            gameObjectWithRigidBody.GetComponent<Rigidbody>().isKinematic = false;
        }





        public void Forward_Direction(Transform target)
        {
            transform.forward = (target.position - transform.position).normalized;
        }
        public void Forward_Direction(TransformVar target) => Forward_Direction(target.Value);


        public void Forward_Direction_NoY(Transform target)
        {
            var forward = (target.position - transform.position);
            forward.y = 0;
            transform.forward = forward.normalized;
        }
        public void Forward_Direction_NoY(TransformVar target) => Forward_Direction_NoY(target.Value);
        public virtual void Toggle_Enable(Behaviour component) => component.enabled = !component.enabled;
        public virtual void Time_Freeze(bool value) => Time_Scale(value ? 0 : 1);
        public virtual void Time_Scale(float value) => Time.timeScale = value;
        public virtual void Freeze_Time(bool value) => Time_Freeze(value);

        /// <summary>Destroy this GameObject by a time </summary>
        public void DestroyMe(float time) => Destroy(gameObject, time);

        /// <summary>Destroy this GameObject</summary>
        public void DestroyMe() => Destroy(gameObject);

        /// <summary>Destroy this GameObject on the Next Frame</summary>
        public void DestroyMeNextFrame() => StartCoroutine(DestroyNextFrame());

        /// <summary>Destroy a GameObject</summary>
        public void DestroyGameObject(GameObject go) => Destroy(go);

        /// <summary>Destroy a Component</summary>
        public void DestroyComponent(Component component) => Destroy(component);

        /// <summary>Disable a gameObject and enable it the next frame</summary>
        public void Reset_GameObject(GameObject go)
        {
            go.SetActive(false);
            this.Delay_Action(() => go.SetActive(true));
        }

        /// <summary>Disable a Monobehaviour and enable it the next frame</summary>
        public void Reset_Monobehaviour(MonoBehaviour go)
        {
            go.SetEnable(false);
            this.Delay_Action(() => go.SetEnable(true));
        }

        /// <summary>Hide this GameObject after X Time</summary>
        public void GameObjectHide(float time) => Invoke(nameof(DisableGo), time);

        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundX() => transform.Rotate(new Vector3(Random.Range(0, 360), 0, 0), Space.Self);

        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundY() => transform.Rotate(new Vector3(0, Random.Range(0, 360), 0), Space.Self);
        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundZ() => transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)), Space.Self);

        //public void Move_Local(Vector3Var vector) => transform.Translate(vector, Space.Self);
        //public void Move_World(Vector3Var vector) => transform.Translate(vector, Space.World);

        public void DebugLog(string value) => Debug.Log($"[{name}]-[{value}]", this);
        public void DebugLog(object value) => Debug.Log($"[{name}]-[{value}]", this);

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Reset the Local Rotation of this gameObject</summary>
        public void Rotation_Reset() => transform.localRotation = Quaternion.identity;

        /// <summary>Reset the Local Position of this gameObject</summary>
        public void Position_Reset() => transform.localPosition = Vector3.zero;

        /// <summary>Reset the Local Rotation of a gameObject</summary>
        public void Rotation_Reset(GameObject go) => go.transform.localRotation = Quaternion.identity;

        /// <summary>Reset the Local Position of a gameObject</summary>
        public void Position_Reset(GameObject go) => go.transform.localPosition = Vector3.zero;

        /// <summary>Reset the Local Rotation of a transform</summary>
        public void Rotation_Reset(Transform go) => go.localRotation = Quaternion.identity;

        /// <summary>Reset the Local Position of a transform</summary>
        public void Position_Reset(Transform go) => go.localPosition = Vector3.zero;

        /// <summary>Parent this Game Object to a new Transform, retains its World Position</summary>
        public void Parent(Transform value) => transform.parent = value;
        public void Parent(GameObject value) => Parent(value.transform);
        public void Parent(Component value) => Parent(value.transform);

        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(Transform value) => value.parent = null;
        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(GameObject value) => Unparent(value.transform);
        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(Component value) => Unparent(value.transform);

        public void ParentToThis(Transform value) => value.transform.parent = this.transform;

        public void ParentToThis(GameObject value) => ParentToThis(value.transform);

        public void ParentToThis(Component value) => ParentToThis(value.transform);

        public void UnparentAllFromThis()
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).parent = null;
                }
            }
        }

        public void UnparentAllFromThisIsNotKinematic()
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject.GetComponent<Rigidbody>() != null)
                    {
                        IsNotKinematicRigidbody(transform.GetChild(i).gameObject);
                    }
                    transform.GetChild(i).parent = null;
                }
            }
        }


        #region RigidBody
        public void RigidBody_KinematicTrue(Rigidbody rb) => rb.isKinematic = true;

        public void RigidBody_KinematicFalse(Rigidbody rb) => rb.isKinematic = false;

        public void RigidBody_KinematicTrue(Collider go)
        {
            if (go.attachedRigidbody != null) go.attachedRigidbody.isKinematic = true;
        }

        public void RigidBody_KinematicFalse(Collider go)
        {
            if (go.attachedRigidbody != null) go.attachedRigidbody.isKinematic = false;
        }

        #endregion


        /// <summary>Disable a behaviour on a gameobject using its index of all the behaviours attached to the gameobject.
        /// Useful when they're duplicated components on a same gameobject </summary>
        public void Behaviour_Disable(int index)
        {
            var components = GetComponents<Behaviour>();
            if (components != null)
            {
                components[index % components.Length].enabled = false;
            }
        }

        /// <summary>Enable a behaviour on a gameobject using its index of all the behaviours attached to the gameobject.
        /// Useful when they're duplicated components on a same gameobject </summary>
        public void Behaviour_Enable(int index)
        {
            var components = GetComponents<Behaviour>();
            if (components != null)
            {
                components[index % components.Length].enabled = true;
            }
        }

        public void Behaviour_EnableNextFrame(Behaviour behaviour)
        {
            behaviour.enabled = false;
            this.Delay_Action(() => behaviour.enabled = true);
        }

        /// <summary>Add an gameobject to Don't destroy on load logic</summary>
        public void Dont_Destroy_On_Load(GameObject value) => DontDestroyOnLoad(value);

        /// <summary>Loads additive a new scene</summary>
        public void Load_Scene_Additive(string value)
        {
            SceneManager.LoadScene(value, LoadSceneMode.Additive);
        }

        /// <summary>Loads a new scene</summary>
        public void Load_Scene(string value)
        {
            if (!string.IsNullOrEmpty(value))
                SceneManager.LoadScene(value, LoadSceneMode.Single);
        }

        /// <summary>Parent this GameObject to a new Transform</summary>
        public void Parent_Local(Transform value)
        {
            transform.parent = value;
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        /// <summary>Parent a GameObject to a new Transform</summary>
        public void Parent_Local(GameObject value) => Parent_Local(value.transform);
        public void Parent_Local(Component value) => Parent_Local(value.transform);


        /// <summary>Instantiate a GameObject in the position of this gameObject</summary>
        public void Instantiate(GameObject value) => Instantiate(value, transform.position, transform.rotation);

        /// <summary>Instantiate a GameObject in the position of this gameObject and it also  parent it </summary>
        public void InstantiateAndParent(GameObject value) => Instantiate(value, transform.position, transform.rotation, transform);


        /// <summary>Show/Hide the Cursor</summary>
        public static void ShowCursor(bool value)
        {
            Cursor.lockState = !value ? CursorLockMode.Locked : CursorLockMode.None;  // Lock or unlock the cursor.
            Cursor.visible = value;
        }

        public static void ShowCursorInvert(bool value) => ShowCursor(!value);

        private void DisableGo() => gameObject.SetActive(false);

        private IEnumerator C_Reset_GameObject(GameObject go)
        {
            if (go.activeInHierarchy)
            {
                go.SetActive(false);
                yield return null;
                go.SetActive(true);

            }
            yield return null;
        }
        IEnumerator C_Reset_Mono(MonoBehaviour go)
        {
            if (go.gameObject.activeInHierarchy)
            {
                go.enabled = (false);
                yield return null;
                go.enabled = (true);

            }
            yield return null;
        }

        IEnumerator DestroyNextFrame()
        {
            yield return null;
            Destroy(gameObject);
        }

        public void RectTransform_Width(float width)
        {
            RectTransform rt = GetComponent<RectTransform>();

            if (rt)
            {
                rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
            }
        }

        public void RectTransform_Height(float height)
        {
            RectTransform rt = GetComponent<RectTransform>();

            if (rt)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
            }
        }
        public void RectTransform_Width(int width) => RectTransform_Width((float)width);
        public void RectTransform_Height(int height) => RectTransform_Height((float)height);
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UnityUtils))]
    public class UnityUtilsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Use this component to execute simple unity logics, " +
                "such as Parent, Instantiate, Destroy, Disable..\nUse it via Unity Events");
        }
    }
#endif
}
