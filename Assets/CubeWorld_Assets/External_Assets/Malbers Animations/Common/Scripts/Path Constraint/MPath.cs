using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Utilities;
using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;
using System;
using MalbersAnimations.Events;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.PathCreation
{
    public enum PathFollowDir { None, Forward, Backward }

    [Serializable] public class PathConstraintEvent : UnityEvent<MPathConstraint> { }


    [AddComponentMenu("Malbers/Animal Controller/Path")]
    public class MPath : MonoBehaviour
    {
        [RequiredField]
        [Tooltip("Path Reference")]
        public IPath Path;

        [Tooltip("The Animal will align automatically when is near the Path ")]
        public BoolReference Automatic = new(true);

        [Tooltip("If the Animal is already on another Path then change to the new path. Else use the Path Input on your character")]
        public BoolReference AutoChangePath = new(true);

        //[Tooltip("Resolution to find the closest point on the path")]
        //public int m_SearchResolution = 50;

        [Tooltip("Radius to check if the Character can Enter this path")]
        [Min(0)] public float SearchRadius = 0.5f;
        [Tooltip("Orient Smothness per path")]
        [Min(0)] public float OrientSmoothness = 1f;

        [Tooltip("Offset of the Radius on the Path")]
        public Vector3 SearchOffset = new(0, 0.5f, 0);

        [Tooltip("Local Offset of the Animal Position with the Path")]
        public Vector3 AlignmentOffset = new(0, 0, 0);

        [Tooltip("Time needed so the animal can enter the same path again")]
        [Min(0)] public float pathCooldown = 1f;

        [Tooltip("The Animal Can Exit at the start of the path")]
        public BoolReference CanExitOnStart = new(true);
        [Tooltip("The Animal Can Exit at the end of the path")]
        public BoolReference CanExitOnEnd = new(true);

        [Tooltip("The Animal Can Exit at the in the middle of the path (Using Input)")]
        public BoolReference CanExitOnMiddle = new(true);

        [Tooltip("Rotate the Character using the Rotation Value Path Points")]
        public BoolReference usePathRotation = new(false);

        [Tooltip("Don't allow the Character to Rotate on the Spline... Move Backwards")]
        public BoolReference LockRotation = new(false);

        [Tooltip("Point the Animal always from Start to End of the Path")]
        public PathFollowDir FollowDirection = PathFollowDir.None;

        [Tooltip("Search for the Animal when is inside the Trigger Bounds")]
        [Min(0)] public float interval = 0.1f;

        [RequiredField, Tooltip("This trigger will activate the search when any animal had entered the trigger")]
        public BoxCollider PathBounds;

        [Min(0), Tooltip("Expand the Bounds this amount")]
        public float expand = 1f;

        [Tooltip("Layer to find the Animals")]
        public LayerReference Layer = new(1048576);


        [Tooltip("When the Animal Enters the Path, it will activate this State")]
        public StateID ActivateState;

        [Tooltip("Remove Grounded From the Animal so it Aligns Directly to the Spline Path")]
        public bool IgnoreGrounded = true;

        [Tooltip("Ignore Vertical Alignment do just Horizontal Alignment")]
        public bool IgnoreVertical = true;


        [Tooltip("If this Path is activated while the character is on another path, Ignore Old Path Reactions")]
        public bool NoExitPathReactions = false;


        [Tooltip("While the animal is on the path, all these states will be disabled")]
        public List<StateID> DisableStates;

        [Tooltip("States that can be used to exit the path or ignore the Path")]
        public List<StateID> IgnoreStates;
        [Tooltip("Modes that can be used to exit early the Path")]
        public List<ModeID> IgnoreModes;
        [Tooltip("The Animal Exit/Ignore the path if is on any Mode")]
        public BoolReference exitAnyMode = new(false);

        [SerializeField, HideInInspector] private TriggerProxy BoundsProxy;

        // private static int m_SearchRadius = -1;
        private float m_PathPosition;
        private float m_PreviousPathPosition;


        /// <summary> The path is closed/Looped </summary>
        public bool IsClosed => Path.IsClosed;

        [Tooltip("Adds a reaction to the Animal entering the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction EnterReaction;

        [Tooltip("Adds a reaction to the Animal exiting the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction ExitReaction;



        [Tooltip("Adds a reaction to the Animal entering the path from the Start point of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction EnterFromStart;

        [Tooltip("Adds a reaction to the Animal entering the path from the End point of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction EnterFromEnd;

        [Tooltip("Adds a reaction to the Animal entering the path from the middle of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction EnterFromMiddle;

        [Tooltip("Adds a reaction to the Animal exiting the path from the start of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction ExitFromStart;

        [Tooltip("Adds a reaction to the Animal exiting the path from the End of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction ExitFromEnd;

        [Tooltip("Adds a reaction to the Animal exiting the path from the Middle of the Path")]
        [SerializeReference, SubclassSelector]
        public Reaction ExitFromMiddle;

        [Tooltip("Stores the Current Path Position of the Character in a Transform")]
        public TransformReference PathPosition = new();


        public PathConstraintEvent OnEnterBounds = new();
        public PathConstraintEvent OnExitBounds = new();

        public BoolEvent CanEnterPath = new();

        public PathConstraintEvent OnEnterPath = new();
        public PathConstraintEvent OnExitPath = new();

        public BoolEvent IsOnEndOfPath = new();
        public BoolEvent IsOnStartOfPath = new();


        public bool ReachEnd { get; internal set; }
        public bool ReachStart { get; internal set; }


        public void SetEndOfPathEvent(bool v) => IsOnEndOfPath.Invoke(ReachEnd = v);
        public void SetStartOfPathEvent(bool v) => IsOnStartOfPath.Invoke(ReachStart = v);


        // [HideInInspector]
        public HashSet<MPathConstraint> ActivePathContraints = new();

        public bool debug;

        public void Debugging(string value)
        {
#if UNITY_EDITOR
            if (debug) UnityEngine.Debug.Log($"<B>[{name}]</B> → <color=white>[{value}]</color>", this);
#endif
        }

        private void Awake()
        {
            if (BoundsProxy == null && PathBounds != null)
            {
                if (!PathBounds.TryGetComponent(out BoundsProxy)) 
                    BoundsProxy = PathBounds.gameObject.AddComponent<TriggerProxy>();
            }
             
            BoundsProxy.Layer = Layer;
            PathBounds.isTrigger = true;
        }

        /// <summary>  List to access all the Paths </summary>
        public static List<MPath> Paths;

        private void OnEnable()
        {
            Paths ??= new List<MPath>();
            Paths.Add(this);

            BoundsProxy?.OnGameObjectEnter.AddListener(_OnBoundsTriggerEnter);
            BoundsProxy?.OnGameObjectExit.AddListener(_OnBoundsTriggerExit);
            
            if (!TryGetComponent(out Path))
            {
                Debugging("Path Not found. Disable All");

                enabled = false;
                PathBounds.enabled = false;
            }
        }

        private void OnDisable()
        {
            BoundsProxy?.OnGameObjectEnter.RemoveListener(_OnBoundsTriggerEnter);
            BoundsProxy?.OnGameObjectExit.RemoveListener(_OnBoundsTriggerExit);

            Paths?.Remove(this);
        }

        private void _OnBoundsTriggerEnter(GameObject gameObject)
        {
            // Debug.Log($"{name}.OnBoundsEnter [{gameObject.name}]");

            var constraint = gameObject.FindComponent<MPathConstraint>();

            if (constraint)
            {
                ActivePathContraints.Add(constraint);

                OnEnterBounds.Invoke(constraint);

                Debugging($"{name}.Constraint Detected: {constraint.name}");

                if (I_CheckInBounds == null)
                {
                    I_CheckInBounds = CheckInBounds();
                    StartCoroutine(I_CheckInBounds);
                }
            }
        }

        private void _OnBoundsTriggerExit(GameObject gameObject)
        {
            var constraint = gameObject.FindComponent<MPathConstraint>();

            if (constraint)
            {
                // Debug.Log($"{name}.OnBoundsExit [{gameObject.name}]");

                ActivePathContraints.Remove(constraint);
                OnExitBounds.Invoke(constraint);

                Debugging($"{name}.Constraint Removed: {constraint.name}");

                if (ActivePathContraints.Count == 0)
                {
                    if (I_CheckInBounds != null)
                    {
                        StopCoroutine(I_CheckInBounds);
                        I_CheckInBounds = null;
                    }
                }
            }
        }

        private IEnumerator I_CheckInBounds;

        private IEnumerator CheckInBounds()
        {
            var WaitTime = new WaitForSeconds(interval);

            while (ActivePathContraints.Count > 0)
            {
                InBounds();
                yield return WaitTime;
            }
        }


        private void Reset()
        {
            Path ??= GetComponent<IPath>();


            if (!TryGetComponent(out PathBounds))
                PathBounds = gameObject.AddComponent<BoxCollider>();


            if (BoundsProxy == null)
            {
                if (!PathBounds.TryGetComponent(out BoundsProxy))
                    BoundsProxy = PathBounds.gameObject.AddComponent<TriggerProxy>();
            }

            PathBounds.isTrigger = true;

            gameObject.SetLayer(2); //Set to IgnoreRaycast


            var newPathPosition = new GameObject("PathPosition");
            newPathPosition.transform.parent = transform;
            newPathPosition.transform.ResetLocal();
            PathPosition.Value = newPathPosition.transform;
        }

        private void OnValidate()
        {
            Path ??= GetComponent<IPath>();
        }

        internal void CalculateBounds()
        {
            OnValidate();

            if (Path == null) return;
            Bounds bounds = Path.bounds;

            bounds.Expand(2f);
            bounds.center = new Vector3(bounds.center.x, bounds.center.y + 1, bounds.center.z);

            PathBounds.size = bounds.size;
            PathBounds.center = bounds.center;
            MTools.SetDirty(PathBounds);
        }

        /// <summary>  Checks if the characters can enter a path </summary>
        public virtual bool InBounds()
        {
            foreach (var item in ActivePathContraints)
            {
                if (item.Path == this) continue;        //Meaning the Path Constraint has already this path as the active one, so skip!

                var Char_Pos = item.transform.position;

                m_PreviousPathPosition = Mathf.Clamp(m_PreviousPathPosition, 0, m_PathPosition);
                m_PathPosition = Path.GetClosestTimeOnPath(Char_Pos);

                // Quaternion newPathOrientation = m_Path.EvaluateOrientationAtUnit(m_PathPosition, t);
                Vector3 PathPos = Path.GetPointAtTime(m_PathPosition) + SearchOffset;   // Apply the offset to get the new position

                MDebug.DrawWireSphere(PathPos, Color.red, SearchRadius, interval);
                MDebug.DrawWireSphere(item.ContraintPos, Color.green, item.Radius, interval);


                if (PathPosition.Value) PathPosition.Value.position = PathPos;

                if (MTools.DoSpheresIntersect(PathPos, SearchRadius, item.ContraintPos, item.Radius)) //if the character is inside the path radius then go for it
                {
                    //Meaning the Path Constraint has already this path as the active one, so skip!
                    if (item.LastPath.Contains(this)) continue;

                    if (Automatic.Value)
                    {
                        //Change to the new path
                        if (item.Path == null || AutoChangePath.Value)
                        {
                            item.EnterPath(this);
                        }
                        else
                        {
                            item.NextPath = this; //store the Next Path the animal can be
                        }

                    }
                    else
                    {
                        if (!InsidePathSphere)
                        {
                            CanEnterPath.Invoke(true);
                            InsidePathSphere = true;
                        }

                        item.NextPath = this;
                    }
                    return true;
                }
                else
                {
                    if (InsidePathSphere)
                    {
                        CanEnterPath.Invoke(false);
                        InsidePathSphere = false;
                    }

                    //Reset the Last Past on since is not near enough clear the LastPath
                    if (item.LastPath.Contains(this)) item.LastPath.Remove(this);
                    if (item.NextPath == this) item.NextPath = null;
                }
            }

            return false;
        }


        public bool InsidePathSphere { get; set; }


        [HideInInspector, SerializeField] private int Editor_Tabs1;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (debug)
            {
                if (Path != null && !Path.IsClosed)
                {
                    Handles.color = Color.white;
                    var StartSpline = Path.StartPath;
                    var EndSpline = Path.EndPath;

                    EndSpline.y -= 0.2f;
                    StartSpline.y -= 0.2f;

                    Handles.Label(StartSpline, "Start");
                    Handles.Label(EndSpline, "End");
                }

                if (!Application.isPlaying)
                {

                    Gizmos.color = Color.red;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(SearchOffset, SearchRadius);
                }
            }


        }


        private void OnDrawGizmosSelected()
        {
            if (debug && PathBounds)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(PathBounds.center, PathBounds.size);
            }
        }
#endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MPath)), CanEditMultipleObjects]
    public class MPathEditor : Editor
    {
        string[] Tabs = new string[] { "Path", "Animal", "Reactions", "Events" };

        SerializedProperty Editor_Tabs1, Layer, //m_Path, 
            PathBounds, usePathRotation, LockRotation, FollowDirection, Automatic, AutoChangePath, ExitOnMiddle, OrientSmoothness,
            ActivateState, DisableStates, IgnoreStates, IgnoreModes, IgnoreGrounded, pathCooldown, AlignmentOffset, IgnoreVertical, PathPosition,
            SearchOffset, interval, SearchRadius, ExitOnStart, ExitOnEnd, NoExitPathReactions, //m_SearchResolution,
            EnterReaction, ExitReaction, debug, exitAnyMode;

        private MPath M;

        private void OnEnable()
        {
            M = (MPath)target;

            Automatic = serializedObject.FindProperty("Automatic");
            NoExitPathReactions = serializedObject.FindProperty("NoExitPathReactions");
            PathPosition = serializedObject.FindProperty("PathPosition");
            AutoChangePath = serializedObject.FindProperty("AutoChangePath");
            exitAnyMode = serializedObject.FindProperty("exitAnyMode");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            // m_Path = serializedObject.FindProperty("m_Path");
            SearchOffset = serializedObject.FindProperty("SearchOffset");
            pathCooldown = serializedObject.FindProperty("pathCooldown");
            FollowDirection = serializedObject.FindProperty("FollowDirection");
            OrientSmoothness = serializedObject.FindProperty("OrientSmoothness");
            SearchRadius = serializedObject.FindProperty("SearchRadius");
            // m_SearchResolution = serializedObject.FindProperty("m_SearchResolution");
            PathBounds = serializedObject.FindProperty("PathBounds");
            Layer = serializedObject.FindProperty("Layer");
            interval = serializedObject.FindProperty("interval");
            ActivateState = serializedObject.FindProperty("ActivateState");
            DisableStates = serializedObject.FindProperty("DisableStates");
            IgnoreStates = serializedObject.FindProperty("IgnoreStates");
            IgnoreModes = serializedObject.FindProperty("IgnoreModes");
            EnterReaction = serializedObject.FindProperty("EnterReaction");
            ExitReaction = serializedObject.FindProperty("ExitReaction");
            IgnoreGrounded = serializedObject.FindProperty("IgnoreGrounded");
            debug = serializedObject.FindProperty("debug");
            usePathRotation = serializedObject.FindProperty("usePathRotation");
            LockRotation = serializedObject.FindProperty("LockRotation");
            ExitOnStart = serializedObject.FindProperty("CanExitOnStart");
            ExitOnEnd = serializedObject.FindProperty("CanExitOnEnd");
            ExitOnMiddle = serializedObject.FindProperty("CanExitOnMiddle");
            AlignmentOffset = serializedObject.FindProperty("AlignmentOffset");
            IgnoreVertical = serializedObject.FindProperty("IgnoreVertical");



            EnterFromStart = serializedObject.FindProperty("EnterFromStart");
            EnterFromEnd = serializedObject.FindProperty("EnterFromEnd");
            ExitFromStart = serializedObject.FindProperty("ExitFromStart");
            ExitFromEnd = serializedObject.FindProperty("ExitFromEnd");
            EnterFromMiddle = serializedObject.FindProperty("EnterFromMiddle");
            ExitFromMiddle = serializedObject.FindProperty("ExitFromMiddle");




            OnEnterBounds = serializedObject.FindProperty("OnEnterBounds");
            OnExitBounds = serializedObject.FindProperty("OnExitBounds");
            CanEnterPath = serializedObject.FindProperty("CanEnterPath");
            OnEndOfPath = serializedObject.FindProperty("IsOnEndOfPath");
            OnStartOfPath = serializedObject.FindProperty("IsOnStartOfPath");


            OnEnterPath = serializedObject.FindProperty("OnEnterPath");
            OnExitPath = serializedObject.FindProperty("OnExitPath");
        }
        SerializedProperty EnterFromStart, EnterFromEnd, EnterFromMiddle, ExitFromStart, ExitFromEnd, ExitFromMiddle;
        SerializedProperty OnEnterBounds, OnExitBounds, CanEnterPath, OnEndOfPath, OnStartOfPath;
        SerializedProperty OnEnterPath, OnExitPath;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs);

            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawPath(); break;
                case 1: DrawAnimal(); break;
                case 2: DrawReactions(); break;
                case 3: DrawEvents(); break;

                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }


        private void DrawReactions()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Layer.isExpanded = MalbersEditor.Foldout(Layer.isExpanded, "Reactions");
                if (Layer.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(EnterReaction);
                    EditorGUILayout.PropertyField(ExitReaction);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(EnterFromStart);
                    EditorGUILayout.PropertyField(EnterFromEnd);
                    EditorGUILayout.PropertyField(EnterFromMiddle);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(ExitFromStart);
                    EditorGUILayout.PropertyField(ExitFromEnd);
                    EditorGUILayout.PropertyField(ExitFromMiddle);
                    EditorGUI.indentLevel--;
                }
            }
        }
        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                OnEnterBounds.isExpanded = MalbersEditor.Foldout(OnEnterBounds.isExpanded, "Events");
                if (OnEnterBounds.isExpanded)
                {
                    EditorGUILayout.PropertyField(OnEnterPath);
                    EditorGUILayout.PropertyField(OnExitPath);

                    EditorGUILayout.PropertyField(OnEnterBounds);
                    EditorGUILayout.PropertyField(OnExitBounds);

                    EditorGUILayout.PropertyField(CanEnterPath);
                    EditorGUILayout.PropertyField(OnStartOfPath);
                    EditorGUILayout.PropertyField(OnEndOfPath);
                }
            }
        }

        private void DrawPath()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                AutoChangePath.isExpanded = MalbersEditor.Foldout(AutoChangePath.isExpanded, "Path");
                if (AutoChangePath.isExpanded)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledGroupScope(true))
                            EditorGUILayout.ObjectField("Path", M.Path as MonoBehaviour, typeof(IPath), false);

                        if (GUILayout.Button(MalbersEditor.Icon_Update, EditorStyles.miniButton, GUILayout.Width(28), GUILayout.Height(20)))
                        {
                            M.Path = M.GetComponent<IPath>();
                            EditorUtility.SetDirty(target);
                            serializedObject.ApplyModifiedProperties();
                        }
                        MalbersEditor.DrawDebugIcon(debug);
                    }
                    EditorGUILayout.PropertyField(Automatic);
                    // EditorGUILayout.PropertyField(m_Path);
                    EditorGUILayout.PropertyField(AutoChangePath);


                    EditorGUILayout.PropertyField(LockRotation);

                    if (M.LockRotation.Value)
                        EditorGUILayout.PropertyField(FollowDirection);

                    EditorGUILayout.PropertyField(usePathRotation);

                    EditorGUILayout.PropertyField(AlignmentOffset);
                    EditorGUILayout.PropertyField(PathPosition);
                    EditorGUILayout.PropertyField(OrientSmoothness);
                }
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ExitOnEnd.isExpanded = MalbersEditor.Foldout(ExitOnEnd.isExpanded, "Exit  Path");
                if (ExitOnEnd.isExpanded)
                {
                    EditorGUILayout.PropertyField(NoExitPathReactions);
                    EditorGUILayout.PropertyField(ExitOnStart);
                    EditorGUILayout.PropertyField(ExitOnEnd);
                    EditorGUILayout.PropertyField(ExitOnMiddle);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                interval.isExpanded = MalbersEditor.Foldout(interval.isExpanded, "Search");
                if (interval.isExpanded)
                {
                    //  EditorGUILayout.PropertyField(m_SearchResolution);
                    EditorGUILayout.PropertyField(interval);
                    EditorGUILayout.PropertyField(pathCooldown);
                    EditorGUILayout.PropertyField(SearchRadius);
                    EditorGUILayout.PropertyField(SearchOffset);
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PathBounds.isExpanded = MalbersEditor.Foldout(PathBounds.isExpanded, "Trigger Bounds");
                if (PathBounds.isExpanded)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(PathBounds);

                        if (GUILayout.Button(new GUIContent("C", "Calculate Trigger Bounds"), GUILayout.Width(26)))
                        {
                            var path = (MPath)target;
                            path.CalculateBounds();
                            EditorUtility.SetDirty(target);
                            EditorUtility.SetDirty(M.PathBounds);
                        }
                    }
                    EditorGUILayout.PropertyField(Layer);
                }
            }


            if (debug.boolValue && Application.isPlaying)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        foreach (var item in M.ActivePathContraints)
                        {
                            EditorGUILayout.ObjectField(item, typeof(MPathConstraint), false);
                        }
                    }
                }
            }
        }

        private void DrawAnimal()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ActivateState.isExpanded = MalbersEditor.Foldout(ActivateState.isExpanded, "Enter Path");
                if (ActivateState.isExpanded)
                {
                    EditorGUILayout.PropertyField(ActivateState);


                    EditorGUILayout.PropertyField(IgnoreVertical);
                    EditorGUILayout.PropertyField(IgnoreGrounded);


                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(DisableStates);
                    EditorGUI.indentLevel--;
                }
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                SearchRadius.isExpanded = MalbersEditor.Foldout(SearchRadius.isExpanded, "Exit Path");

                if (SearchRadius.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(IgnoreStates);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(exitAnyMode);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(IgnoreModes);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
#endif
}