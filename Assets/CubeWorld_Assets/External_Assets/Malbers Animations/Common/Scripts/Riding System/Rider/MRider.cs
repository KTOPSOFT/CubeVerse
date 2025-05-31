using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Linq;
using System;
using MalbersAnimations.Controller;



#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>  Horse Animset Pro RIDING SYSTEM  </summary>
namespace MalbersAnimations.HAP
{
    public enum DismountType { Random, MountInput, Last, RiderInput }
    [AddComponentMenu("Malbers/Riding/Rider")]
    [DefaultExecutionOrder(-50)]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/riding/mrider")]
    public class MRider : MonoBehaviour, IAnimatorListener, IRider
    {
        #region Public Variables
        /// <summary>Parent to mount Point </summary>
        public BoolReference Parent = new(true);

        /// <summary>This animal is the one that you can call or StartMount </summary>
        public GameObjectReference m_MountStored = new();

        public Mount MountStored
        {
            get => p_MountStored;
            protected set
            {
                p_MountStored = value;
                //Debug.Log("mountTrigger " + mountTrigger);
            }
        }
        private Mount p_MountStored;

        /// <summary>True iff we want to start mounted an animal </summary> 
        public BoolReference StartMounted;

        [Tooltip("Resync Animations with the Mount")]
        public bool ReSync = true;

        [Tooltip("Gravity Direction." +
            "This is used to properly orient the Rider back to the Defult Gravity when dismounting. E.g. When the Animal is on a Slope.")]
        public Vector3Reference Gravity = new(Vector3.down);

        [SerializeField] private BoolReference m_CanMount = new(false);
        [SerializeField] private BoolReference m_CanDismount = new(false);
        [SerializeField] private BoolReference m_CanCallAnimal = new(false);

        [Tooltip("The rider Rigidbody will be set to kinematic while riding")]
        [SerializeField] private BoolReference kinematicRider = new(true);
        private bool DefaultKinematic;


        [Tooltip("Calculate the IK for the Feet here in this script")]
        [SerializeField] private bool useIKFeet = true;

        /// <summary>Changes the Dismount animation on the Rider</summary>
        public DismountType DismountType = DismountType.Random;

        /// <summary>Mounted Layer Path</summary>
        public string LayerPath = "Layers/Mount v2";
        /// <summary>Mounted Layer Name</summary>
        public string MountLayer = "Mounted";

        [Tooltip("Reference for the Right Hand gameobject")]
        [ContextMenuItem("Find Right Hand", "FindRHand")]
        public Transform RightHand;

        [ContextMenuItem("Find Left Hand", "FindLHand")]
        [Tooltip("Reference for the Left Hand gameobject")]
        public Transform LeftHand;

        [Tooltip("Left Offset to place the Reins in the Left Hand")]
        public Vector3Reference LeftReinOffset = new();

        [Tooltip("Right Offset to place the Reins in the Right Hand")]
        public Vector3Reference RightReinOffset = new();

        private bool freeRightHand = true;
        private bool freeLeftHand = true;

        public readonly static int IKLeftFootHash = Animator.StringToHash("IKLeftFoot");
        public readonly static int IKRightFootHash = Animator.StringToHash("IKRightFoot");
        public readonly static int MountHash = Animator.StringToHash("Mount");
        public readonly static int MountSideHash = Animator.StringToHash("MountSide");
        public static readonly int EmptyHash = Animator.StringToHash("Empty");

        /// <summary>Type to Update to set Everyframe the position and rotation of the rider to the Animal Mount Point</summary>
        [Utilities.Flag("Update Type")]
        public UpdateMode LinkUpdate = UpdateMode.Update | UpdateMode.FixedUpdate;

        /// <summary>Time to Align to the Mount Trigger Position while is playing the Mount Animation</summary>
        public FloatReference AlingMountTrigger = new(0.2f);

        private Hashtable animatorParams;

        public bool debug;

        #region Call Animal

        [Tooltip("Ground Layer to teleport the Mount")]
        public LayerReference Ground = new(1);
        [Tooltip("If the Animal is futher than this radius. It will be teleported near the Rider ")]
        public FloatReference CallRadius = new(30);
        [Tooltip("Distance the Animal will appear near the Rider")]
        public FloatReference TeleportDistance = new(7);
        [Tooltip("Height used to Cast a ray to Find Obstacles for the teleportation")]
        public FloatReference TeleportHeight = new(3f);


        public AudioClip CallAnimalA;
        public AudioClip StopAnimalA;
        public AudioSource RiderAudio;
        /// <summary>Call for  the animal, True: Calls The Animal, False: if the animal was calling then stop its movement</summary>
        public bool ToggleCall { get; set; }
        #endregion

        #region Colliders

        [RequiredField] public CapsuleCollider MainCollider;
        private OverrideCapsuleCollider DefaultCollider;


        public OverrideCapsuleCollider MountCollider;
        // [ExposeScriptableAsset] public CapsuleColliderPreset MountCollider;

        [Tooltip("Internal Rider Colliders. This needs to be disabled while mounting or it cause an error where the rider to push the horse collider")]

        [ContextMenuItem("Find Internal Colliders", "GetExtraColliders")]
        public List<Collider> colliders;
        #endregion

        #region UnityEvents
        public GameObjectEvent OnFindMount = new();
        public BoolEvent OnCanMount = new();
        public BoolEvent OnCanDismount = new();
        public BoolEvent CanCallMount = new();

        public UnityEvent OnStartMounting = new();
        public UnityEvent OnEndMounting = new();
        public UnityEvent OnStartDismounting = new();
        public UnityEvent OnEndDismounting = new();
        #endregion

        public BoolReference DisableComponents;
        public Behaviour[] DisableList = new Behaviour[0];
        #endregion

        #region Auto Properties

        /// <summary>Montura stored when the Riders enters a MountTrigger</summary>
        public Mount Montura { get; set; }

        public GameObject Mount => Montura != null ? Montura.Animal.gameObject : null;

        public virtual IInputSource RiderInput { get; set; }
        public virtual MAnimal RiderAnimal { get; set; }


        /// <summary> If Null means that we are NOT Near to an Animal</summary>
        public MountTriggers MountTrigger { get; set; }
        //{
        //    get => mountTrigger;
        //    protected set
        //    {
        //        mountTrigger = value;
        //        Debug.Log("mountTrigger "+ mountTrigger);
        //    }
        //}
        //MountTriggers mountTrigger;

        public bool CanMount { get => m_CanMount.Value; protected set => m_CanMount.Value = value; }
        public bool CanDismount { get => m_CanDismount.Value; protected set => m_CanDismount.Value = value; }

        /// <summary>Check if we can call the Animal</summary>
        public bool CanCallAnimal { get => m_CanCallAnimal.Value; protected set => m_CanCallAnimal.Value = value; }

        /// <summary> Speed Multiplier for the Speeds Changes while using other Animals</summary>
        public float SpeedMultiplier { get; set; }

        public float TargetSpeedMultiplier { get; set; }

        public bool ForceLateUpdateLink { get; set; }

        /// <summary> Store all the MonoBehaviours on this GameObject</summary>
        protected MonoBehaviour[] AllComponents { get; set; }
        #endregion

        #region IK VARIABLES    
        protected float L_IKFootWeight = 0f;        //IK Weight for Left Foot
        protected float R_IKFootWeight = 0f;        //IK Weight for Right Foot
        #endregion

        /// <summary>Target Rotation the Rider does while Mounting / Dismouting</summary>
        public Quaternion MountRotation { get; set; }

        /// <summary>Target Posttion the Rider does while Mounting / Dismouting</summary>
        public Vector3 MountPosition { get; set; }

        internal int MountLayerIndex = -1;                    //Mount Layer Index

        private bool startGravity;
        protected AnimatorUpdateMode Default_Anim_UpdateMode;

        #region Properties

        /// <summary>Is the Rider Aiming</summary>
        public bool IsAiming { get; set; }

        /// <summary>Sents to all listeners what the Riding System is doing </summary>

        public Action<RiderAction> RiderStatus { get; set; }


        protected bool mounted;
        public bool Mounted
        {
            get => mounted;
            set
            {
                mounted = value;
                //Debug.Log("mounted = " + mounted);
                SetAnimParameter(MountHash, Mounted);                           //Update Mount Parameter on the Animator
            }
        }
        public bool IsOnHorse { get; protected set; }

        public bool IsRiding => IsOnHorse && Mounted;

        /// <summary>Returns true if the Rider is from the Start of the Mount to the End of the Dismount</summary>
        public bool IsMountingDismounting => IsOnHorse || Mounted;

        public bool IsMounting => !IsOnHorse && Mounted;

        /// <summary>Returns true if the Rider is between the Start and the End of the Dismount Animations</summary>
        public bool IsDismounting => IsOnHorse && !Mounted;


        #region private vars
        /// <summary>Straight Spine Weight for the Bones</summary>
        protected float SP_Weight;

        // protected float SAim_Weight;
        protected RigidbodyConstraints DefaultConstraints;
        protected CollisionDetectionMode DefaultCollision;
        #region Re-Sync with Horse
        //Used this for Sync Animators
        private float RiderNormalizedTime;
        private float HorseNormalizedTime;

