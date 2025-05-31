using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Reactions;
using System.Collections;





#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/Animal Tracker")]
    [DefaultExecutionOrder(10000)]
    public class AnimalTracker : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;
        [RequiredField] public Transform Tracker;
        [Tooltip("Unparent the Tracker")]
        public bool NoParent = true;

        [Tooltip("Use FixedUpdate instead of Update")]
        public bool FixedUpdate = false;

        private float CurrentLerp;


        public List<TransformTracker> Trackers = new();


        public float DebugSize = 0.05f;


        private TransformTracker Current;
        public int CurrentIndex { get; private set; }

        /// <summary> Store the Current State  </summary>
        public int CurrentState { get; private set; }

        /// <summary> Store the Current State  </summary>
        public int LastState { get; private set; }
        /// <summary> Store the Current State  </summary>
        public int LastStance { get; private set; }
        /// <summary> Store the Current State  </summary>
        public int CurrentStance { get; private set; }

        /// <summary> Store the Current State  </summary>
        public int CurrentStartMode { get; private set; }

        /// <summary> Store the Current State  </summary>
        public int CurrentExitMode { get; private set; }


        private void OnEnable()
        {
            animal.OnStateChange.AddListener(OnStateChange);
            animal.OnStanceChange.AddListener(OnStanceChange);
            animal.OnModeStart.AddListener(OnModeStart);
            animal.OnModeEnd.AddListener(OnModeEnd);

            //Cast the first state and stance



            if (NoParent) Tracker.SetParent(null);


            Initialize();

            if (FixedUpdate)
                StartCoroutine(UpdateCycleFixed());
            else
                StartCoroutine(UpdateCycle());


            // OnStateChange(animal.ActiveStateID);
            OnStanceChange(animal.ActiveStance.ID);

            CurrentLerp = 0;
        }

        private IEnumerator UpdateCycleFixed()
        {
            var fixedTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return fixedTime;
                UpdateTrackerPos(Time.fixedDeltaTime);
            }
        }

        private IEnumerator UpdateCycle()
        {
            var fixedTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return null;
                UpdateTrackerPos(Time.deltaTime);
            }
        }

        private void Initialize()
        {
            foreach (var item in Trackers)
            {
                if (item.RelativeTo == null || item.RelativeTo == transform)
                    item.RelativeTo = transform.parent; //Set the Relative To to the Tracker if is null
            }
        }

        private void OnDisable()
        {
            animal.OnStateChange.RemoveListener(OnStateChange);
            animal.OnStanceChange.RemoveListener(OnStanceChange);
            animal.OnModeStart.RemoveListener(OnModeStart);
            animal.OnModeEnd.RemoveListener(OnModeEnd);

            StopAllCoroutines();
        }

        private void OnStateChange(int state)
        {
            FindState(state);

            LastState = CurrentState; //Store the Last State
            CurrentState = state; //Store the Current Entering State
        }


        private void OnStanceChange(int stance)
        {
            var found = FindStance(stance);

            LastStance = CurrentStance; //Store the Last Stance
            CurrentStance = stance;

            if (!found) FindState(CurrentState); //When a Stance changes recheck the current tracker.
        }

        private bool FindState(int state)
        {
            bool Found = false;

            for (int i = 0; i < Trackers.Count; i++)
            {
                var item = Trackers[i];

                if (!item.Active) continue;

                bool ValidState = item.CheckState && item.State.ID == state;
                bool ValidStance = !item.CheckStance || (item.CheckStance && item.Stance == CurrentStance);

                if (ValidState && ValidStance)
                {
                    if (!Found && item.RepositionTracker)
                    {
                        // Debug.Log($"FindState: {item.name}");
                        item.reaction?.React(animal);
                        Found = true;
                        //Update the Reposition Tracker 
                        if (item.RepositionTracker)
                        {
                            CurrentLerp = 0;
                            CurrentIndex = i;
                            Current = item;
                        }
                    }
                }
            }

            return Found;
        }

        private bool FindStance(int stance)
        {
            bool Found = false;

            for (int i = 0; i < Trackers.Count; i++)
            {
                var item = Trackers[i];
                if (!item.Active) continue;

                bool ValidStance = item.CheckStance && item.Stance.ID == stance;
                bool ValidState = !item.CheckState || (item.CheckState && item.State == CurrentState);

                if (ValidState && ValidStance)
                {
                    if (!Found)
                    {
                        //  Debug.Log($"FindStance {item.name}");
                        item.reaction?.React(animal);
                        Found = true;

                        //Update the Reposition Tracker 
                        if (item.RepositionTracker)
                        {
                            CurrentLerp = 0;
                            CurrentIndex = i;
                            Current = item;
                        }
                    }
                }
            }
            return Found;
        }

        private bool FoundMode;

        private void OnModeStart(int Mode, int Ability)
        {
            for (int i = 0; i < Trackers.Count; i++)
            {
                var item = Trackers[i];
                if (item.ModeAction == TransformTracker.ModeStatus.Start)
                {
                    bool ValidState = !item.CheckState || (item.CheckState && item.State == CurrentState);
                    bool ValidStance = !item.CheckStance || (item.CheckStance && item.Stance == CurrentStance);

                    if (item.CheckMode && item.Mode.ID == Mode && ValidState && ValidStance)
                    {
                        if (!item.CheckAbility || (item.CheckAbility && item.Ability == Ability))
                        {
                            FoundMode = true;
                            //item.OnTrackerChanged.Invoke();
                            // Debug.Log($"OnModeStart {item.name}");
                            item.reaction?.React(animal);

                            //Update the Reposition Tracker 
                            if (item.RepositionTracker)
                            {
                                CurrentLerp = 0;
                                CurrentIndex = i;
                                Current = item;
                            }
                        }
                    }
                }
            }
        }

        private void OnModeEnd(int Mode, int Ability)
        {
            foreach (var item in Trackers)
            {
                if (item.ModeAction == TransformTracker.ModeStatus.Exit)
                {
                    bool ValidState = !item.CheckState || (item.CheckState && item.State == CurrentState);
                    bool ValidStance = !item.CheckStance || (item.CheckStance && item.Stance == CurrentStance);

                    if (item.CheckMode && item.Mode.ID == Mode && ValidState && ValidStance)
                    {
                        if (!item.CheckAbility || (item.CheckAbility && item.Ability == Ability))
                        {
                            // item.OnTrackerChanged.Invoke();
                            item.reaction?.React(animal);
                            // Debug.Log($"OnModeEnd {item.name}");
                            CurrentLerp = 0;

                        }
                    }
                }
            }

            if (FoundMode)
                FindState(CurrentState); //When a Stance changes recheck the current tracker.


            FoundMode = false;
        }

        public void SetAnimal(GameObject go)
        {
            if (animal != go)
            {
                animal.OnStateChange.RemoveListener(OnStateChange);
                animal.OnStanceChange.RemoveListener(OnStanceChange);
                animal.OnModeStart.RemoveListener(OnModeStart);
                animal.OnModeEnd.RemoveListener(OnModeEnd);
            }

            if (go.TryGetComponent(out animal))
            {
                animal.OnStateChange.AddListener(OnStateChange);
                animal.OnStanceChange.AddListener(OnStanceChange);
                animal.OnModeStart.AddListener(OnModeStart);
                animal.OnModeEnd.AddListener(OnModeEnd);


                //Cast the first state and stance
                OnStateChange(animal.ActiveStateID);
                OnStanceChange(animal.ActiveStance.ID);
            }
        }
        public void SetAnimal(Component an) => SetAnimal(an.gameObject);


        private void UpdateTrackerPos(float deltatime)
        {
            if (Current == null || Tracker == null) return;

            CurrentLerp = Mathf.Lerp(CurrentLerp, Current.Lerp, deltatime * Current.Lerp);

            Tracker.SetPositionAndRotation(
                Vector3.Slerp(Tracker.position, Current.RelativeTo.TransformPoint(Current.Position), deltatime * CurrentLerp),
                Quaternion.Slerp(Tracker.rotation, Current.RelativeTo.rotation * Quaternion.Euler(Current.Rotation), deltatime * CurrentLerp));
        }

        private void Reset()
        {
            animal = this.FindComponent<MAnimal>();

            Trackers = new List<TransformTracker>(1)
            {
                new TransformTracker()
                {
                    Active = true,
                    RelativeTo = transform,
                    State = MTools.GetInstance<StateID>("Idle"),
                    Stance = MTools.GetInstance<StanceID>("Default"),
                    Ability = -1,
                    Lerp = 2,
                    Position = new Vector3(0, 0, 0.25f),
                    Rotation = new Vector3(0, 0, 0)
                }
            };
        }


        private void OnValidate()
        {
            foreach (var item in Trackers)
            {
                var newName = "";
                if ((item.track & TrackerType.State) == TrackerType.State)
                {
                    newName += " [" + (item.State != null ? item.State.name : "NONE") + "]";
                }

                if ((item.track & TrackerType.Stance) == TrackerType.Stance)
                {
                    newName += " [" + (item.State != null ? item.Stance.name : "NONE") + "]";
                }

                if ((item.track & TrackerType.Mode) == TrackerType.Mode)
                {
                    newName += " [" + (item.Mode != null ? item.Mode.name : "NONE") + "]";
                }

                if ((item.track & TrackerType.Ability) == TrackerType.Ability)
                {
                    newName += " Ability [" + (item.Ability) + "]";
                }

                item.name = newName;


                if (item.RelativeTo == null || item.RelativeTo == transform)
                    item.RelativeTo = transform.parent; //Set the Relative To to the Tracker if is null
            }
        }

        [HideInInspector, SerializeField]
        private int selectedIndex;

        private void OnDrawGizmosSelected()
        {
            foreach (var item in Trackers)
            {
                if (item.RelativeTo == null || !item.RepositionTracker) continue;

                Gizmos.color = item.Active ? item.DebugColor : Color.red;
                var oldMatrix = Gizmos.matrix;

                Gizmos.matrix = Matrix4x4.TRS(item.RelativeTo.TransformPoint(item.Position), item.RelativeTo.rotation * Quaternion.Euler(item.Rotation), Vector3.one);
                Gizmos.DrawCube(Vector3.zero, Vector3.one * DebugSize);
                Gizmos.matrix = oldMatrix;
                Gizmos.DrawLine(item.RelativeTo.TransformPoint(item.Position), item.RelativeTo.position);
            }
        }
    }

    public enum TrackerType { State = 1, Stance = 2, Mode = 4, Ability = 8 }

    [System.Serializable]
    public class TransformTracker
    {
        [HideInInspector]
        public string name;
        public enum ModeStatus { Start, Exit }

        [HideInInspector] public bool Active;

        [Flag, HideInInspector]
        public TrackerType track = TrackerType.State;

        [Hide(nameof(track), false, true, true, (int)TrackerType.State)]
        public StateID State;
        [Hide(nameof(track), false, true, true, (int)TrackerType.Stance)]
        public StanceID Stance;
        [Hide(nameof(track), false, true, true, (int)TrackerType.Mode, (int)TrackerType.Ability)]
        public ModeID Mode;
        [Hide(nameof(track), false, true, true, (int)TrackerType.Ability)]
        [Min(-1)] public int Ability = -1;

        [Hide(nameof(track), false, true, true, (int)TrackerType.Mode, (int)TrackerType.Ability)]
        public ModeStatus ModeAction = ModeStatus.Start;

        public bool CheckStance => (track & TrackerType.Stance) == TrackerType.Stance;
        public bool CheckState => (track & TrackerType.State) == TrackerType.State;
        public bool CheckMode => (track & TrackerType.Mode) == TrackerType.Mode;
        public bool CheckAbility => (track & TrackerType.Ability) == TrackerType.Ability;



        //public float CurrentLerp;

        [Space]
        [SerializeReference, SubclassSelector]
        public Reaction reaction;
        // public UnityEvent OnTrackerChanged = new();



        // [Tooltip("Reposition the Tracker GameObject using the Position and Rotation offsets when the tracker is active ")]
        [HideInInspector]
        public bool RepositionTracker = true;

        [Hide(nameof(RepositionTracker), false, true)]
        [Tooltip("Lerp Speed to the new Tracker Position")]
        public float Lerp = 2;

        [Hide(nameof(RepositionTracker), false, true)]
        public Vector3 Position;
        [Hide(nameof(RepositionTracker), false, true)]
        public Vector3 Rotation;

        [Hide(nameof(RepositionTracker), false, true)]
        [Tooltip("Use this Node Transform to a of the Tracker Object. Reference for the Local Position of the tracker")]
        public Transform RelativeTo;
        [Hide(nameof(RepositionTracker), false, true)]
        public Color DebugColor = Color.green;

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(AnimalTracker))]
    public class AnimatorTrackerTransformEditor : UnityEditor.Editor
    {
        private AnimalTracker M;

        private SerializedProperty animal, Tracker, NoParent, Trackers, DebugSize, selectedIndex, FixedUpdate;

        private ReorderableList Reo_List_Trackers;

        private readonly Color selectedColor = new(0.4f, 1f, 2f, 1f);

        private void OnEnable()
        {
            M = (AnimalTracker)target;

            animal = serializedObject.FindProperty("animal");
            Tracker = serializedObject.FindProperty("Tracker");
            NoParent = serializedObject.FindProperty("NoParent");
            Trackers = serializedObject.FindProperty("Trackers");
            DebugSize = serializedObject.FindProperty("DebugSize");
            selectedIndex = serializedObject.FindProperty("selectedIndex");
            selectedIndex = serializedObject.FindProperty("selectedIndex");
            FixedUpdate = serializedObject.FindProperty("FixedUpdate");

            Reo_List_Trackers = new(serializedObject, Trackers, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = DrawHeaderCallback,
                onSelectCallback = OnSelectCallback,
            };


            Reo_List_Trackers.index = selectedIndex.intValue;
        }

        private void OnSelectCallback(ReorderableList list)
        {
            selectedIndex.intValue = list.index;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            float height = EditorGUIUtility.singleLineHeight;
            Rect rect1 = new Rect(rect.x + 30, rect.y + 2, rect.width / 2, height);
            Rect rect2 = new Rect(rect.x + 20 + rect.width / 2, rect.y + 2, rect.width / 2 - 40, height);
            Rect rect3 = new Rect(rect.width - 40, rect.y + 2, 70, height);

            EditorGUI.LabelField(rect1, "   Tracking Action");
            EditorGUI.LabelField(rect2, "   Description");
            EditorGUI.LabelField(rect3, new GUIContent("Reposition", "Reposition the Tracker GameObject using the Position and Rotation offsets when the tracker is active "));
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            Rect rect0 = new Rect(rect.x, rect.y + 2, 20, height);
            Rect rect1 = new Rect(rect.x + 20, rect.y + 2, rect.width / 2, height);
            Rect rect2 = new Rect(rect.x + 20 + rect.width / 2, rect.y + 2, rect.width / 2 - 20, height);
            Rect rect3 = new Rect(rect.width + 20, rect.y + 2, 20, height);

            // Use rect1 and rect2 as needed

            var element = Trackers.GetArrayElementAtIndex(index);


            var track = element.FindPropertyRelative("track");
            var name = element.FindPropertyRelative("name");
            var active = element.FindPropertyRelative("Active");
            var RepositionTracker = element.FindPropertyRelative("RepositionTracker");


            var OldColor = GUI.backgroundColor;
            var OldColorRuntime = GUI.color;

            if (Application.isPlaying)
            {
                GUI.color = M.CurrentIndex == index ? selectedColor : OldColor;
            }



            GUI.backgroundColor = index == selectedIndex.intValue ? selectedColor : OldColor;


            EditorGUI.PropertyField(rect0, active, GUIContent.none);

            EditorGUIUtility.labelWidth = 35;
            EditorGUI.PropertyField(rect1, track);
            EditorGUIUtility.labelWidth = 0;
            EditorGUI.LabelField(rect2, name.stringValue, EditorStyles.boldLabel);
            EditorGUI.PropertyField(rect3, RepositionTracker, GUIContent.none);

            GUI.backgroundColor = OldColor;
            GUI.color = OldColorRuntime;

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Track an Animal States,Stances and Modes and react to it.");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(animal);
                EditorGUILayout.PropertyField(Tracker);
                EditorGUILayout.PropertyField(FixedUpdate);

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(NoParent);

                    EditorGUIUtility.labelWidth = 40;
                    EditorGUILayout.PropertyField(DebugSize, GUILayout.MaxWidth(80));
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            Reo_List_Trackers.DoLayoutList();

            Reo_List_Trackers.index = selectedIndex.intValue;

            var index = selectedIndex.intValue;


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (index != -1 && index < Trackers.arraySize)
                {
                    var element = Trackers.GetArrayElementAtIndex(index);
                    if (element != null)
                        EditorGUILayout.PropertyField(element);
                }
            }


            //  base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            var index = selectedIndex.intValue;

            if (index == -1) return;

            if (M.Trackers.Count < index)
            {
                var item = M.Trackers[index];

                var transform = item.RelativeTo;

                if (!item.RepositionTracker) return;

                //if (transform == null) continue;
                //if (!item.RepositionTracker) continue;

                var Rotation = Tools.pivotRotation == PivotRotation.Local ? transform.rotation : Quaternion.identity;

                if (Tools.current == Tool.Move)
                {
                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        Vector3 piv = transform.TransformPoint(item.Position);
                        Vector3 NewPivPosition = Handles.PositionHandle(piv, Rotation);

                        if (cc.changed)
                        {
                            Undo.RecordObject(target, "Change Pos Goal");
                            item.Position = transform.InverseTransformPoint(NewPivPosition);
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
                else if (Tools.current == Tool.Rotate)
                {
                    //Rotation = Quaternion.identity;

                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        Quaternion rot = Rotation * Quaternion.Euler(item.Rotation);
                        Vector3 piv = transform.TransformPoint(item.Position);
                        Quaternion NewRotation = Handles.RotationHandle(rot, piv);

                        if (cc.changed)
                        {
                            Undo.RecordObject(target, "Change Rot Goal");

                            NewRotation = Quaternion.Inverse(Rotation) * NewRotation;

                            item.Rotation = NewRotation.eulerAngles;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
            }
        }
    }
#endif
}
