using UnityEngine;
#if UNITY_EDITOR
//CustomPatch: added extra custom editor code logic
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/ai/wander-area")]
    /// <summary>  Wander Area waypoint used on the Animal to wander around. </summary>
    [AddComponentMenu("Malbers/AI/AI Wander Area")]
    public class AIWanderArea : MWayPoint
    {
#if UNITY_EDITOR
        //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
        public bool debugPersistentGizmos;
#endif

        public enum AreaType { Circle, Box };


        [Tooltip("Type of Area to wander")]
        public AreaType m_AreaType = AreaType.Circle;

        [Min(0)] public float radius = 5;

        public Vector3 BoxArea = new Vector3(10, 1, 10);


        [Range(0, 1), Tooltip("Probability of keep wandering on this WayPoint Area")]
        public float WanderWeight = 1f;

        public Vector3 Destination { get; internal set; }

        private Transform currentNextTarget;

        // [SerializeField] private bool isChild;
        [SerializeField] private AIWanderArea MainArea;
        [SerializeField] private AIWanderArea[] ChildWanderAreas;

        bool IsChild => MainArea != this;

        protected override void OnEnable()
        {
            base.OnEnable();

            FindWanderAreas();

            if (!IsChild) GetNextDestination(); //Find the first random destination if it is a Main Wander Area
            currentNextTarget = MainArea.transform; //Store the current next target as this transform
        }

        //CustomPatch: added function to calculate bounds
        public Bounds CalculateEnclosingBounds()
        {
            if (MainArea == null)
            {
                Debug.LogError("Main wander area is null.");
                return new Bounds(); // Return an empty Bounds if invalid data
            }

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                FindWanderAreas(); // Make sure the wander areas are up to date
#endif

            // Start with the bounds of the main area itself
            Bounds enclosingBounds = CalculateAreaBounds(MainArea);

            // If the area has child wander areas, include their bounds as well
            foreach (AIWanderArea childArea in ChildWanderAreas)
            {
                Bounds childBounds = CalculateAreaBounds(childArea);
                enclosingBounds.Encapsulate(childBounds);  // Expand the bounds to include the child's bounds
            }

            return enclosingBounds;
        }

        private Bounds CalculateAreaBounds(AIWanderArea area)
        {
            // Start with the bounds of the current area
            Vector3 center = area.transform.position;
            float radius = area.radius;
            Vector3 minPoint = center - Vector3.one * radius;
            Vector3 maxPoint = center + Vector3.one * radius;

            Bounds areaBounds = new Bounds(center, maxPoint - minPoint);
            return areaBounds;
        }

        public virtual void FindWanderAreas()
        {
            MainArea = transform.parent != null ? (transform.parent.GetComponentInParent<AIWanderArea>()) : this;
            if (MainArea == null) MainArea = this; //Re-check in case this wander area is child of something else

            ChildWanderAreas = null;

            if (!IsChild)
            {
                ChildWanderAreas = GetComponentsInChildren<AIWanderArea>();
                if (ChildWanderAreas != null) foreach (var wa in ChildWanderAreas)
                    {
                        wa.DebugColor = DebugColor;
                        wa.stoppingDistance = stoppingDistance;
                    }
            }
        }

        public virtual Vector3 GetNextDestination()
        {
            if (!IsChild && ChildWanderAreas != null && ChildWanderAreas.Length > 1) //Means this area has multiple areas inside
            {
                return ChildWanderAreas[Random.Range(0, ChildWanderAreas.Length)].GetNextDestinationArea(); //Get a random point inlcuding the Main Wander Area
            }
            else
            {
                return GetNextDestinationArea();
            }
        }

        public virtual Vector3 GetNextDestinationArea()
        {
            switch (m_AreaType)
            {
                case AreaType.Circle:
                    Vector2 vector2 = (Random.insideUnitCircle * radius);
                    Destination = transform.TransformPoint(new Vector3(vector2.x, 0, vector2.y)); //Get the world position inside the circle
                    break;
                case AreaType.Box:
                    Destination = transform.TransformPoint(RandomPointInBox(BoxArea));  //Get the world position inside the Box
                    break;
                default:
                    Destination = transform.position;
                    break;
            }

            MainArea.Destination = Destination; //Super Important

            MDebug.DrawWireSphere(Destination, Color.red, 0.1f, 2);

            return MainArea.Destination;
        }

        public override Vector3 GetCenterPosition(int Index)
          => GetNextDestination();
        //  => MainArea.Destination;

        public override float StopDistance() => MainArea.stoppingDistance;

        public override float SlowDistance() => MainArea.slowingDistance;

        public override Transform NextTarget() => MainArea.FindNextTarget();

        public override void TargetArrived(GameObject target)
        {
            MainArea.OnTargetArrived.Invoke(target);
            FindNextTarget();
        }

        private Transform FindNextTarget()
        {
            if (NextTargets != null && NextTargets.Count > 0)
            {
                var probability = UnityEngine.Random.Range(0f, 1f);

                if (WanderWeight != 0 && probability <= WanderWeight) //Find the next destination on the same wander Area.
                {
                    GetNextDestination();
                    currentNextTarget = MainArea.transform;  //Keep itself as the target
                }
                else //Find the next on one of the Next Targets.
                {
                    currentNextTarget = NextTargets[UnityEngine.Random.Range(0, NextTargets.Count)]; //Get the next target
                }
            }
            else
            {
                currentNextTarget = MainArea.transform;  //Keep itself as the target
            }

            return currentNextTarget;
        }

        private Vector3 RandomPointInBox(Vector3 size)
        {
            return new Vector3(
                (Random.value - 0.5f) * size.x,
                (Random.value - 0.5f) * size.y,
                (Random.value - 0.5f) * size.z);
        }

        [HideInInspector, SerializeField] private bool ShowRadius;

        private void Reset()
        {
            DebugColor.a = 0.2f;
        }

        private void OnValidate()
        {
            FindWanderAreas(); //for the colors

            if (BoxArea.x < 0) BoxArea.x = 0;
            if (BoxArea.y < 0) BoxArea.y = 0;
            if (BoxArea.z < 0) BoxArea.z = 0;
        }

#if UNITY_EDITOR
        //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
        private bool isGizmoSelected = true;


        private void OnDrawGizmos()
        {
            //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
            if (!isGizmoSelected && !debugPersistentGizmos)
                return;

            var DebugColorWire = DebugColor;
            DebugColorWire.a = 1;


            UnityEditor.Handles.color = DebugColorWire;
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, stoppingDistance);
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, slowingDistance);

            switch (m_AreaType)
            {
                case AreaType.Circle:
                    UnityEditor.Handles.color = DebugColorWire;
                    UnityEditor.Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
                    UnityEditor.Handles.color = DebugColor;
                    UnityEditor.Handles.DrawSolidDisc(Vector3.zero, Vector3.up, radius);
                    break;
                case AreaType.Box:

                    var sizeX = transform.lossyScale.x * BoxArea.x;
                    var sizeY = transform.lossyScale.y * BoxArea.y;
                    var sizeZ = transform.lossyScale.z * BoxArea.z;

                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.color = DebugColor;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    Gizmos.color = DebugColorWire;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    break;
            }

            //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
            isGizmoSelected = false;
        }


        private void OnDrawGizmosSelected()
        {
            //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
            isGizmoSelected = true;

            var DebugColorWire = DebugColor;
            DebugColorWire.a = 1;
            Gizmos.color = DebugColorWire;

            Gizmos.DrawRay(transform.position, transform.up * Height);
            Gizmos.DrawWireSphere(transform.position + transform.up * Height, Height * 0.1f);
            Gizmos.DrawWireSphere(transform.position, Height * 0.1f);

            if (nextWayPoints != null)
            {
                foreach (var item in nextWayPoints)
                {
                    if (item) Gizmos.DrawLine(transform.position, item.position);
                }
            }
        }