        [Tooltip("If the Mount and the Rider has a difference on the Animator greater than this value, it will force a resync")]
        public float ResyncThreshold = 0.1f;

        [Tooltip("Sync the Modes from the Rider")]
        public bool SyncModes = false;


        #endregion
        #endregion

        #region References
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody m_rigidBody;

        /// <summary>Reference for the Animator </summary>
        public Animator Anim { get => animator; protected set => animator = value; }  //Reference for the Animator 
        /// <summary>Reference for the rigidbody</summary>
        public Rigidbody RB { get => m_rigidBody; protected set => m_rigidBody = value; }  //Reference for the Animator 

        /// <summary>Root Gameobject for the Rider Character</summary>
        public Transform RiderRoot { get => m_root; protected set => m_root = value; }

        [SerializeField] private Transform m_root;


        #region Bones
        /// <summary>Spine Bone Transform</summary>
        public Transform Spine { get; set; }
        //  public Transform Hips { get; private set; }
        public Transform Chest { get; set; }

        /// <summary>Ground Character Controller</summary>
        public ISleepController Character { get; protected set; }
        #endregion


        #endregion
        #endregion

        /// <summary>  Store All colliders that are enabled and not Triggers </summary>
        private void GetExtraColliders()
        {
            colliders = GetComponentsInChildren<Collider>(false).ToList();

            var CleanCol = new List<Collider>();

            foreach (var col in colliders)
            {
                if (col.enabled && !col.isTrigger)
                    CleanCol.Add(col);
            }

            colliders = new List<Collider>(CleanCol);


            if (MainCollider)
            {
                DefaultCollider = new OverrideCapsuleCollider(MainCollider) { modify = (CapsuleModifier)(-1) };
                colliders.Remove(MainCollider); //Remove the Main Collider from the Extra Colliders
            }

            MTools.SetDirty(this);
        }

        public virtual void Awake()
        {
            if (RiderRoot == null) RiderRoot = transform.FindObjectCore();
            if (Anim == null) Anim = this.FindComponent<Animator>();
            if (RB == null) RB = this.FindComponent<Rigidbody>();

            RiderInput ??= this.FindInterface<IInputSource>();

            Character = GetComponent<ISleepController>(); //Find if there's a ground controller (ANIMAL CONTROLLER)
            RiderAnimal = GetComponent<MAnimal>(); //Find  the Input Controller on the Rider

            animatorParams = new Hashtable();


            if (MainCollider == null && !TryGetComponent(out MainCollider))
            { Debug.LogWarning("The Rider needs a Main Collider on the component", this); }


            if (MainCollider)
                DefaultCollider = new OverrideCapsuleCollider(MainCollider) { modify = (CapsuleModifier)(-1) };


            if (Anim)
            {
                //Store all the Animator parameters
                foreach (AnimatorControllerParameter parameter in Anim.parameters)
                    animatorParams.Add(parameter.nameHash, parameter.name);

                MountLayerIndex = Anim.GetLayerIndex(MountLayer);

                if (MountLayerIndex != -1)
                {
                    Anim.SetLayerWeight(MountLayerIndex, 1); //Just in case
                    Anim.Play(EmptyHash, MountLayerIndex, 0);
                }
                Spine = Anim.GetBoneTransform(HumanBodyBones.Spine);                   //Get the Rider Spine transform
                //Hips = Anim.GetBoneTransform(HumanBodyBones.Hips);                   //Get the Rider Hips transform
                Chest = Anim.GetBoneTransform(HumanBodyBones.Chest);                   //Get the Rider Chest transform

                Default_Anim_UpdateMode = Anim.updateMode;                             //Gets the Update Mode of the Animator to restore later when dismounted.
            }

            GetExtraColliders();

            IsOnHorse = Mounted = false;
            ForceLateUpdateLink = false;
            SpeedMultiplier = 1f;
            TargetSpeedMultiplier = 1f;

            if ((int)LinkUpdate == 0 || !Parent)
                LinkUpdate = UpdateMode.FixedUpdate | UpdateMode.LateUpdate;


            startGravity = RB.useGravity;
            DefaultKinematic = RB.isKinematic;

            FindStoredMount();
        }


        protected virtual void OnEnable()
        {
            FindStoredMount();
            if (StartMounted.Value || Mounted) Start_Mounted();

            UpdateCanMountDismount();
        }

        void Update()
        {
            if ((LinkUpdate & UpdateMode.Update) == UpdateMode.Update) UpdateRiderTransform();
        }
        private void LateUpdate()
        {
            if ((LinkUpdate & UpdateMode.LateUpdate) == UpdateMode.LateUpdate || ForceLateUpdateLink) UpdateRiderTransform();
        }

        private void FixedUpdate()
        {
            if ((LinkUpdate & UpdateMode.FixedUpdate) == UpdateMode.FixedUpdate) UpdateRiderTransform();
        }

        /// <summary>Updates the Rider Position to the Mount Point</summary>
        public virtual void UpdateRiderTransform()
        {
            if (IsRiding)
            {
                transform.SetPositionAndRotation(Montura.MountPoint.position, Montura.MountPoint.rotation);

                //Update the Mount Position/Rotation Also
                MountRotation = transform.rotation;
                MountPosition = transform.position;
            }
        }

        /// <summary>Add the Mount Rotation and Position Modifications while Mounting Dismounting called y the animator</summary>
        public virtual void Mount_TargetTransform(Vector3 pos, Quaternion rot)
        {
            MountPosition = pos;
            MountRotation = rot;

            transform.SetPositionAndRotation(MountPosition, MountRotation);
        }

        void Debbuging(string value, string color = "white")
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<b>{name}:<color={color}> [{value}] </color> </b>", this);  //Debug
#endif
        }

        /// <summary>Set the Mount Index Value</summary>
        internal void SetMountSide(int side) => SetAnimParameter(MountSideHash, side);

        public virtual void MountAnimal()
        {
            if (!CanMount || !enabled || Montura == null) return;

            //Check if the Mount is already mounted by someone else
            if (Montura.Mounted && Montura.Rider != this)
            {
                Montura = null;
                MountTrigger = null;
                Debug.Log("Mountura is already being used");
                return;
            }


            if (!Montura.InstantMount)                                           //If is instant Mount play it      
            {
                Debbuging("Mount Animal", "cyan");
                Mounted = true;                                                  //Update MountSide Parameter In the Animator
                SetMountSide(MountTrigger.MountID);                              //Update MountSide Parameter In the Animator
                // Anim?.Play(MountTrigger.MountAnimation, MountLayerIndex);      //Play the Mounting Animations
            }
            else
            {
                Debbuging("Instant Mount", "cyan");

                Anim?.Play(Montura.MountIdle, MountLayerIndex);                //Ingore the Mounting Animations
                Anim?.Update(0);                             //Update the Animator ????

                Start_Mounting();
                End_Mounting();
            }
        }

        public virtual void DismountAnimal()
        {
            if (!CanDismount || !enabled) return;

            Debbuging("Dismount Animal", "cyan");

            Montura.Mounted = Mounted = false;                                  //Unmount the Animal
            MountTrigger = GetDismountTrigger();

            SetMountSide(MountTrigger.DismountID);                               //Update MountSide Parameter In the Animator

            if (Montura.InstantDismount)                                         //Use for Instant Dismount
            {
                Anim.Play(EmptyHash, MountLayerIndex);
                SetMountSide(0);                                                //Update MountSide Parameter In the Animator

                Start_Dismounting();

                var MT = Montura.MountTriggers.Find(x => x.MountID.Value == MountTrigger.DismountID);
                End_Dismounting();

                //Move the rider directly to the mounttrigger
                RiderRoot.SetPositionAndRotation(MT.transform.position + (MT.transform.forward * -0.2f), MT.transform.rotation);
            }
        }

        public void DismountAnimal(int dismountID)
        {
            if (!CanDismount || !enabled) return;
            Debbuging("Dismount Animal force DismountID " + dismountID, "cyan");
            Montura.Mounted = Mounted = false;

            if (dismountID > 0 && dismountID < Montura.MountTriggers.Count)
                MountTrigger = Montura.MountTriggers[dismountID];

            SetMountSide(dismountID);
        }


        /// <summary>Return the Correct Mount Trigger using the DismountType</summary>
        protected MountTriggers GetDismountTrigger()
        {
            switch (DismountType)
            {
                case DismountType.Last:
                    if (MountTrigger == null) MountTrigger = Montura.MountTriggers[UnityEngine.Random.Range(0, Montura.MountTriggers.Count)];
                    return MountTrigger;
                case DismountType.MountInput:
                    return MountTriggerByInput(Montura.Animal.RawInputAxis);

                case DismountType.Random:
                    int Randomindex = UnityEngine.Random.Range(0, Montura.MountTriggers.Count);
                    return Montura.MountTriggers[Randomindex];

                case DismountType.RiderInput:
                    return MountTriggerByInput(RiderAnimal.RawInputAxis);
                default:
                    return MountTrigger;
            }


            //Local function to find the Mount Trigger by Input
            MountTriggers MountTriggerByInput(Vector3 MoveInput)
            {
                var close = MountTrigger;
                var Diference = Vector3.Angle(MountTrigger.Direction, MoveInput);

                foreach (var mt in Montura.MountTriggers)
                {
                    var newDiff = Vector3.Angle(mt.Direction, MoveInput);

                    if (newDiff < Diference)
                    {
                        Diference = newDiff;
                        close = mt;
                    }
                }
                return close;
            }
        }

