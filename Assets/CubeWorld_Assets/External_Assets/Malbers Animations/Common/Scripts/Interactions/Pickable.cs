using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using MalbersAnimations.Reactions;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Interaction/Pickable")]
    [SelectionBase]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/pickable")]
    public class Pickable : MonoBehaviour, ICollectable
    {
        public bool Align = false;
        [Min(0)] public float AlignTime = 0.15f;

        [Tooltip("Delay time after calling the Pick() method. the item will be parented to the PickUp component after this time has passed")]
        public FloatReference PickDelay = new(0);
        [Tooltip("Delay time after calling the Drop() method. the item will be unparented from the PickUp component after this time has passed")]
        public FloatReference DropDelay = new(0);
        [Tooltip("Cooldown needed to pick or drop again the collectable")]
        public FloatReference coolDown = new(0f);
        [Tooltip("When an Object is Collectable it means that the Picker can still pick objects, the item was collected by another component (E.g. Weapons or Inventory)")]

        public BoolReference m_Collectable = new(false);
        [Tooltip("The Pick Up Drop Logic will be called via animator events/messages. Use These methods on the Animator: TryPick(), TryDrop(), TryPickUpDrop()")]
        public BoolReference m_ByAnimation = new(false);
        [Tooltip("The Pick Up Drop Logic will be called via animator events/messages")]
        public BoolReference m_DestroyOnPick = new(false);

        [Tooltip(" Amount Pickable Item can store.. that it can be use for anything")]
        public IntReference m_Amount = new(1); //Not done yet

        [Tooltip("The pickable can be picked. Set it to false to temporalry disabled the Pick method at runtime")]
        public BoolReference canBePicked = new(true); //Not done yet


        [Tooltip("The pickable will be pick automatically when it enters the focus area from the picker")]
        public BoolReference m_AutoPick = new(false); //Not done yet
        public IntReference m_ID = new();         //Not done yet

        /// <summary>Who Did the Picking </summary>
        public MPickUp Picker { get; set; }


        [Tooltip("What holder will the item be parent to. -1: Default Holder. >=0 : Index of the Extra Holder list")]
        public int holder = -1;

        public BoolEvent OnFocused = new();
        public GameObjectEvent OnFocusedBy = new();
        public GameObjectEvent OnUnfocusedBy = new();
        public GameObjectEvent OnPicked = new();
        public GameObjectEvent OnPickedFailed = new();
        public GameObjectEvent OnPrePicked = new();
        public GameObjectEvent OnDropped = new();
        public GameObjectEvent OnPreDropped = new();


        [SerializeReference, SubclassSelector]
        public Reaction FocusedByReaction;
        [SerializeReference, SubclassSelector]
        public Reaction UnFocusedByReaction;
        [SerializeReference, SubclassSelector]
        public Reaction PickedReaction;
        [SerializeReference, SubclassSelector]
        public Reaction PrePickedReaction;
        [SerializeReference, SubclassSelector]
        public Reaction DroppedReaction;
        [SerializeReference, SubclassSelector]
        public Reaction PreDroppedReaction;

        [SerializeField] private Rigidbody rb;


        [Tooltip("Destroys Rigidbody while picked. This will remove completely the rigidbody from the Pickable object and it will restore it after is dropped.")]
        public BoolReference DestroyRbOnPick = new(false);

        [RequiredField] public Collider[] m_colliders;

        [Tooltip("If the item is picked it will be kinematic")]
        public BoolReference kinematicOnPick = new(true);

        [Tooltip("Disable Colliders when the Item is picked (Colliders will be enabled back when the item is dropped")]
        public BoolReference disableColliders = new(true);

        /// <summary>  Cache the Rigid body parameters  on start  </summary>
        protected RigidbodyParameters rigidbodyParameters;
        /// <summary>  Cache  if the RB is kinematic  </summary>
        protected bool defaultKinematic;
        protected RigidbodyConstraints defaultConstraints;

        protected float currentPickTime;


        /// <summary>Is this Object being picked </summary>
        public bool IsPicked { get; set; }

        /// <summary>Current value of the Item</summary>
        public int Amount { get => m_Amount.Value; set => m_Amount.Value = value; }

        /// <summary>The Item will be autopicked if the Picker is focusing it</summary>
        public bool AutoPick { get => m_AutoPick.Value; set => m_AutoPick.Value = value; }

        /// <summary>The pickable can be picked. Set it to false to temporaliry disabled the Pick method at runtime</summary>
        public bool CanBePicked { get => canBePicked.Value; set => canBePicked.Value = value; }
        public bool Collectable { get => m_Collectable.Value; set => m_Collectable.Value = value; }
        public Rigidbody RigidBody => rb;

        /// <summary>The Pick Up Drop Logic will be called via animator events/messages</summary>
        public bool ByAnimation { get => m_ByAnimation.Value; set => m_ByAnimation.Value = value; }
        public bool DestroyOnPick { get => m_DestroyOnPick.Value; set => m_DestroyOnPick.Value = value; }
        public bool InCoolDown => !MTools.ElapsedTime(CurrentPickTime, coolDown);
        public int ID { get => m_ID.Value; set => m_ID.Value = value; }

        protected Vector3 DefaultScale;


        protected bool focused;
        public virtual bool Focused
        {
            get => focused;
            protected set
            {
                focused = value;
                OnFocused.Invoke(focused);
            }
        }

        public virtual void SetFocused(GameObject FocusBy, bool isFocused)
        {
            Focused = isFocused;

            if (isFocused)
            {
                OnFocusedBy.Invoke(FocusBy);
                FocusedByReaction?.React(FocusBy);
            }
            else
            {
                //Maybe this can be removed after
                OnFocusedBy.Invoke(null);
                FocusedByReaction?.React((Component)null);


                OnUnfocusedBy.Invoke(FocusBy);
                UnFocusedByReaction?.React(FocusBy);
            }
        }

        /// <summary>Game Time the Pickable was Picked</summary>
        public float CurrentPickTime { get => currentPickTime; set => currentPickTime = value; }

        protected virtual void OnDisable()
        {
            Focused = false;
        }



        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (m_colliders == null || m_colliders.Length == 0) m_colliders = GetComponents<Collider>();

            CurrentPickTime = -coolDown;

            DefaultScale = transform.localScale;

            if (rb)
            {
                defaultKinematic = rb.isKinematic; //Store the default value of the kinematic
                defaultConstraints = rb.constraints; //Store the default value of the constraints
                rigidbodyParameters = new(rb);
            }
        }

        public virtual void Pick()
        {
            OnPickDisablePhysics();                 //Disable all physics when the item is picked
            IsPicked = !Collectable;                //Check if the Item is collectable 

            //Weapons can be picked witout having a picker
            var realPicker = Picker ? Picker.Root.gameObject : null;

            SetFocused(realPicker, false);

            OnPicked.Invoke(realPicker);             //Call the Event
            PickedReaction?.React(realPicker);

            CurrentPickTime = Time.time;            //Store the time it was picked
            if (Collectable) enabled = false;

            if (DestroyOnPick)
            {
                if (TryGetComponent(out IPoolGameObject poolObject))
                {
                    poolObject.Pool_Release(gameObject); //Go back to the Pool
                }
                else
                {
                    DestroyPickUp();
                }
            }
        }

        protected virtual void DestroyPickUp()
        {
            //Needs to check first if the item is from a Pool



            Destroy(gameObject);
        }

        public virtual void Drop()
        {
            OnDropEnablePhysics();
            IsPicked = false;
            enabled = true;

            transform.parent = null;                                //UnParent
            transform.localScale = DefaultScale;                    //Restore the Scale


            var realPicker = Picker ? Picker.Root.gameObject : null;

            OnDropped.Invoke(realPicker);
            DroppedReaction?.React(realPicker);

            Picker = null;                                          //Reset who did the picking
            CurrentPickTime = Time.time;
        }

        /// <summary> Call this in case a picker has still the item </summary>
        public virtual void ForceDrop() => Picker?.DropItem();

        public virtual void OnPickDisablePhysics()
        {
            if (DestroyRbOnPick) //Destroy the Rigidbody
            {
                Destroy(rb);
                rb = null;
            }


            if (RigidBody)
            {
                RigidBody.useGravity = false;

#if !UNITY_2022_3_OR_NEWER
                RigidBody.velocity = Vector3.zero;
#endif
                RigidBody.isKinematic = kinematicOnPick.Value;
                //  Debug.Log($"kin {name}");

                if (RigidBody.isKinematic)
                    RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                RigidBody.constraints = RigidbodyConstraints.FreezeAll; //Freeze the rotation of the item

                // RigidBody.detectCollisions = false;

            }

            if (disableColliders.Value)
            {
                foreach (var c in m_colliders)
                {
                    if (c) c.enabled = false; //Disable all colliders
                }
            }
        }

        public virtual void OnDropEnablePhysics()
        {
            if (RigidBody)
            {
                RigidBody.useGravity = true;
                RigidBody.isKinematic = defaultKinematic; //Restore the kinematic value of the item

                if (!RigidBody.isKinematic)
                    RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // RigidBody.detectCollisions = true;

                RigidBody.constraints = defaultConstraints; //Restore the constraints of the item
            }

            if (DestroyRbOnPick) //Restore the Rigidbody
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rigidbodyParameters.RestoreRigidBody(rb);
            }

            foreach (var c in m_colliders)
            {
                c.enabled = true; //Enable all colliders
            }
        }

        public virtual void SetEnable(bool enable)
        {
            this.enabled = enable;
        }
        [HideInInspector] public int EditorTabs = 0;
    }

    public struct RigidbodyParameters
    {
        public bool useGravity;
        public bool isKinematic;
        public CollisionDetectionMode collisionDetectionMode;
        public RigidbodyConstraints constraints;
        public bool detectCollisions;
        public RigidbodyInterpolation interpolation;
        public float mass;
        public float drag;
        public float angularDrag;
        public Vector3 centerOfMass;
        public Vector3 inertiaTensor;

        public RigidbodyParameters(Rigidbody rb)
        {
            useGravity = rb.useGravity;
            isKinematic = rb.isKinematic;
            collisionDetectionMode = rb.collisionDetectionMode;
            constraints = rb.constraints;
            detectCollisions = rb.detectCollisions;
            interpolation = rb.interpolation;
            mass = rb.mass;
            drag = rb.linearDamping;
            angularDrag = rb.angularDamping;
            centerOfMass = rb.centerOfMass;
            inertiaTensor = rb.inertiaTensor;
        }

        public void RestoreRigidBody(Rigidbody rb)
        {
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            rb.collisionDetectionMode = collisionDetectionMode;
            rb.constraints = constraints;
            rb.detectCollisions = detectCollisions;
            rb.interpolation = interpolation;
            rb.mass = mass;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.centerOfMass = centerOfMass;
            rb.inertiaTensor = inertiaTensor;
        }

    }
    //INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(Pickable)), CanEditMultipleObjects]
    public class PickableEditor : Editor
    {
        private SerializedProperty //   PickAnimations, PickUpMode, PickUpAbility, DropMode, DropAbility,DropAnimations, 
            Align, AlignTime, AlignDistance, AlignPos, EditorTabs,
            m_AutoPick, DropDelay, PickDelay, rb, kinematicOnPick, disableColliders, DestroyRbOnPick, canBePicked,
            //kinematicOnDrop,
            CoolDown,// SceneRoot,
            OnFocused, OnFocusedBy, OnUnfocusedBy,
            OnPrePicked, OnPicked, OnDropped, OnPreDropped, holder, OnPickedFailed,

            PrePickedReaction, PreDroppedReaction, DroppedReaction, PickedReaction, UnFocusedByReaction, FocusedByReaction,


            Amount, IntID, m_collider, m_Collectable, m_ByAnimation, m_DestroyOnPick;

        private Pickable m;

        protected string[] Tabs1 = new string[] { "General", "Events", "Reactions" };

        private void OnEnable()
        {
            m = (Pickable)target;

            EditorTabs = serializedObject.FindProperty("EditorTabs");
            //SceneRoot = serializedObject.FindProperty("SceneRoot");

            rb = serializedObject.FindProperty("rb");
            kinematicOnPick = serializedObject.FindProperty("kinematicOnPick");
            disableColliders = serializedObject.FindProperty("disableColliders");
            DestroyRbOnPick = serializedObject.FindProperty("DestroyRbOnPick");
            //kinematicOnDrop = serializedObject.FindProperty("kinematicOnDrop");


            PickDelay = serializedObject.FindProperty("PickDelay");
            DropDelay = serializedObject.FindProperty("DropDelay");
            m_Collectable = serializedObject.FindProperty("m_Collectable");
            m_ByAnimation = serializedObject.FindProperty("m_ByAnimation");
            m_DestroyOnPick = serializedObject.FindProperty("m_DestroyOnPick");


            Align = serializedObject.FindProperty("Align");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignDistance = serializedObject.FindProperty("AlignDistance");

            canBePicked = serializedObject.FindProperty("canBePicked");
            OnPickedFailed = serializedObject.FindProperty("OnPickedFailed");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnFocusedBy = serializedObject.FindProperty("OnFocusedBy");
            OnUnfocusedBy = serializedObject.FindProperty("OnUnfocusedBy");
            OnPicked = serializedObject.FindProperty("OnPicked");
            OnPrePicked = serializedObject.FindProperty("OnPrePicked");
            OnDropped = serializedObject.FindProperty("OnDropped");
            OnPreDropped = serializedObject.FindProperty("OnPreDropped");


            holder = serializedObject.FindProperty("holder");


            FocusedByReaction = serializedObject.FindProperty("FocusedByReaction");
            UnFocusedByReaction = serializedObject.FindProperty("UnFocusedByReaction");
            PickedReaction = serializedObject.FindProperty("PickedReaction");
            DroppedReaction = serializedObject.FindProperty("DroppedReaction");
            PreDroppedReaction = serializedObject.FindProperty("PreDroppedReaction");
            PrePickedReaction = serializedObject.FindProperty("PrePickedReaction");



            //ShowEvents = serializedObject.FindProperty("ShowEvents");
            Amount = serializedObject.FindProperty("m_Amount");
            IntID = serializedObject.FindProperty("m_ID");
            m_collider = serializedObject.FindProperty("m_colliders");
            AlignPos = serializedObject.FindProperty("AlignPos");
            //Collectable = serializedObject.FindProperty("Collectable");
            m_AutoPick = serializedObject.FindProperty("m_AutoPick");
            CoolDown = serializedObject.FindProperty("coolDown");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Pickable - Collectable object");
            EditorTabs.intValue = GUILayout.Toolbar(EditorTabs.intValue, Tabs1);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (Application.isPlaying)
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ToggleLeft("Is Picked", m.IsPicked);
                        EditorGUILayout.ToggleLeft("Is Focused", m.Focused);
                    }
                }

                if (EditorTabs.intValue == 0)
                    DrawGeneral();
                else if (EditorTabs.intValue == 1)
                    DrawEvents();
                else
                {
                    DrawReactions();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnFocused);
            EditorGUILayout.PropertyField(OnFocusedBy);
            EditorGUILayout.PropertyField(OnUnfocusedBy);
            if (m.PickDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPrePicked, new GUIContent("On Pre-Picked By"));
            EditorGUILayout.PropertyField(OnPicked, new GUIContent("On Picked By"));
            if (m.DropDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPreDropped, new GUIContent("On Pre-Dropped By"));
            EditorGUILayout.PropertyField(OnDropped, new GUIContent("On Dropped By"));

            EditorGUILayout.PropertyField(OnPickedFailed);

        }

        private void DrawReactions()
        {
            EditorGUILayout.PropertyField(FocusedByReaction);
            EditorGUILayout.PropertyField(UnFocusedByReaction);
            if (m.PickDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(PrePickedReaction);
            EditorGUILayout.PropertyField(PickedReaction);
            if (m.DropDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(PreDroppedReaction);
            EditorGUILayout.PropertyField(DroppedReaction);

        }

        private void DrawGeneral()
        {
            m_AutoPick.isExpanded = MalbersEditor.Foldout(m_AutoPick.isExpanded, "Pickable Item");

            if (m_AutoPick.isExpanded)
            {
                EditorGUILayout.PropertyField(canBePicked);
                EditorGUILayout.PropertyField(holder);
                EditorGUILayout.PropertyField(IntID, new GUIContent("ID", "Int value the Pickable Item can store. This ID is used by the Picker component to Identify each Pickable Object"));
                EditorGUILayout.PropertyField(Amount);

                EditorGUILayout.PropertyField(m_AutoPick, new GUIContent("Auto", "The Item will be Picked Automatically"));
                EditorGUILayout.PropertyField(m_ByAnimation,
                    new GUIContent("Use Animation", "The Item will Pre-Picked/Dropped by the Picker Animator." +
                    " Pick-Drop Logic is called by Animation Event or Animator Message Behaviour.\nUse the Methods: TryPickUpDrop(); TryPickUp(); TryDrop();"));

                //   EditorGUILayout.PropertyField(SceneRoot);


                EditorGUILayout.PropertyField(m_Collectable, new GUIContent("Collectable", "The Item will Picked by the Pickable and it will be stored"));
                if (m.Collectable)
                    EditorGUILayout.PropertyField(m_DestroyOnPick, new GUIContent("Destroy Collectable", "The Item will be destroyed after is picked"));
            }

            rb.isExpanded = MalbersEditor.Foldout(rb.isExpanded, "References");
            if (rb.isExpanded)
            {
                EditorGUILayout.PropertyField(rb, new GUIContent("Rigid Body"));
                EditorGUILayout.PropertyField(kinematicOnPick);
                EditorGUILayout.PropertyField(DestroyRbOnPick);
                // EditorGUILayout.PropertyField(kinematicOnDrop);

                EditorGUILayout.PropertyField(disableColliders);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_collider);
                EditorGUI.indentLevel--;
            }


            CoolDown.isExpanded = MalbersEditor.Foldout(CoolDown.isExpanded, "Delays");

            if (CoolDown.isExpanded)
            {
                EditorGUILayout.PropertyField(CoolDown);
                EditorGUILayout.PropertyField(PickDelay);
                EditorGUILayout.PropertyField(DropDelay);
            }


            Align.isExpanded = MalbersEditor.Foldout(Align.isExpanded, "Alignment");

            if (Align.isExpanded)
            {
                EditorGUILayout.PropertyField(Align, new GUIContent("Align On Pick", "Align the Item to the Picker Position"));

                if (Align.boolValue)
                {

                    //using (new GUILayout.HorizontalScope())
                    //{

                    //    EditorGUILayout.PropertyField(AlignPos, new GUIContent("Align Pos", "align the Position"));

                    //    EditorGUIUtility.labelWidth = 60;
                    //    EditorGUILayout.PropertyField
                    //        (AlignDistance, new GUIContent("Distance", "Distance to move the Animal towards the Item"), GUILayout.MinWidth(50));
                    //    EditorGUIUtility.labelWidth = 0;
                    //}
                    EditorGUILayout.PropertyField(AlignTime, new GUIContent("Time", "Time required to do the alignment"));
                }
            }


        }
    }
#endif
}