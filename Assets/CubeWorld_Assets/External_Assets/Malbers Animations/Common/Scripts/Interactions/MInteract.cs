using MalbersAnimations.Scriptables;
using UnityEngine;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    [DefaultExecutionOrder(15)]
    [SelectionBase]
    [AddComponentMenu("Malbers/Interaction/Interactable")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/interactable")]
    public class MInteract : MonoBehaviour, IInteractable
    {
        [Tooltip("Own Index. This is used to Identify each Interactable. 0 or -1 means that all interactors can interact with this.")]
        public IntReference m_ID = new(0);
        [Tooltip("ID for the Interactor. Makes this Interactable to interact only with Interactors with this ID Value\n" +
            "By default its -1, which means that can be activated by anyone")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_InteracterID")]
        public IntReference m_InteractorID = new(-1);

        [Tooltip("If the Interactor has this Interactable focused, it will interact with it automatically.\n" +
            "It also is used by the AI Animals. If the Animal Reaches this gameobject to Interact with it this needs to be set to true")]
        [SerializeField] private BoolReference m_Auto = new(false);

        [Tooltip("Interact Once, after that it cannot longer work, unlest the Interactable is Restarted. Disable the component")]
        [SerializeField] private BoolReference m_singleInteraction = new(false);

        [Tooltip("Destroy after a Single Interaction. (After the Delay)")]
        [SerializeField] private BoolReference m_Destroy = new(false);

        [Tooltip("Delay time to activate the events on the Interactable")]
        public FloatReference m_Delay = new(0);

        [Tooltip("CoolDown between Interactions when the Interactable is NOT a Single/One time interaction")]
        public FloatReference m_CoolDown = new(0);

        [Tooltip("When an Interaction is executed these events will be invoked." +
            "\n\nOnInteractWithGO(GameObject) -> will have the *INTERACTER* gameObject as parameter" +
            "\n\nOnInteractWith(Int) -> will have the *INTERACTER* ID as parameter")]
        public InteractionEvents events = new();


        public GameObjectEvent OnInteractWithGO = new();
        public IntEvent OnInteractWithID = new();
        public GameObjectEvent OnFocused = new();
        public GameObjectEvent OnUnfocused = new();
        public BoolEvent OnCoolDown = new();

        //[SerializeReference, SubclassSelector]
        //public Reaction InteractReaction;
        //[SerializeReference, SubclassSelector]
        //public Reaction FocusedReaction;
        //[SerializeReference, SubclassSelector]
        //public Reaction UnFocusedReaction;

        public int Index => m_ID;

        public bool Active { get => enabled && !InCooldown; set => enabled = value; }
        public bool SingleInteraction { get => m_singleInteraction.Value; set => m_singleInteraction.Value = value; }
        public bool Auto { get => m_Auto.Value; set => m_Auto.Value = value; }
        // public bool Active { get =>enabled; set => enabled = value; }

        /// <summary>Delay time to Activate the Interaction on the Interactable</summary>
        public float Delay { get => m_Delay.Value; set => m_Delay.Value = value; }

        /// <summary>CoolDown Between Interactions</summary>
        public float Cooldown { get => m_CoolDown.Value; set => m_CoolDown.Value = value; }

        /// <summary>Is the Interactable in CoolDown?</summary>
        public bool InCooldown => !MTools.ElapsedTime(CurrentActivationTime, Cooldown);

        public IInteractor CurrentInteractor { get; set; }

        private bool focused;
        public bool Focused
        {
            get => focused;
            set
            {
                if (focused != value)
                {
                    focused = value;

                    if (focused)
                    {
                        OnFocused.Invoke(CurrentInteractor?.Owner);
                    }
                    else
                    {
                        OnUnfocused.Invoke(CurrentInteractor?.Owner);
                    }
                }
            }
        }

        public GameObject Owner { get; set; }

        private float CurrentActivationTime;

        public string Description = "Invoke events when an Interactor interacts with it";
        [HideInInspector] public bool ShowDescription = true;
        [ContextMenu("Show Description")]
        internal void EditDescription() => ShowDescription ^= true;

        private void OnEnable()
        {
            Owner = transform.FindObjectCore().gameObject;
            CurrentActivationTime = -Cooldown;


            //Make Sure the Old events are added to the new ones
            if (events.OnInteractWith.GetPersistentEventCount() > 0)
            {
                MDebug.LogWarning($"[{name}] Old Events are still used. Please copy the values to the new events. Then use the Context Menu Clear Old Events", gameObject);
            }

            if (events.OnInteractWithGO.GetPersistentEventCount() > 0)
            {
                MDebug.LogWarning($"[{name}] Old Events are still used. Please copy the values to the new events. Then use the Context Menu Clear Old Events", gameObject);
            }
        }


        [ContextMenu("Clear Old Events")]
        private void ClearOldEvents()
        {
            events.OnInteractWith = new IntEvent();
            events.OnInteractWithGO = new GameObjectEvent();
            MTools.SetDirty(this);
        }

        private void OnDisable()
        {
            focused = false;
            CurrentInteractor?.UnFocus(this);    //Clean the Current Focused item  
        }

        /// <summary> Receive an Interaction from the Interacter </summary>
        /// <param name="InteracterID">ID of the Interacter</param>
        /// <param name="interacter">Interacter's GameObject</param>
        public bool Interact(int InteracterID, GameObject interacter)
        {
            if (Active)
            {
                if (m_InteractorID <= 0 || m_InteractorID == InteracterID) //Check for Interactor ID
                {
                    CurrentActivationTime = Time.time;

                    this.Delay_Action(Delay, () =>
                     {
                         OnInteractWithGO.Invoke(interacter);
                         OnInteractWithID.Invoke(InteracterID);
                     }
                    );

                    if (SingleInteraction)
                    {
                        Focused = false;
                        Active = false;

                        if (m_Destroy.Value)
                        {
                            Destroy(gameObject, Delay + 0.001f); //Destroy one frame after
                        }
                    }


                    if (Cooldown > 0 && !m_Destroy.Value)
                    {
                        OnCoolDown.Invoke(true);
                        this.Delay_Action(Cooldown, () => OnCoolDown.Invoke(false));
                    }

                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>  Receive an Interaction from an gameObject </summary>
        /// <param name="InteracterID">ID of the Interacter</param>
        /// <param name="interacter">Interacter's GameObject</param>
        public bool Interact(IInteractor interacter)
        {
            if (interacter != null)
                return Interact(interacter.ID, interacter.Owner.gameObject);

            return false;
        }

        public void Interact() => Interact(-1, null);

        public virtual void Restart()
        {
            Focused = false;
            Active = true;
            CurrentActivationTime = -Cooldown;
        }

        [SerializeField] private int Editor_Tabs1;

        public void DestroyMe(float time)
        {
            Destroy(gameObject, time);
        }

    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MInteract)), CanEditMultipleObjects]
    public class MInteractEditor : UnityEditor.Editor
    {
        SerializedProperty m_ID, m_InteractorID, m_Auto, m_singleInteraction, m_Delay, m_Destroy,
            //InteractReaction, FocusedReaction, UnFocusedReaction,
            m_CoolDown, events, OnFocused, OnUnfocused, OnInteractWithGO, OnInteractWithID,
            OnCoolDown, Editor_Tabs1, Description, ShowDescription;
        protected string[] Tabs1 = new string[] { "General", "Events"/*, "Reactions" */};
        MInteract M;

        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));
        private GUIStyle style;
        private void OnEnable()
        {
            M = (MInteract)target;
            m_ID = serializedObject.FindProperty("m_ID");
            m_InteractorID = serializedObject.FindProperty("m_InteractorID");
            m_Auto = serializedObject.FindProperty("m_Auto");
            m_singleInteraction = serializedObject.FindProperty("m_singleInteraction");
            m_Delay = serializedObject.FindProperty("m_Delay");
            m_CoolDown = serializedObject.FindProperty("m_CoolDown");
            events = serializedObject.FindProperty("events");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnUnfocused = serializedObject.FindProperty("OnUnfocused");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            ShowDescription = serializedObject.FindProperty("ShowDescription");
            Description = serializedObject.FindProperty("Description");
            m_Destroy = serializedObject.FindProperty("m_Destroy");
            OnCoolDown = serializedObject.FindProperty("OnCoolDown");


            OnInteractWithGO = serializedObject.FindProperty("OnInteractWithGO");
            OnInteractWithID = serializedObject.FindProperty("OnInteractWithID");



            //  InteractReaction = serializedObject.FindProperty("InteractReaction");
            // FocusedReaction = serializedObject.FindProperty("FocusedReaction");
            // UnFocusedReaction = serializedObject.FindProperty("UnFocusedReaction");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ShowDescription.boolValue)
            {
                if (style == null)
                {
                    style = new GUIStyle(StyleBlue)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft,
                        stretchWidth = true
                    };
                    style.normal.textColor = EditorStyles.label.normal.textColor;
                }

                Description.stringValue = UnityEditor.EditorGUILayout.TextArea(Description.stringValue, style);
            }

            //MalbersEditor.DrawDescription("Interactable Element that invoke events when an Interactor interact with it");
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            if (Application.isPlaying)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ObjectField("Current Interactor",
                            M.CurrentInteractor?.Owner, typeof(GameObject), false);

                        Repaint();
                    }
                }
            }

            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawGeneral(); break;
                case 1: DrawEvents(); break;
                // case 2: DrawReactions(); break;
                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(m_InteractorID, new GUIContent("Interactor ID"));
                EditorGUILayout.PropertyField(m_ID, new GUIContent("Index"));

                EditorGUILayout.PropertyField(m_Auto, new GUIContent("Auto Interact"));
                EditorGUILayout.PropertyField(m_singleInteraction, new GUIContent("Single Interaction"));
                EditorGUILayout.PropertyField(m_Delay);
                if (!M.SingleInteraction)
                {
                    EditorGUILayout.PropertyField(m_CoolDown, new GUIContent("Cooldown"));
                }
                else
                {
                    EditorGUILayout.PropertyField(m_Destroy);
                }
            }

            EditorGUIUtility.labelWidth = 0;
        }

        //private void DrawReactions()
        //{
        //    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        //    {
        //        EditorGUILayout.PropertyField(InteractReaction);
        //        EditorGUILayout.PropertyField(FocusedReaction);
        //        EditorGUILayout.PropertyField(UnFocusedReaction);
        //    }
        //}

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnInteractWithGO);
            EditorGUILayout.PropertyField(OnInteractWithID);
            EditorGUILayout.PropertyField(OnFocused);
            EditorGUILayout.PropertyField(OnUnfocused);

            if (M.Cooldown > 0)
                EditorGUILayout.PropertyField(OnCoolDown);

            using (new EditorGUI.DisabledGroupScope(true))
                EditorGUILayout.PropertyField(events, new GUIContent("Old Events (It will removed on future Updates)"), true);
        }
    }
#endif
}