        private void FindStoredMount()
            => MountStored = m_MountStored.Value != null ? m_MountStored.Value.FindComponent<Mount>() : null;


        /// <summary> Set a horse with a Mount Component to be the stored mount. Necessary to call a horse using call animal</summary> 
        public virtual void Set_StoredMount(GameObject newMount)
        {
            m_MountStored.Value = newMount;
            FindStoredMount();
        }

        public virtual void ClearStoredMount()
        {
            m_MountStored.Value = null;
            MountStored = null;
        }

        public virtual void Start_Mounted(GameObject mount)
        {
            StartMounted.Value = mount;
            FindStoredMount();
            Start_Mounted();
        }

        /// <summary>Set all the correct atributes and variables to Start Mounted on the next frame</summary>
        public virtual void Start_Mounted()
        {
            if (MountStored != null && m_MountStored.Value.activeSelf)
            {
                if (m_MountStored.Value.IsPrefab()) //If the Stored Mount is a Prefab Instantiate it at the back of the Rider
                    m_MountStored.Value = Instantiate(m_MountStored.Value, transform.position - transform.forward, Quaternion.identity);

                Montura = MountStored;  //Set on the Rider which mount is using

                Debbuging("Start Mounted", "green");

                // StopMountAI();

                Montura.Rider = this;   //Set on the Mount which Rider is using it

                if (MountTrigger == null)
                    MountTrigger = Montura.transform.GetComponentInChildren<MountTriggers>(); //Save the first Mount trigger you found


                Start_Mounting();
                End_Mounting();

                Anim?.Play(Montura.MountIdle, MountLayerIndex);               //Play Mount Idle Animation Directly

                Montura.Mounted = Mounted = true;                             //Send to the animalMount that mounted is active

                UpdateRiderTransform();
            }
            else
            {
                Debug.Log("There's no stored Mount or the Stored Mount GameObject has no Mount component");
            }
        }

        /// <summary>Force the Rider to Dismount</summary>
        public virtual void ForceDismount()
        {
            if (Montura && IsRiding)
            {
                Debbuging("Force Dismount", "green");
                DisconnectWithMount();
                Anim?.Play(EmptyHash, MountLayerIndex);
                SetMountSide(0);                                //Update MountSide Parameter In the Animator
                Start_Dismounting();
                End_Dismounting();
            }
        }



        /// <summary>CallBack at the Start of the Mount Animations</summary>
        public virtual void Start_Mounting()
        {
            Montura.StartMounting(this);         //Sync Mounted Values in Animal and Rider    

            IsOnHorse = false;
            Mounted = true;                       //Sync Mounted Values in Animal and Rider

            if (RB)                                                 //Deactivate stuffs for the Rider's Rigid Body
            {
                RB.useGravity = false;
                DefaultConstraints = RB.constraints;                //Store the Contraints before mounting
                DefaultCollision = RB.collisionDetectionMode;       //Store the Contraints before mounting
                RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                RB.constraints = RigidbodyConstraints.FreezeAll;
                if (kinematicRider.Value) RB.isKinematic = true;
            }

            ToogleColliders(false);            //Deactivate All Colliders on the Rider IMPORTANT ... or the Rider will try to push the animal

            if (MainCollider && !MountCollider.IsNull)
            {
                MountCollider.Modify(MainCollider); //Modify the collider properties
                MainCollider.enabled = false;
            }

            ToggleCall = false;                                     //Set the Call to Stop Animal
            CallAnimal(false);                                      //If is there an animal following us stop him

            m_MountStored.Value = Montura.Animal.gameObject;        //Store the last animal you mounted
            MountStored = Montura;

            if (Parent) SetRiderParent(Montura.MountPoint);

            if (!MountTrigger)
                MountTrigger = Montura.GetComponentInChildren<MountTriggers>();         //If null add the first mount trigger founded

            if (DisableComponents)
                ToggleComponents(false);                                                //Disable all Monobehaviours breaking the Riding System

            OnStartMounting.Invoke();                                                   //Invoke UnityEvent for  Start Mounting
            RiderStatus?.Invoke(RiderAction.StartMount);

            if (Character != null)
            {
                Character.Sleep = true;   //IF there's an Animal Controller send it to Sleep.
                Character.Grounded = false;
            }

            if (RiderInput != null) RiderInput.MoveCharacter = false; //Stop the Movement of the Rider

            UpdateCanMountDismount();

            Debbuging("Start Mounting", "green");

            //Sync the States to the Rider after mounting
            SetAnimParameter(Montura.Animal.hash_Grounded, Montura.Animal.Grounded);
            SetAnimParameter(Montura.Animal.hash_StateOn);

            //Debug.Log(Montura.Animal.ActiveStateID);


            SetAnimParameter(Montura.Animal.hash_State, Montura.Animal.ActiveStateID.ID);

        }