#endif
    }

    //INSPECTOR--------------


    #region Inspector


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AIWanderArea))]
    [UnityEditor.CanEditMultipleObjects]
    public class AIWanderAreaEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty
            pointType, stoppingDistance, slowingDistance, m_AreaType, m_height,
            radius, BoxArea, WaitTime, WanderWeight, nextWayPoints, DebugColor, OnTargetArrived;

        //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
        SerializedProperty debugPersistentGizmosProperty;
        AIWanderArea M;

        private bool isChild;

        private void OnEnable()
        {
            //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
            debugPersistentGizmosProperty = serializedObject.FindProperty(nameof(M.debugPersistentGizmos));

            M = (AIWanderArea)target;
            pointType = serializedObject.FindProperty("pointType");
            stoppingDistance = serializedObject.FindProperty("stoppingDistance");
            slowingDistance = serializedObject.FindProperty("slowingDistance");
            m_AreaType = serializedObject.FindProperty("m_AreaType");
            radius = serializedObject.FindProperty("radius");
            BoxArea = serializedObject.FindProperty("BoxArea");
            WaitTime = serializedObject.FindProperty("m_WaitTime");
            WanderWeight = serializedObject.FindProperty("WanderWeight");
            nextWayPoints = serializedObject.FindProperty("nextWayPoints");
            DebugColor = serializedObject.FindProperty("DebugColor");
            OnTargetArrived = serializedObject.FindProperty("OnTargetArrived");
            m_height = serializedObject.FindProperty("m_height");

            isChild = M.transform.parent != null && (M.transform.parent.GetComponentInParent<AIWanderArea>() != null);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Type of Waypoint that uses an Area to get the Destination point");
            if (!isChild)
            {
                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                {
                    //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
                    UnityEditor.EditorGUILayout.PropertyField(debugPersistentGizmosProperty);

                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.PropertyField(pointType);
                    UnityEditor.EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.Width(40));
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUILayout.PropertyField(m_height);
                    UnityEditor.EditorGUILayout.PropertyField(stoppingDistance);
                    UnityEditor.EditorGUILayout.PropertyField(slowingDistance);
                    UnityEditor.EditorGUILayout.PropertyField(WaitTime);
                }
                UnityEditor.EditorGUILayout.EndVertical();
            }
            UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            {
                //CustomPatch: added toggle to optionally persist the gizmos drawing of this wander area and all its children
                if (isChild)
                    UnityEditor.EditorGUILayout.PropertyField(debugPersistentGizmosProperty);

                UnityEditor.EditorGUILayout.PropertyField(m_AreaType);
                var aretype = (AIWanderArea.AreaType)m_AreaType.intValue;

                switch (aretype)
                {
                    case AIWanderArea.AreaType.Circle:
                        UnityEditor.EditorGUILayout.PropertyField(radius);

                        break;
                    case AIWanderArea.AreaType.Box:
                        UnityEditor.EditorGUILayout.PropertyField(BoxArea);
                        break;
                    default:
                        break;
                }

            }
            UnityEditor.EditorGUILayout.EndVertical();

            if (isChild)
            {
                UnityEditor.EditorGUILayout.HelpBox("Type, Stop Distance, Wait Time, and Next Destination properties are handled by the parent Wander Area",
                    UnityEditor.MessageType.Info);
            }
            else
            {
                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                {
                    UnityEditor.EditorGUILayout.LabelField("Next Destination", UnityEditor.EditorStyles.boldLabel);
                    UnityEditor.EditorGUILayout.PropertyField(WanderWeight);
                    UnityEditor.EditorGUI.indentLevel++;
                    UnityEditor.EditorGUILayout.PropertyField(nextWayPoints, true);
                    UnityEditor.EditorGUI.indentLevel--;
                }
                UnityEditor.EditorGUILayout.EndVertical();
                UnityEditor.EditorGUILayout.PropertyField(OnTargetArrived);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    #endregion
}