using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using System.Collections.Generic;
using MalbersAnimations.Reactions;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Interaction/Interactor")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/interactor")]
    public class MInteractor : MonoBehaviour, IInteractor
    {
        [Tooltip("Layer for the Interact with colliders")]
        [SerializeField] private LayerReference Layer = new(-1);
        [SerializeField] private QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.Ignore;


        [Tooltip("ID for the Interactor")]
        public IntReference m_ID = new(0);

        [Tooltip("Collider set as Trigger to Find Interactables OnTrigger Enter")]
        //[RequiredField] 
        public Collider InteractionArea;

        [Tooltip("When an Interaction is executed these events will be invoked." +
         "\n\nOnInteractWithGO(GameObject) -> will have the *INTERACTABLE* gameObject as parameter" +
         "\n\nOnInteractWith(Int) -> will have the *INTERACTABLE* ID as parameter")]
        public InteractionEvents events = new();
        public GameObjectEvent OnFocused = new();
        public GameObjectEvent OnUnfocused = new();

        public System.Action<GameObject, int> OnFocusing;
        public System.Action<GameObject, int> OnUnFocusing;

        //public GameObjectEvent OnFocusing => OnFocused;
        //public GameObjectEvent OnUnFocusing => OnUnfocused;

        public int ID => m_ID.Value;

        public bool Enabled { get => !enabled; set => enabled = !value; }

        public GameObject Owner => RealRoot.gameObject;

        /// <summary>Current Interactable this interactor has on its Interaction Area </summary>
        public List<IInteractable> FocusedInteractables;


        /// <summary>Interaction Trigger Proxy to Subsribe to OnEnter OnExit Trigger</summary>
        public TriggerProxy Proxy { get; set; }



        public List<MInteractorReaction> reactions = new();
        private Transform RealRoot;

        public bool debug;

        private void OnValidate()
        {
            if (InteractionArea != null) { InteractionArea.isTrigger = true; }
        }


        private void OnEnable()
        {
            FocusedInteractables = new List<IInteractable>();

            RealRoot = transform.FindObjectCore();

            Proxy = TriggerProxy.CheckTriggerProxy(InteractionArea, Layer, TriggerInteraction, RealRoot);

            if (Proxy)
            {
                Proxy.OnTrigger_Enter.AddListener(TriggerEnter);
                Proxy.OnTrigger_Exit.AddListener(TriggerExit);
            }
        }

        private void OnDisable()
        {
            var focusCache = FocusedInteractables.ToArray(); //Cache in case the List changes (Crazy Error)

            foreach (var item in focusCache) UnFocus(item);


            FocusedInteractables = null;

            if (Proxy)
            {
                Proxy.OnTrigger_Enter.RemoveListener(TriggerEnter);
                Proxy.OnTrigger_Exit.RemoveListener(TriggerExit);
            }
        }

        private void TriggerEnter(Collider collider)
        {
            if (collider.isTrigger && TriggerInteraction == QueryTriggerInteraction.Ignore) return;    //Skip colliders

            var NewInteractables = collider.FindInterfaces<IInteractable>(); //Find all Interactables

            if (NewInteractables != null)
                foreach (var item in NewInteractables)
                {
                    if (FocusedInteractables.Contains(item)) continue; //The new interactable its already there
                    Focus(item);
                }
        }

        private void TriggerExit(Collider collider)
        {
            if (collider != null)
            {
                var NewInteractabless = collider.FindInterfaces<IInteractable>();

                if (NewInteractabless != null)
                    foreach (var item in NewInteractabless)
                    {
                        if (item != null && FocusedInteractables.Contains(item)) //means the interactor is exiting
                            UnFocus(item);
                    }
            }
        }


        public virtual void Focus(IInteractable item)
        {
            if (item != null && item.Active) //Ignore One Disable Interactors
            {
                item.CurrentInteractor = this;
                OnFocused.Invoke(item.Owner);

                OnFocusing?.Invoke(item.Owner, item.Index);

                item.Focused = true;
                FocusedInteractables.Add(item);
                if (item.Auto) Interact(item); //Interact if the interacter is on Auto
            }
        }

        public virtual void Focus(Component item)
        {
            if (item is IInteractable) Focus(item as IInteractable);
        }

        public virtual void Focus(GameObject item)
        {
            if (item != null) Focus(item.FindInterface<IInteractable>());
        }

        public void UnFocus(IInteractable item)
        {
            if (item != null)
            {
                if (item.Owner)
                {
                    OnUnfocused.Invoke(item.Owner);
                    OnUnFocusing?.Invoke(item.Owner, item.Index);
                }

                item.Focused = false;
                item.CurrentInteractor = null;
                FocusedInteractables.Remove(item);
            }
        }


        /// <summary> Receive an Interaction from the Interacter </summary>
        public bool Interact(IInteractable inter)
        {
            if (inter.Interact(this))
            {
                events.OnInteractWithGO.Invoke(inter.Owner);
                events.OnInteractWith.Invoke(inter.Index);

                foreach (var r in reactions)
                {
                    r.React(inter.Index);
                }

                if (debug) Debug.Log($"{RealRoot.name} -> Interact ({inter.Index} : {inter.Owner.name})", this);
                return true;
            }
            return false;
        }


        public void Interact()
        {
            var focusCache = FocusedInteractables.ToArray(); //Cache in case the List changes (Crazy Error)
            foreach (var item in focusCache)
                Interact(item);
        }

        public void Restart()
        {
            FocusedInteractables = new List<IInteractable>();
            OnUnfocused.Invoke(null);
            OnFocused.Invoke(null);

            OnUnFocusing?.Invoke(null, -1);
            OnFocusing?.Invoke(null, -1);
        }

        public void Interact(GameObject interactable)
        {
            if (interactable)
                Interact(interactable.FindInterface<IInteractable>());
        }

        public void Interact(Component interactable)
        {
            if (interactable)
                Interact(interactable.FindInterface<IInteractable>());
        }

        [SerializeField] private int Editor_Tabs1;
    }


    [System.Serializable]
    public class MInteractorReaction
    {
        public ComparerInt Is = ComparerInt.Equal;
        [Tooltip("Interactable Index. Set it to Zero or 1 to use this reaction with all Interactables")]
        public IntReference Index = new();
        public Component target;
        [SerializeReference, SubclassSelector]
        public Reaction reaction;


        public void React(int newInteractable)
        {
            if (reaction != null)
            {
                //if newInteractable is -1 mean is a interactable that all interactors can interact
                if (Index.Value <= 0 || Index.Value.CompareInt(newInteractable, Is))
                {
                    target = reaction.VerifyComponent(target);
                    reaction.TryReact(target);
                }
            }
            else
            {
                Debug.LogError("Reaction is Empty. Please use any reaction");
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MInteractor))]
    public class MInteractorEditor : UnityEditor.Editor
    {
        SerializedProperty m_ID, InteractionArea, events, Editor_Tabs1, OnFocusedInteractable, OnUnfocusedInteractable, reactions, debug,
            triggerInteraction, Layer;
        protected string[] Tabs1 = new string[] { "General", "Events", "Reactions" };

        MInteractor M;

        private void OnEnable()
        {
            M = (MInteractor)target;
            m_ID = serializedObject.FindProperty("m_ID");
            InteractionArea = serializedObject.FindProperty("InteractionArea");
            events = serializedObject.FindProperty("events");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            OnFocusedInteractable = serializedObject.FindProperty("OnFocused");
            OnUnfocusedInteractable = serializedObject.FindProperty("OnUnfocused");
            Layer = serializedObject.FindProperty("Layer");
            triggerInteraction = serializedObject.FindProperty("TriggerInteraction");
            reactions = serializedObject.FindProperty("reactions");
            debug = serializedObject.FindProperty("debug");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Interactor element that invoke events when interacts with an Interactable");

            using (new GUILayout.HorizontalScope())
            {
                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
                MalbersEditor.DrawDebugIcon(debug);
            }
            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawGeneral(); break;
                case 1: DrawEvents(); break;
                case 2: DrawReactions(); break;
                default: break;
            }


            if (Application.isPlaying)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    if (M.FocusedInteractables != null)
                    {
                        foreach (var item in M.FocusedInteractables)
                        {
                            EditorGUILayout.ObjectField($"Focused Item [ID:{item.Index}]", item.Owner, typeof(GameObject), false);
                        }
                    }
                }
                Repaint();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawReactions()
        {
            EditorGUILayout.PropertyField(reactions);
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(Layer);
                EditorGUILayout.PropertyField(triggerInteraction);
                EditorGUILayout.PropertyField(m_ID);
                EditorGUILayout.PropertyField(InteractionArea);
            }
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(events);

            if (events.isExpanded)
            {
                EditorGUILayout.PropertyField(OnFocusedInteractable);
                EditorGUILayout.PropertyField(OnUnfocusedInteractable);
            }
        }
    }
#endif
}