        /// <summary>CallBack at the End of the Mount Animations </summary>
        public virtual void End_Mounting()
        {
            IsOnHorse = true;                              //Sync Mounted Values in Animal and Rider again Double Check

            if (Parent)
            {
                //Reset Rotation when PARENTED
                RiderRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            if (Anim)
            {
                Anim.updateMode = Montura.Anim.updateMode;                              //Use the Same UpdateMode from the Animal

                SetAnimParameter(Montura.Animal.hash_Grounded, Montura.Animal.Grounded);
                SetAnimParameter(Montura.Animal.hash_State, Montura.Animal.ActiveStateID.ID);
                SetAnimParameter(Montura.Animal.hash_Mode, Montura.Animal.ModeAbility);
                SetAnimParameter(Montura.Animal.hash_ModeStatus, Montura.Animal.ModeStatus);
                SetAnimParameter(Montura.Animal.hash_Stance, Montura.ID);

                Anim.speed = Montura.Anim.speed; //In case the Mount is not using Speed Modifiers
                ConnectWithMount();
            }

            OnEndMounting.Invoke();
            Montura.End_Mounting();

            //Broadcast that Mount has finished
            RiderStatus?.Invoke(RiderAction.EndMount);


            if (MainCollider)
            {
                if (!MountCollider.IsNull) MountCollider.Modify(MainCollider); //Modify the collider properties
                MainCollider.enabled = true;
            }

            UpdateCanMountDismount();

            SendMessage("SetIgnoreTransform", Montura.Animal.transform, SendMessageOptions.DontRequireReceiver);

            Debbuging("End Mounting", "green");
        }

        /// <summary> CallBack at the Start of the Dismount Animations</summary>
        public virtual void Start_Dismounting()
        {
            if (Parent) SetRiderParent(null);               //Unparent! Important!

            Montura.Start_Dismounting();
            Mounted = false;

            if (Anim)
            {
                Anim.updateMode = Default_Anim_UpdateMode;                               //Restore Update mode to its original

                SetAnimParameter(Montura.Animal.hash_Stance, 0); //Reset  the Stance.
                SetAnimParameter(Montura.Animal.hash_Mode, 0);
                SetAnimParameter(Montura.Animal.hash_ModeStatus, 0);
                ResetAnimParameter(Montura.Animal.hash_ModeOn);
                ResetAnimParameter(Montura.Animal.hash_StateOn);

                DisconnectWithMount();
                Anim.speed = 1f;
            }

            OnStartDismounting.Invoke();
            RiderStatus?.Invoke(RiderAction.StartDismount);

            UpdateCanMountDismount();

            SendMessage("ClearIgnoreTransform", SendMessageOptions.DontRequireReceiver); //Clear the Ingore Transform on the Weapon Manager

            Debbuging("Start Dismounting", "green");
        }

        /// <summary>CallBack at the End of the Dismount Animations</summary>
        public virtual void End_Dismounting()
        {
            IsOnHorse = false;                              //Is no longer on the Animal

            if (Montura) Montura.EndDismounting();             //Disable Montura Logic

            Montura = null;                                 //Reset the Montura
            MountTrigger = null;                            //Reset the Active Mount Trigger
            ToggleCall = false;                             //Reset the Call Animal

            //Reactivate stuffs for the Rider's Rigid Body
            if (RB)
            {
                RB.isKinematic = DefaultKinematic;
                RB.useGravity = startGravity;
                RB.constraints = DefaultConstraints;
                RB.collisionDetectionMode = DefaultCollision;
            }

            if (Anim)
            {
                Anim.speed = 1;                            //Reset AnimatorSpeed
                MTools.ResetFloatParameters(Anim);
            }

            //Reset the Up Vector; ****IMPORTANT 
            RiderRoot.rotation = Quaternion.FromToRotation(RiderRoot.up, -Gravity.Value) * RiderRoot.rotation;

            DefaultCollider.Modify(MainCollider);             //Restore Main Collider
            ToogleColliders(true);                              //Enabled Rider  Colliders

            if (DisableComponents) ToggleComponents(true);      //Enable all Monobehaviours breaking the Mount System


            UpdateCanMountDismount();
            OnEndDismounting.Invoke();                                                      //Invoke UnityEvent when is off Animal
            RiderStatus?.Invoke(RiderAction.EndDismount);

            if (RiderInput != null) RiderInput.MoveCharacter = true; //Start the Movement of the Rider

            if (Character != null)
            {
                Character.Sleep = false;
                Character.Grounded = true;

                //Little Hack for the new  Inputs
                SendMessage("ResetInputAxis", SendMessageOptions.DontRequireReceiver);
            }
            Debbuging("End Dismounting", "green");
        }

        /// <summary>Connect the Animal Events from the Riders Methods (Grounded, State, Mode)</summary>
        protected virtual void ConnectWithMount()
        {
            if (Anim)
            {
                Montura.Animal.SetBoolParameter += SetAnimParameter;
                Montura.Animal.SetIntParameter += SetAnimParameter;
                Montura.Animal.SetFloatParameter += SetAnimParameter;
                Montura.Animal.SetTriggerParameter += SetAnimParameter;
                if (ReSync) Montura.Animal.StateCycle += Animators_Locomotion_ReSync;
            }
        }

        /// <summary>Disconnect the Animal Events from the Riders Methods (Grounded, State, Mode)</summary>
        protected virtual void DisconnectWithMount()
        {
            if (Anim)
            {
                Montura.Animal.SetBoolParameter -= SetAnimParameter;
                Montura.Animal.SetIntParameter -= SetAnimParameter;
                Montura.Animal.SetFloatParameter -= SetAnimParameter;
                Montura.Animal.SetTriggerParameter -= SetAnimParameter;

                if (ReSync) Montura.Animal.StateCycle -= Animators_Locomotion_ReSync;
            }
        }

        public virtual void MountTriggerEnter(Mount mount, MountTriggers mountTrigger)
        {
            Montura = mount;                                   //Set to Mount on this Rider    
            MountTrigger = mountTrigger;                       //Send the side transform to mount
            OnFindMount.Invoke(mount.Animal.gameObject);       //Invoke Found Mount
            RiderStatus?.Invoke(RiderAction.InMountTrigger);


            if (!mountTrigger.AutoMount)
                Montura.OnCanBeMounted.Invoke(Montura.CanBeMountedByState); //Invoke Can Be mounted to true ???

            Montura.NearbyRider = this;

            UpdateCanMountDismount();
        }

        public virtual void MountTriggerExit()
        {
            if (Montura)
                Montura.ExitMountTrigger();

            MountTrigger = null;
            Montura = null;
            OnFindMount.Invoke(null); ////Invoke Null Mount
            RiderStatus?.Invoke(RiderAction.OutMountTrigger);

            UpdateCanMountDismount();
        }


        protected virtual void SetRiderParent(Transform target)
        {
            RiderRoot.parent = target;
        }

        /// <summary> Update the values Can Mount Can Dismount </summary>
        public virtual void UpdateCanMountDismount()
        {
            CanMount = Montura && !Mounted && !IsOnHorse && Montura.CanBeMountedByState;

            OnCanMount.Invoke(CanMount);

            bool canDismount = IsRiding && Montura && Montura.CanBeDismountedByState;
            CanDismount = canDismount;
            OnCanDismount.Invoke(CanDismount);


            bool canCallAnimal = !Montura && !Mounted && !IsOnHorse && m_MountStored.Value != null;

            if (canCallAnimal != CanCallAnimal)
            {
                CanCallAnimal = canCallAnimal;
                CanCallMount.Invoke(CanCallAnimal);
            }
        }

        /// <summary> Syncronize the Animal/Rider animations if Rider loose sync with the animal on the locomotion state </summary>
        protected virtual void Animators_Locomotion_ReSync(int CurrentState)
        {
            if (!Anim || MountLayerIndex == -1) return;
            if (Montura.Animal.Stance != 0) return;     //Skip if the we are not on the default stance                                                       
            if (Montura.ID != 0) return;                // if is not the Horse (Wagon do not sync )                                                        

            if (Anim.IsInTransition(MountLayerIndex) || Montura.Anim.IsInTransition(0)) return; //Do not Resync when is  

            //Search for syncron the locomotion state on the animal. resync on Locomotion, Fly, Swim
            if (MTools.CompareOR(CurrentState, StateEnum.Locomotion, StateEnum.Fly, StateEnum.Swim))
            {
                var HorseStateInfo = Montura.Animal.Anim.GetCurrentAnimatorStateInfo(0);
                var RiderStateInfo = Anim.GetCurrentAnimatorStateInfo(MountLayerIndex);

                HorseNormalizedTime = HorseStateInfo.normalizedTime;            //Get the normalized time from the Rider
                RiderNormalizedTime = RiderStateInfo.normalizedTime;            //Get the normalized time from the Horse

                var Diff = Mathf.Abs(HorseNormalizedTime - RiderNormalizedTime); //Calculate the difference on the syncronization

                if (Diff >= ResyncThreshold)   //Checking if the animal and the rider are unsync by the threshold
                {
                    Anim.CrossFade(RiderStateInfo.fullPathHash, 0.2f, MountLayerIndex, HorseNormalizedTime);                 //Normalized with blend
                    if (debug) Debug.Log($"Re-sync [Mount:{HorseNormalizedTime:F3}]  [Rider:{RiderNormalizedTime:F3}].  [Diff: {Diff:F3}]");
                }
            }
            else
            {
                RiderNormalizedTime = HorseNormalizedTime = 0;
            }
        }

        /// <summary> If the Animal has a IMountAI component it can be called</summary>
        public virtual void CallAnimal(bool call)
        {
            if (CanCallAnimal)
            {
                ToggleCall = call;

                if (m_MountStored.Value.IsPrefab()) //Insantiate a new Horse Behind the Rider...(Weird Option but it works for now)
                {
                    //If there's an old Mount... Stop it
                    if (MountStored)
                    {
                        Debbuging($"Old Stored Mount Stop {MountStored.Animal.name}", "yellow");
                        MountStored.AI?.ClearTarget();
                        MountStored.AI?.Stop();
                    }



                    MountStored = m_MountStored.Value.FindComponent<Mount>();

                    if (MountStored)
                    {
                        Debbuging($"Mount Instantiated!  [{MountStored.Animal.name}]", "yellow");
                    }
                    else
                    {
                        Debbuging($"Stored mount does not contain any Mount Component.", "yellow");
                        return;
                    }

                    Vector3 InstantiatePosition = TeleportRaycast();

                    var InsMount = Instantiate(m_MountStored.Value, InstantiatePosition, Quaternion.identity);

                    InsMount.name = InsMount.name.Replace("(Clone)", "");

                    m_MountStored.UseConstant = true;
                    m_MountStored.Value = InsMount;

                    MountStored = InsMount.FindComponent<Mount>();

                    ToggleCall = true;

                }
                else
                {
                    if (!MountStored)
                    {
                        MountStored = m_MountStored.Value.FindComponent<Mount>();

                        if (!MountStored)
                        {
                            Debbuging($"Stored mount does not contain any Mount Component.");  //Debug
                            return;
                        }
                    }
                }


                Debbuging($"Calling Animal: ToggleCall = <{ToggleCall}>");  //Debug

                if (MountStored.AI != null && MountStored.AI.Active)
                {
                    if (ToggleCall)
                    {
                        if (Vector3.Distance(transform.position, MountStored.Animal.transform.position) > CallRadius)
                        {
                            Vector3 InstantiatePosition = TeleportRaycast();


                            MountStored.Animal.Teleport(InstantiatePosition);
                        }


                        MountStored.AI.SetActive(true);
                        MountStored.AI.SetTarget(RiderRoot, true); //Set the Rider as the Target to follow
                        MountStored.AI.Move(); //Move the Animal 

                        if (CallAnimalA != null && RiderAudio != null)
                            RiderAudio.PlayOneShot(CallAnimalA);

                        MountStored.OnCalled.Invoke(RiderRoot.gameObject);

                        RiderStatus?.Invoke(RiderAction.CallMount);
                    }
                    else
                    {
                        StopMountAI();

                        if (StopAnimalA != null && RiderAudio != null)
                            RiderAudio.PlayOneShot(StopAnimalA);

                        RiderStatus?.Invoke(RiderAction.CallMountStop);
                    }
                }
            }
        }

        /// <summary>Raycast to find the position to instantiate the new mount in the back of the rider</summary>
        protected virtual Vector3 TeleportRaycast()
        {
            var UpVector = -Gravity.Value;
            var InstantiatePosition = transform.position - (transform.forward * 4);
            var HeightPoint = (transform.position + UpVector * TeleportHeight);
            var NextPoint = (HeightPoint + (-transform.forward * TeleportDistance));

            Debug.DrawRay(HeightPoint, -transform.forward * TeleportDistance, Color.white, 3f);
            Debug.DrawRay(HeightPoint + (-transform.forward * TeleportDistance), (2 * TeleportHeight * Gravity.Value), Color.white, 3f);

            //If we did not hit anything
            if (Physics.Raycast(HeightPoint, -transform.forward, out var hit0, TeleportDistance, Ground.Value))
            {
                NextPoint = hit0.point;
            }

            if (Physics.Raycast(NextPoint - (transform.forward * 0.1f), Gravity.Value, out var hit2, TeleportHeight * 2, Ground.Value))
            {
                InstantiatePosition = hit2.point;
            }


            return InstantiatePosition;
        }

        public virtual void StopMountAI()
        {
            if (MountStored != null && MountStored.AI != null)
            {
                MountStored.AI.Stop();
                MountStored.AI.ClearTarget();
            }
        }

        public virtual void CallAnimalToggle()
        {
            if (CanCallAnimal)
            {
                ToggleCall ^= true;
                CallAnimal(ToggleCall);
            }
        }


        /// <summary>Enable/Disable The  Colliders in this gameobject </summary>
        protected virtual void ToogleColliders(bool active)
        {
            foreach (var col in colliders)
            {
                if (col != null) col.enabled = active;
            }
        }

        /// <summary>Toogle the MonoBehaviour Components Attached to this game Objects but the Riders Scripts </summary>
        protected virtual void ToggleComponents(bool enabled)
        {
            if (DisableList.Length == 0)
            {
                foreach (var component in AllComponents)
                {
                    if (component != null)
                    {
                        if (component is MRider) continue; //Do not Disable or enable Rider
                        component.enabled = enabled;
                    }
                }
            }
            else
            {
                foreach (var component in DisableList)
                {
                    if (component != null)
                        component.enabled = enabled;
                }
            }
        }

        #region Set Animator Parameters
        /// <summary>Set a Int on the Animator</summary>
        public void SetAnimParameter(int hash, int value)
        {
            //Do not set the Mode on the Rider
            if (Montura && !SyncModes)
            {
                if (hash == Montura.Animal.hash_Mode || hash == Montura.Animal.hash_ModeStatus) return;
            }


            if (HasParam(hash)) Anim.SetInteger(hash, value);

        }

        /// <summary>Set a float on the Animator</summary>
        public void SetAnimParameter(int hash, float value)
        {
            ////Do not set the Mode on the Rider
            //if (Montura && !SyncModes)
            //{
            //    if (hash == Montura.Animal.hash_ModePower) return;
            //}


            if (HasParam(hash)) Anim.SetFloat(hash, value);

        }

        /// <summary>Set a Bool on the Animator</summary>
        public void SetAnimParameter(int hash, bool value) { if (HasParam(hash)) Anim.SetBool(hash, value); }

        /// <summary>Set a Trigger on the Animator</summary>
        public void SetAnimParameter(int hash)
        {
            //Do not set the Mode on the Rider
            if (Montura && !SyncModes && hash == Montura.Animal.hash_ModeOn) return;


            if (HasParam(hash)) Anim.SetTrigger(hash);

        }
        public void ResetAnimParameter(int hash) { if (HasParam(hash)) Anim.ResetTrigger(hash); }
        private bool HasParam(int hash) => animatorParams.ContainsKey(hash);
        #endregion

        /// <summary> Checks and Execute  without Input if the Rider can Mount, Dismount or Call an Animal </summary>
        public virtual void CheckMountDismount()
        {
            UpdateCanMountDismount();

            if (CanMount) MountAnimal();              //if are near an animal and we are not already on an animal//Run mounting Animations
            else if (CanDismount) DismountAnimal();           //if we are already mounted and the animal is not moving (Mounted && IsOnHorse && Montura.CanDismount)//Run Dismounting Animations
            else if (CanCallAnimal) CallAnimalToggle();         //if there is no horse near, call the animal stored
        }

        /// <summary>IK Feet Adjustment while mounting</summary>
        void OnAnimatorIK()
        {
            if (Anim == null) return;           //If there's no animator skip

            IKFeet();

            IK_Reins();

            SolveStraightMount();
        }

        private void IK_Reins()
        {
            if (IsRiding)
            {
                if (Montura && LeftHand && RightHand)
                {
                    var New_L_ReinPos = LeftHand.TransformPoint(LeftReinOffset);
                    var New_R_ReinPos = RightHand.TransformPoint(RightReinOffset);

                    if (!freeLeftHand && !freeRightHand) //When Both hands are free
                    {
                        Montura.ResetLeftRein();
                        Montura.ResetRightRein();
                        return;
                    }

                    if (Montura.LeftRein)
                    {
                        if (freeLeftHand)
                        {
                            Montura.LeftRein.position = New_L_ReinPos;     //Put it in the middle o the left hand
                        }
                        else
                        {
                            if (freeRightHand)
                            {
                                Montura.LeftRein.position = New_R_ReinPos; //if the right hand is holding a weapon put the right rein to the Right hand
                            }
                        }
                    }
                    if (Montura.RightRein)
                    {
                        if (freeRightHand)
                        {
                            Montura.RightRein.position = New_R_ReinPos; //Put it in the middle o the RIGHT hand
                        }
                        else
                        {
                            if (freeLeftHand)
                            {
                                Montura.RightRein.position = New_L_ReinPos; //if the right hand is holding a weapon put the right rein to the Left hand
                            }
                        }
                    }
                }
            }
        }

        //FIX THIS!!!!
        private void SolveStraightMount()
        {
            if (IsRiding && !IsAiming) //AIM IS IMPORTANT!!! CANNOT HAVE STRAIGHT SPINE WHILE AIMING
            {
                if (Montura.StraightSpine)
                {
                    SP_Weight = Mathf.MoveTowards(SP_Weight, Montura.StraightSpine ? 1 : 0, Montura.Animal.DeltaTime * Montura.smoothSM / 2);
                }
                else
                {
                    SP_Weight = Mathf.MoveTowards(SP_Weight, 0, Montura.Animal.DeltaTime * Montura.smoothSM / 2);
                }

                if (SP_Weight != 0)
                {
                    //if (Montura.MountBase)
                    //{
                    //    var targetRot = Quaternion.FromToRotation(Montura.MountBase.up, Montura.Animal.UpVector) * Montura.MountBase.rotation;
                    //    Montura.MountBase.rotation = Quaternion.Lerp(Montura.MountBase.rotation, targetRot, SP_Weight);
                    //}

                    Anim.SetLookAtPosition(Montura.MonturaSpineOffset);
                    Anim.SetLookAtWeight(SP_Weight, 0.6f, 1);
                }
            }
        }

        private void IKFeet()
        {
            if (useIKFeet && Montura)
            {
                //linking the weights to the animator
                if (IsMountingDismounting)
                {
                    L_IKFootWeight = 1f;
                    R_IKFootWeight = 1f;

                    if (IsMounting || IsDismounting)
                    {
                        L_IKFootWeight = Anim.GetFloat(IKLeftFootHash);
                        R_IKFootWeight = Anim.GetFloat(IKRightFootHash);
                    }

                    //setting the weight
                    Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, L_IKFootWeight);
                    Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, R_IKFootWeight);

                    Anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, L_IKFootWeight);
                    Anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, R_IKFootWeight);

                    //Knees
                    Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, L_IKFootWeight);
                    Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, R_IKFootWeight);

                    //Set the IK Positions
                    Anim.SetIKPosition(AvatarIKGoal.LeftFoot, Montura.FootLeftIK.position);
                    Anim.SetIKPosition(AvatarIKGoal.RightFoot, Montura.FootRightIK.position);

                    //Knees
                    Anim.SetIKHintPosition(AvatarIKHint.LeftKnee, Montura.KneeLeftIK.position);    //Position
                    Anim.SetIKHintPosition(AvatarIKHint.RightKnee, Montura.KneeRightIK.position);  //Position

                    Anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, L_IKFootWeight);   //Weight
                    Anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, R_IKFootWeight);  //Weight

                    //setting the IK Rotations of the Feet
                    Anim.SetIKRotation(AvatarIKGoal.LeftFoot, Montura.FootLeftIK.rotation);
                    Anim.SetIKRotation(AvatarIKGoal.RightFoot, Montura.FootRightIK.rotation);
                }
                else
                {
                    Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                    Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);

                    Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                    Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
                }
            }
        }

        /// <summary>Used for listening Message behaviour from the Animator</summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);


        /// <summary> Enable Disable the Input for the Mount</summary>
        public virtual void EnableMountInput(bool value) => Montura?.EnableInput(value);

        /// <summary> Enable Disable an Input for the Mount</summary>
        public void DisableMountInput(string input) => Montura.MountInput?.DisableInput(input);

        /// <summary> Enable Disable an Input for the Mount</summary>
        public void EnableMountInput(string input) => Montura.MountInput?.EnableInput(input);


        #region IKREINS
        /// <summary>Free the Right Hand (True :The reins will not be on the Hand)</summary>
        public void ReinRightHand(bool value)
        {
            if (Montura != null)
            {
                freeRightHand = value;
                if (freeRightHand) Montura.ResetRightRein();
                //   Debug.Log("freeRightHand = " + freeRightHand);
            }
        }


        /// <summary>Free the Left Hand (True :The reins will not be on the Hand)</summary>
        public void ReinLeftHand(bool value)
        {
            if (Montura != null)
            {
                freeLeftHand = value;
                if (freeLeftHand) Montura.ResetLeftRein();
                //Debug.Log("freeLeftHand = " + freeLeftHand);
            }
        }

        /// <summary>No Weapons is on the Hands so put the Reins on the Hands</summary>
        public void FreeBothHands()
        {
            ReinRightHand(false);
            ReinLeftHand(false);
        }

        /// <summary>Grab Reins with both Hands</summary>
        public void GrabReinsBothHands()
        {
            ReinRightHand(true);
            ReinLeftHand(true);
        }

        /// <summary>Release Reins from both Hands</summary>
        public void ReleaseReinsBothHands()
        {
            ReinRightHand(false);
            ReinLeftHand(false);
        }

        public void WeaponInHands()
        {
            ReinRightHand(true);
            ReinLeftHand(true);
        }
        #endregion

        #region Inspector Stuffs
#if UNITY_EDITOR
        private void OnValidate()
        {
            //if (MountCollider == null)
            //    MountCollider = Resources.Load<CapsuleColliderPreset>("Mount_Capsule");

            if (m_root == null)
                m_root = transform;

            if (Anim == null)
                Anim = GetComponent<Animator>();

            if (RB == null)
                RB = GetComponent<Rigidbody>();
        }

        private void Reset()
        {
            animator = this.FindComponent<Animator>();
            RB = this.FindComponent<Rigidbody>();
            RiderRoot = transform; //IMPORTANT

            MainCollider = GetComponent<CapsuleCollider>();



            BoolVar CanMountV = MTools.GetInstance<BoolVar>("Can Mount");
            BoolVar CanDismountV = MTools.GetInstance<BoolVar>("Can Dismount");
            BoolVar CanCallMountV = MTools.GetInstance<BoolVar>("Can Call Mount");


            MEvent CanMountE = MTools.GetInstance<MEvent>("Rider Can Mount");
            MEvent CanDismountE = MTools.GetInstance<MEvent>("Rider Can Dismount");
            MEvent RiderMountUI = MTools.GetInstance<MEvent>("Rider Mount UI");

            MEvent CanCallMountE = MTools.GetInstance<MEvent>("Rider Can Call Mount");

            MEvent RiderisRiding = MTools.GetInstance<MEvent>("Rider is Riding");
            MEvent SetCameraSettings = MTools.GetInstance<MEvent>("Set Camera Settings");
            BoolVar RCWeaponInput = MTools.GetInstance<BoolVar>("RC Weapon Input");

            m_CanCallAnimal.Variable = CanCallMountV;
            m_CanCallAnimal.UseConstant = false;

            m_CanMount.Variable = CanMountV;
            m_CanMount.UseConstant = false;

            m_CanDismount.Variable = CanDismountV;
            m_CanDismount.UseConstant = false;

            OnCanMount = new BoolEvent();
            OnCanDismount = new BoolEvent();
            CanCallMount = new BoolEvent();
            OnStartMounting = new UnityEvent();
            OnEndMounting = new UnityEvent();
            OnStartMounting = new UnityEvent();
            OnStartDismounting = new UnityEvent();

            // if (CanMountV != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanMount, CanMountV.SetValue);
            if (CanMountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanMount, CanMountE.Invoke);

            // if (CanDismountV != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanDismount, CanDismountV.SetValue);
            if (CanDismountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCanDismount, CanDismountE.Invoke);

            //  if (CanCallMountV != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount, CanCallMountV.SetValue);
            if (CanCallMountE != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount, CanCallMountE.Invoke);

            if (RiderMountUI != null) UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartMounting, RiderMountUI.Invoke, false);

            if (RiderisRiding != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnEndMounting, RiderisRiding.Invoke, true);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartDismounting, RiderisRiding.Invoke, false);
            }

            if (SetCameraSettings != null) UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(OnStartDismounting, SetCameraSettings.Invoke, transform);

            if (RCWeaponInput != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnStartDismounting, RCWeaponInput.SetValue, false);
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(OnEndMounting, RCWeaponInput.SetValue, true);
            }
        }

        ///Editor Variables
        [SerializeField] private int Editor_Tabs1;
        [SerializeField] private bool EditOffsetReinLeft, EditOffsetReinRight;

        [ContextMenu("Create Mount Inputs")]
        void ConnectToInput()
        {
            MInput input = GetComponent<MInput>() ?? gameObject.AddComponent<MInput>();


            #region Mount Input
            var mountInput = input.FindInput("Mount");

            if (mountInput == null)
            {
                mountInput = new InputRow("Mount", "Mount", KeyCode.F, InputButton.Down, InputType.Key);
                input.inputs.Add(mountInput);

                //mountInput.active.Variable = MTools.GetInstance<BoolVar>("Can Mount");
                //mountInput.active.UseConstant = false;


                //Connect the Dismount Input
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnStartMounting, input.DisableInput, mountInput.Name);
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnEndDismounting, input.EnableInput, mountInput.Name);

                UnityEditor.Events.UnityEventTools.AddPersistentListener(mountInput.OnInputDown, MountAnimal);


                Debug.Log("<B>Mount</B> Input created and connected to Rider.MountAnimal");
            }
            #endregion

            #region Dismount Input


            var DismountInput = input.FindInput("Dismount");

            if (DismountInput == null)
            {
                DismountInput = new InputRow("Dismount", "Dismount", KeyCode.F, InputButton.LongPress, InputType.Key);

                DismountInput.LongPressTime = 0.2f;

                input.inputs.Add(DismountInput);


                DismountInput.Active = false; //Disable


                //Connect the Dismount Input
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnEndMounting, input.EnableInput, DismountInput.Name);
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnStartDismounting, input.DisableInput, DismountInput.Name);

                UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnLongPress, DismountAnimal); //Connect the Logic to the Dismount

                var RiderDismountUI = MTools.GetInstance<MEvent>("Rider Dismount UI");


                if (RiderDismountUI != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnLongPress, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnInputFloat, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(DismountInput.OnInputUp, RiderDismountUI.Invoke);
                    UnityEditor.Events.UnityEventTools.AddIntPersistentListener(DismountInput.OnInputDown, RiderDismountUI.Invoke, 0);
                }


                Debug.Log("<B>Dismount</B> Input created and connected to Rider.DismountAnimal");
            }

            #endregion

            #region CanCallMount Input


            var CanCallMount = input.FindInput("Call Mount");

            if (CanCallMount == null)
            {
                CanCallMount = new InputRow("Call Mount", "Call Mount", KeyCode.F, InputButton.Down, InputType.Key);
                input.inputs.Add(CanCallMount);

                //CanCallMount.active.Variable = MTools.GetInstance<BoolVar>("Can Call Mount");
                //CanCallMount.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(CanCallMount.OnInputDown, CallAnimalToggle);

                Debug.Log("<B>Call Mount</B> Input created and connected to Rider.CallAnimalToggle");
            }

            #endregion

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(input);
        }

        [ContextMenu("Create Event Listeners")]
        void CreateEventListeners()
        {
            MEvent RiderSetMount = MTools.GetInstance<MEvent>("Rider Set Mount");
            MEvent RiderSetDismount = MTools.GetInstance<MEvent>("Rider Set Dismount");

            var listener = this.FindComponent<MEventListener>() ?? gameObject.AddComponent<MEventListener>();

            listener.Events ??= new List<MEventItemListener>();

            if (listener.Events.Find(item => item.Event == RiderSetMount) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RiderSetMount,
                    useVoid = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, MountAnimal);
                listener.Events.Add(item);

                Debug.Log("<B>Rider Set Mount</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == RiderSetDismount) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RiderSetDismount,
                    useVoid = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, DismountAnimal);
                listener.Events.Add(item);

                Debug.Log("<B>Rider Set Dismount</B> Added to the Event Listeners");
            }

        }

        public void FindRHand()
        {
            if (animator != null && animator.avatar.isHuman)
            {
                RightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                MTools.SetDirty(this);
            }
        }
        public void FindLHand()
        {
            if (animator != null && animator.avatar.isHuman)
            {
                LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                MTools.SetDirty(this);
            }
        }




#if MALBERS_DEBUG
        void OnDrawGizmos()
        {
            if (Anim && Application.isPlaying && Mounted && Montura.debug && Montura.Animal.ActiveStateID == StateEnum.Locomotion)
            {
                Transform head = Anim.GetBoneTransform(HumanBodyBones.Head);

                Gizmos.color = (int)RiderNormalizedTime % 2 == 0 ? Color.red : Color.white;

                Gizmos.DrawSphere((head.position - RiderRoot.right * 0.2f), 0.05f);

                Gizmos.color = (int)HorseNormalizedTime % 2 == 0 ? new Color(0.11f, 1f, 0.25f) : Color.white;
                Gizmos.DrawSphere((head.position + RiderRoot.right * 0.2f), 0.05f);

                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(head.position + transform.up * 0.5f, "Sync Status");

            }

            if (debug)
            {
                if (!Application.isPlaying)
                {
                    var C = Color.white; C.a = 0.2f;

                    Gizmos.color = C;
                    Gizmos.DrawSphere((RiderRoot.position + (Vector3.up * TeleportHeight)), 0.025f);
                    UnityEditor.Handles.DrawWireDisc(RiderRoot.position, transform.up, CallRadius);

                    var UpVector = -Gravity.Value;
                    var HeightPoint = (RiderRoot.position + UpVector * TeleportHeight);

                    Gizmos.DrawRay(HeightPoint, -RiderRoot.forward * TeleportDistance);
                    Gizmos.DrawRay(HeightPoint + (-RiderRoot.forward * TeleportDistance), (2 * TeleportHeight * Gravity.Value));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!MountCollider.IsNull)
            {
                MDebug.GizmoCapsule(transform.TransformPoint(MountCollider.center), transform.rotation, MountCollider.height, MountCollider.radius, Color.yellow, MountCollider.direction);
            }

            if (RightHand && LeftHand)
            {
                var RightHandRein = RightHand.TransformPoint(RightReinOffset);
                var LeftHandRein = LeftHand.TransformPoint(LeftReinOffset);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(RightHandRein, 0.01f);
                Gizmos.DrawSphere(LeftHandRein, 0.01f);

                Gizmos.DrawLine(RightHandRein, RightHand.position);
                Gizmos.DrawLine(LeftHandRein, LeftHand.position);
            }
        }
#endif
#endif
        #endregion
    }

    #region INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(MRider), true)]
    public class MRiderEd : Editor
    {
        public readonly string version = "Riding System - HAP [v4.4.8b]";

        protected MRider M;

        protected SerializedProperty
            MountStored, StartMounted, Parent, animator, m_rigidBody, m_root, gravity, ReSync, SyncModes, ResyncThreshold,
            MountLayer, LayerPath, OnCanMount, OnCanDismount, OnStartMounting, OnEndMounting, m_CanMount, m_CanDismount, m_CanCallAnimal,
            OnStartDismounting, OnEndDismounting, OnFindMount, CanCallMount, OnAlreadyMounted, DisableList, MainCollider, kinematicRider,
            CallAnimalA, StopAnimalA, RiderAudio, MountCollider, colliders,
            // IKMounted,
            useIKFeet,
            LinkUpdate, debug, AlingMountTrigger, DismountType, DisableComponents, Editor_Tabs1,
            LeftHand, RightHand, RightReinOffset, LeftReinOffset,
            Ground, CallRadius, TeleportDistance, TeleportHeight,


            EditOffsetReinLeft, EditOffsetReinRight
            ;


        protected virtual void OnEnable()
        {
            M = (MRider)target;

            EditOffsetReinLeft = serializedObject.FindProperty("EditOffsetReinLeft");
            EditOffsetReinRight = serializedObject.FindProperty("EditOffsetReinRight");
            kinematicRider = serializedObject.FindProperty("kinematicRider");


            Ground = serializedObject.FindProperty("Ground");
            CallRadius = serializedObject.FindProperty("CallRadius");
            TeleportDistance = serializedObject.FindProperty("TeleportDistance");
            TeleportHeight = serializedObject.FindProperty("TeleportHeight");

            MountStored = serializedObject.FindProperty("m_MountStored");
            MainCollider = serializedObject.FindProperty("MainCollider");
            MountCollider = serializedObject.FindProperty("MountCollider");


            RightReinOffset = serializedObject.FindProperty("RightReinOffset");
            LeftReinOffset = serializedObject.FindProperty("LeftReinOffset");
            colliders = serializedObject.FindProperty("colliders");


            ReSync = serializedObject.FindProperty("ReSync");
            SyncModes = serializedObject.FindProperty("SyncModes");
            ResyncThreshold = serializedObject.FindProperty("ResyncThreshold");

            m_CanMount = serializedObject.FindProperty("m_CanMount");
            m_CanDismount = serializedObject.FindProperty("m_CanDismount");
            m_CanCallAnimal = serializedObject.FindProperty("m_CanCallAnimal");
            gravity = serializedObject.FindProperty("Gravity");

            animator = serializedObject.FindProperty("animator");
            m_rigidBody = serializedObject.FindProperty("m_rigidBody");
            m_root = serializedObject.FindProperty("m_root");
            StartMounted = serializedObject.FindProperty("StartMounted");
            Parent = serializedObject.FindProperty("Parent");
            MountLayer = serializedObject.FindProperty("MountLayer");
            LayerPath = serializedObject.FindProperty("LayerPath");
            useIKFeet = serializedObject.FindProperty("useIKFeet");
            // IKMounted = serializedObject.FindProperty("IKMounted");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

            OnCanMount = serializedObject.FindProperty("OnCanMount");
            OnCanDismount = serializedObject.FindProperty("OnCanDismount");
            OnStartMounting = serializedObject.FindProperty("OnStartMounting");
            OnEndMounting = serializedObject.FindProperty("OnEndMounting");
            OnStartDismounting = serializedObject.FindProperty("OnStartDismounting");
            OnEndDismounting = serializedObject.FindProperty("OnEndDismounting");
            OnFindMount = serializedObject.FindProperty("OnFindMount");
            CanCallMount = serializedObject.FindProperty("CanCallMount");
            OnAlreadyMounted = serializedObject.FindProperty("OnAlreadyMounted");

            CallAnimalA = serializedObject.FindProperty("CallAnimalA");
            StopAnimalA = serializedObject.FindProperty("StopAnimalA");

            RiderAudio = serializedObject.FindProperty("RiderAudio");


            RightHand = serializedObject.FindProperty("RightHand");
            LeftHand = serializedObject.FindProperty("LeftHand");

            LinkUpdate = serializedObject.FindProperty("LinkUpdate");

            debug = serializedObject.FindProperty("debug");
            AlingMountTrigger = serializedObject.FindProperty("AlingMountTrigger");
            DismountType = serializedObject.FindProperty("DismountType");


            DisableComponents = serializedObject.FindProperty("DisableComponents");
            DisableList = serializedObject.FindProperty("DisableList");
        }

        #region GUICONTENT
        private readonly GUIContent G_DisableComponents = new GUIContent("Disable Components", "If some of the components are breaking the Rider Logic, disable them");
        private readonly GUIContent G_DisableList = new GUIContent("Disable List", "Monobehaviours that will be disabled while mounted");
        // private readonly GUIContent G_CreateColliderMounted = new GUIContent("Create capsule collider while Mounted", "This collider is for hit the Rider while mounted");
        private readonly GUIContent G_Parent = new GUIContent("Parent to Mount", "Parent the Rider to the Mount Point on the Mountable Animal");
        private readonly GUIContent G_DismountType = new GUIContent("Dismount Type", "Changes the Dismount animation on the Rider.\nRandom: Randomly select a Dismount Animation.\nInput: Select the Dismount Animation by the Horizontal and Vertical Input Axis.\n Last: Uses the Last Mount Animation as a reference for the Dismount Animation.");
        // private readonly GUIContent G_DismountMountOnDeath = new GUIContent("Dismount if mount dies", "The Rider will automatically dismount if the Animal Dies");
        #endregion

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription(version);
            serializedObject.Update();

            if (!Application.isPlaying) AddMountLayer();

            // EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Events", "Advanced", "Debug" });
            int Selection = Editor_Tabs1.intValue;

            if (Selection == 0) DrawGeneral();
            else if (Selection == 1) DrawEvents();
            else if (Selection == 2) DrawAdvanced();
            else if (Selection == 3) DrawDebug();
            serializedObject.ApplyModifiedProperties();
            //EditorGUILayout.EndVertical(); 
        }


        private void OnSceneGUI()
        {
            // if (Tools.current == Tool.Move)
            {
                var RightHandT = M.RightHand;

                if (RightHandT && EditOffsetReinRight.boolValue)
                {
                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        Vector3 piv = RightHandT.TransformPoint(M.RightReinOffset.Value);
                        Vector3 NewPivPosition = Handles.PositionHandle(piv, Quaternion.identity);

                        if (cc.changed)
                        {
                            Undo.RecordObject(target, "Change Pos Rein Offest");
                            M.RightReinOffset.Value = RightHandT.InverseTransformPoint(NewPivPosition);
                            EditorUtility.SetDirty(target);
                        }
                    }
                }

                var LeftHandT = M.LeftHand;

                if (LeftHandT && EditOffsetReinLeft.boolValue)
                {
                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        Vector3 piv = LeftHandT.TransformPoint(M.LeftReinOffset.Value);
                        Vector3 NewPivPosition = Handles.PositionHandle(piv, Quaternion.identity);

                        if (cc.changed)
                        {
                            Undo.RecordObject(target, "Change Pos Rein Offest");
                            M.LeftReinOffset.Value = LeftHandT.InverseTransformPoint(NewPivPosition);
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
            }
        }

        private void AddMountLayer()
        {
            Animator anim = M.Anim;

            if (anim)
            {
                var controller = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;
                if (controller)
                {
                    var layers = controller.layers.ToList();

                    if (layers.Find(layer => layer.name == M.MountLayer) == null)
                    {
                        var defaultColor = GUI.color;
                        GUI.color = Color.green;

                        var ST = new GUIStyle(EditorStyles.miniButtonMid) { fontStyle = FontStyle.Bold };

                        if (GUILayout.Button(new GUIContent("Add Mounted Layer",
                            "There's no [Mounted] layer on the Current Animator. This will add all the Animator Parameters and States needed for the Riding System"), ST))
                        {
                            AddLayerMounted(controller);
                        }

                        GUI.color = defaultColor;
                    }
                }
            }
        }

        private void DrawDebug()
        {
            if (Application.isPlaying)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ToggleLeft("Can Mount", M.CanMount);
                        EditorGUILayout.ToggleLeft("Can Dismount", M.CanDismount);
                        EditorGUILayout.ToggleLeft("Can Call Animal", M.CanCallAnimal);
                        EditorGUILayout.Space();
                        EditorGUILayout.ToggleLeft("Mounted", M.Mounted);

                        EditorGUILayout.ToggleLeft("Is on Horse", M.IsOnHorse);
                        EditorGUILayout.ToggleLeft("Is Mounting", M.IsMounting);
                        EditorGUILayout.ToggleLeft("Is Riding", M.IsRiding);
                        EditorGUILayout.ToggleLeft("Is Dismounting", M.IsDismounting);
                        //EditorGUILayout.FloatField("Straight Spine", M.SP_Weight);
                        EditorGUILayout.Space();
                        EditorGUILayout.ObjectField("Current Mount", M.Montura, typeof(Mount), false);
                        EditorGUILayout.ObjectField("Stored Mount", M.MountStored, typeof(Mount), false);
                        EditorGUILayout.ObjectField("Mount Trigger", M.MountTrigger, typeof(MountTriggers), false);
                    }
                    Repaint();
                }
            }
        }

        private void DrawAdvanced()
        {

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(m_rigidBody);
                EditorGUILayout.PropertyField(m_root, new GUIContent("Rider's Root", "Root Gameobject for the Rider Character"));
                //  EditorGUILayout.PropertyField(IKMounted);
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(ReSync);
                EditorGUILayout.PropertyField(SyncModes);
                EditorGUILayout.PropertyField(ResyncThreshold);
                EditorGUILayout.PropertyField(AlingMountTrigger,
                    new GUIContent("Align MTrigger Time", "Time to Align to the Mount Trigger Position while is playing the Mount Animation"));
                EditorGUILayout.PropertyField(LayerPath);
                EditorGUILayout.PropertyField(MountLayer);
                EditorGUILayout.PropertyField(useIKFeet);
            }

        }

        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (animator.isExpanded = MalbersEditor.Foldout(animator.isExpanded, "Mount|Dismount"))
                {
                    EditorGUILayout.PropertyField(OnStartMounting);
                    EditorGUILayout.PropertyField(OnEndMounting);
                    EditorGUILayout.PropertyField(OnStartDismounting);
                    EditorGUILayout.PropertyField(OnEndDismounting);
                }



                if (m_root.isExpanded = MalbersEditor.Foldout(m_root.isExpanded, "Other"))
                {
                    EditorGUILayout.PropertyField(OnCanMount);
                    EditorGUILayout.PropertyField(OnCanDismount);
                    EditorGUILayout.PropertyField(OnFindMount);
                    EditorGUILayout.PropertyField(CanCallMount);
                }
            }
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var MStoredGUI = "Stored Mount";
                var MStoredTooltip = "If Start Mounted is Active this will be the Animal to mount.";

                if (M.m_MountStored.Value != null && M.m_MountStored.Value.IsPrefab())
                {
                    MStoredGUI += "[P]";
                    MStoredTooltip += "\nThe Stored Mount is a Prefab. It will be instantiated";
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(StartMounted, new GUIContent("Start Mounted", "Set an animal to start mounted on it"));
                    MalbersEditor.DrawDebugIcon(debug);
                }
                EditorGUILayout.PropertyField(MountStored, new GUIContent(MStoredGUI, MStoredTooltip));

                //if (M.StartMounted.Value && M.MountStored == null)
                //{
                //    EditorGUILayout.HelpBox("Select an Animal with 'IMount' interface from the scene if you want to start mounted on it", MessageType.Warning);
                //}
            }



            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Parent.isExpanded = MalbersEditor.Foldout(Parent.isExpanded, "Parenting");
                if (Parent.isExpanded)
                {
                    EditorGUILayout.PropertyField(Parent, G_Parent);
                    EditorGUILayout.PropertyField(LinkUpdate,
                        new GUIContent("Link Update", "Updates Everyframe the position and rotation of the rider to the Animal Mount Point"));
                    EditorGUILayout.PropertyField(DismountType, G_DismountType);
                    EditorGUILayout.PropertyField(gravity);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MainCollider.isExpanded = MalbersEditor.Foldout(MainCollider.isExpanded, "Rider Collider");
                if (MainCollider.isExpanded)
                {


                    EditorGUILayout.PropertyField(kinematicRider);
                    EditorGUILayout.PropertyField(MainCollider, new GUIContent("Main Collider", "Main Character collider for the Rider"));
                    EditorGUILayout.PropertyField(MountCollider, new GUIContent("Collider Modifier", "When mounting the Collider will change its properties to this preset"));

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(colliders, new GUIContent("Internal Colliders", "When mounting the Collider will change its properties to this preset"));
                    EditorGUI.indentLevel--;
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                RightHand.isExpanded = MalbersEditor.Foldout(RightHand.isExpanded, "Rider Hands");
                if (RightHand.isExpanded)
                {
                    EditorGUILayout.PropertyField(LeftHand);
                    EditorGUILayout.PropertyField(RightHand);
                    EditorGUILayout.Space();

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(LeftReinOffset);
                        EditOffsetReinLeft.boolValue = GUILayout.Toggle(EditOffsetReinLeft.boolValue, EditIcon, EditorStyles.miniButtonMid, GUILayout.Width(28), GUILayout.Height(20));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(RightReinOffset);
                        EditOffsetReinRight.boolValue = GUILayout.Toggle(EditOffsetReinRight.boolValue, EditIcon, EditorStyles.miniButtonMid, GUILayout.Width(28), GUILayout.Height(20));
                    }
                }
            }

            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    m_CanMount.isExpanded = MalbersEditor.Foldout(m_CanMount.isExpanded, "Exposed Values");
                    if (m_CanMount.isExpanded)
                    {
                        EditorGUILayout.PropertyField(m_CanMount,
                            new GUIContent("Can Mount",
                            "It will be enabled when the Rider is near a mount Trigger,\nIt's used on the Active parameter of the Mount Input"));

                        EditorGUILayout.PropertyField(m_CanDismount,
                            new GUIContent("Can Dismount",
                            "It will be enabled when the Rider riding a mount,\nIt's used on the Active parameter of the Dismount Input"));

                        EditorGUILayout.PropertyField(m_CanCallAnimal,
                            new GUIContent("Can Call Mount", "It will be enabled when the Rider has a Mount Stored and is not near or mounted is near the mount,\nIt's used on the Active parameter of the Can call Mount Input"));
                    }
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Ground.isExpanded = MalbersEditor.Foldout(Ground.isExpanded, "Call Mount");
                if (Ground.isExpanded)
                {
                    EditorGUILayout.PropertyField(Ground);
                    EditorGUILayout.PropertyField(CallRadius);
                    EditorGUILayout.PropertyField(TeleportDistance);
                    EditorGUILayout.PropertyField(TeleportHeight);

                    EditorGUILayout.PropertyField(CallAnimalA, new GUIContent("Call Animal", "Sound to call the Stored Animal"));
                    EditorGUILayout.PropertyField(StopAnimalA, new GUIContent("Stop Animal", "Sound to stop calling the Stored Animal"));
                    EditorGUILayout.PropertyField(RiderAudio, new GUIContent("Audio Source", "The reference for the audio source"));
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(DisableComponents, G_DisableComponents);

                if (M.DisableComponents.Value)
                {
                    MalbersEditor.Arrays(DisableList, G_DisableList);

                    if (M.DisableList != null && M.DisableList.Length == 0)
                    {
                        EditorGUILayout.HelpBox("If 'Disable List' is empty , it will disable all Monovehaviours while riding", MessageType.Info);
                    }
                }
            }
        }
        void AddLayerMounted(UnityEditor.Animations.AnimatorController AnimController)
        {
            var MountAnimator = Resources.Load<UnityEditor.Animations.AnimatorController>(M.LayerPath);

            AddParametersOnAnimator(AnimController, MountAnimator);

            foreach (var item in MountAnimator.layers)
            {
                AnimController.AddLayer(item);
            }

            Debug.Log("Mount Layer added to: " + AnimController.name);
        }

        public static void AddParametersOnAnimator(UnityEditor.Animations.AnimatorController AnimController, UnityEditor.Animations.AnimatorController Mounted)
        {
            AnimatorControllerParameter[] parameters = AnimController.parameters;
            AnimatorControllerParameter[] Mountedparameters = Mounted.parameters;

            foreach (var param in Mountedparameters)
            {
                if (!SearchParameter(parameters, param.name))
                {
                    AnimController.AddParameter(param);
                }
            }
        }

        public static bool SearchParameter(AnimatorControllerParameter[] parameters, string name)
        {
            foreach (AnimatorControllerParameter item in parameters)
            {
                if (item.name == name) return true;
            }
            return false;
        }




        private static GUIContent editicon;

        public static GUIContent EditIcon
        {
            get
            {
                editicon ??= new GUIContent(EditorGUIUtility.IconContent("TransformTool"));
                return editicon;
            }
        }
    }
#endif
    #endregion
}