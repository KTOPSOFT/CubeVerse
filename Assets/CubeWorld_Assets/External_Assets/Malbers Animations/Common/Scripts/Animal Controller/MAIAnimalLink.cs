using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Reactions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller.AI
{
    [AddComponentMenu("Malbers/AI/AI Animal Link")]
    public class MAIAnimalLink : MonoBehaviour
    {
        public static List<MAIAnimalLink> OffMeshLinks;
        public bool BiDirectional = true;


        [SerializeReference, SubclassSelector]
        public Reaction StartReaction;
        [SerializeReference, SubclassSelector]
        public Reaction EndReaction;
        public Color DebugColor = Color.yellow;


        public float StoppingDistance = 1f;
        public float SlowingDistance = 1f;
        public float SlowingLimit = 0.3f;

        public bool AlignToLink = true;
        public float AlignTime = 0.2f;

        public bool ForwardToVertical;

        [Tooltip("OffMesh Start Link Transform For aligning when the Character is near the start point")]
        [RequiredField] public Transform start;
        [Tooltip("OffMesh End Link Transform")]
        [RequiredField] public Transform end;

        [Tooltip("Input Axis Mode instead of Direction to Move. Use this for Climb")]
        public bool UseInputAxis;


        public bool debug = true;

        //#if UNITY_6000_0_OR_NEWER
        //        NavMeshLink meshLink;
        //#else
        //        UnityEngine.AI.OffMeshLink meshLink;
        //#endif

        //        private void Awake()
        //        {
        //#if UNITY_6000_0_OR_NEWER
        //            meshLink = GetComponent<NavMeshLink>();
        //#else
        //            meshLink = GetComponent<UnityEngine.AI.OffMeshLink>();
        //#endif
        //        }

        protected virtual void OnEnable()
        {
            OffMeshLinks ??= new List<MAIAnimalLink>();
            OffMeshLinks.Add(this);
        }

        protected virtual void OnDisable()
        {
            OffMeshLinks.Remove(this);
        }

        public virtual void Execute(IAIControl ai, MAnimal animal, Vector3 StartPoint, Vector3 EndPoint)
        {
            animal.StartCoroutine(OffMeshMove(ai, animal, StartPoint, EndPoint));
        }

        /// <summary>  AI START Pathfinding Coroutine </summary>
        public IEnumerator Coroutine_Execute(IAIControl ai, MAnimal animal, Vector3 StartPoint, Vector3 EndPoint)
        {
            yield return OffMeshMove(ai, animal, StartPoint, EndPoint);
        }


        private IEnumerator OffMeshMove(IAIControl ai, MAnimal animal, Vector3 StartPoint, Vector3 EndPoint)
        {
            if (AlignToLink && start && end)
            {
                Debbuging($"Start alignment with [{animal.name}]");
                var NearAlign = animal.transform.NearestTransform(start, end); //Find the closest alignpoint
                yield return MTools.AlignTransform_Rotation(animal.transform, NearAlign.rotation, AlignTime);
                Debbuging($"Finish alignment with [{animal.name}]");
            }

            MDebug.DrawRay(animal.Position, StartPoint.DirectionTo(EndPoint), Color.red, 2);


            StartReaction?.React(animal);
            Debbuging($"Start Offmesh Coroutine");

            ai.InOffMeshLink = true;
            ai.AIDirection = StartPoint.DirectionTo(EndPoint);

            RemainingDistance = float.MaxValue;

            // var axis = animal.transform.NearestPoint(start.position, end.position) == start.position ? StartAxis : EndAxis;

            while (RemainingDistance >= StoppingDistance && ai.InOffMeshLink)
            {
                var AIDirection = StartPoint.DirectionTo(EndPoint);

                MDebug.Draw_Arrow(animal.Position, AIDirection, Color.green);

                MDebug.DrawWireSphere(EndPoint, DebugColor, StoppingDistance);
                MDebug.DrawWireSphere(EndPoint, Color.cyan, SlowingDistance);

                if (!UseInputAxis) //If its using Direction vector to move
                {
                    ai.AIDirection = (AIDirection);
                    animal.Move(AIDirection * SlowMultiplier);
                }
                else //If its using Input Axis to to move (Meaning go All Horizontal, or Forward movement)
                {
                    AIDirection = transform.InverseTransformDirection(AIDirection); //Convert to UP Down like Climb
                    AIDirection.z = AIDirection.y;
                    AIDirection.y = 0;
                    animal.SetInputAxis(AIDirection * SlowMultiplier);
                    animal.UsingMoveWithDirection = false;
                }

                RemainingDistance = Vector3.Distance(animal.transform.position, EndPoint);
                yield return null;
            }

            if (ai.InOffMeshLink)
                EndReaction?.React(animal); //Execute the End Reaction only if the Animal has not interrupted the Offmesh Link

            Debbuging($"End Offmesh Coroutine");
            ai.CompleteOffMeshLink();
        }


        public float SlowMultiplier
        {
            get
            {
                var result = 1f;
                if (SlowingDistance > StoppingDistance && RemainingDistance < SlowingDistance)
                    result = Mathf.Max(RemainingDistance / SlowingDistance, SlowingLimit);
                return result;
            }
        }

        public float RemainingDistance { get; private set; }

        private void Debbuging(string valu)
        {
            if (debug) Debug.Log($"<B>OffMeshLink - [{name}]</B> -> {valu}", this);
        }



#if UNITY_EDITOR



        private void OnDrawGizmos()
        {
            Gizmos.color = DebugColor;
            Handles.color = DebugColor;

            var AxisSize = transform.lossyScale.y;

            if (start)
            {
                Gizmos.DrawSphere(start.position, 0.2f * AxisSize);
                Handles.ArrowHandleCap(0, start.position, start.rotation, AxisSize, EventType.Repaint);
            }
            if (end)
            {
                Gizmos.DrawSphere(end.position, 0.2f * AxisSize);
                Handles.ArrowHandleCap(0, end.position, end.rotation, AxisSize, EventType.Repaint);

            }
            if (start && end)
                Handles.DrawDottedLine(start.position, end.position, 5);
        }

        private void OnDrawGizmosSelected()
        {
            if (start)
            {
                Gizmos.color = DebugColor;
                Gizmos.DrawWireSphere(start.position, 0.2f * transform.lossyScale.y);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(start.position, StoppingDistance);
                if (StoppingDistance < SlowingDistance)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(start.position, SlowingDistance);
                }
            }
            if (end)
            {
                Gizmos.color = DebugColor;
                Gizmos.DrawWireSphere(end.position, 0.2f * transform.lossyScale.y);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(end.position, StoppingDistance);
                if (StoppingDistance < SlowingDistance)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(end.position, SlowingDistance);
                }
            }
        }


        //        private void Reset()
        //        {
        //#if UNITY_6000_0_OR_NEWER
        //            meshLink = GetComponent<NavMeshLink>();
        //#else
        //            meshLink = GetComponent<UnityEngine.AI.OffMeshLink>();
        //#endif
        //            if (meshLink)
        //            {
        //                start = meshLink.startTransform;
        //                end = meshLink.endTransform;
        //                BiDirectional =

        //#if UNITY_6000_0_OR_NEWER
        //                meshLink.bidirectional;
        //#else
        //                meshLink.biDirectional;
        //#endif
        //            }
        //            else
        //            {
        //                start = transform;
        //            }
        //        }



#endif


#if UNITY_EDITOR
        [CustomEditor(typeof(MAIAnimalLink))]
        public class MAILinkEditor : Editor
        {
            SerializedProperty StartReaction, EndReaction, Start, End, DebugColor, UseInputAxis, EndAxis, StartAxis, AlignToLink, AlignTime, debug,
                StoppingDistance, SlowingLimit, SlowingDistance, BiDirectional;

            MAIAnimalLink M;

            private void OnEnable()
            {
                M = (MAIAnimalLink)target;
                StartReaction = serializedObject.FindProperty("StartReaction");
                debug = serializedObject.FindProperty("debug");
                EndReaction = serializedObject.FindProperty("EndReaction");
                Start = serializedObject.FindProperty("start");
                End = serializedObject.FindProperty("end");
                StoppingDistance = serializedObject.FindProperty("StoppingDistance");
                SlowingLimit = serializedObject.FindProperty("SlowingLimit");
                SlowingDistance = serializedObject.FindProperty("SlowingDistance");
                DebugColor = serializedObject.FindProperty("DebugColor");
                UseInputAxis = serializedObject.FindProperty("UseInputAxis");
                StartAxis = serializedObject.FindProperty("StartAxis");
                EndAxis = serializedObject.FindProperty("EndAxis");
                BiDirectional = serializedObject.FindProperty("BiDirectional");
                AlignToLink = serializedObject.FindProperty("AlignToLink");
                AlignTime = serializedObject.FindProperty("AlignTime");
            }
            public override void OnInspectorGUI()
            {
                //base.OnInspectorGUI();
                serializedObject.Update();

                MalbersEditor.DrawDescription("Uses Animal reactions to move the Agent when its at a OffMeshLinks");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StoppingDistance);
                EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.Width(50));
                MalbersEditor.DrawDebugIcon(debug);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(SlowingDistance);
                EditorGUILayout.PropertyField(SlowingLimit);
                EditorGUILayout.PropertyField(BiDirectional);
                EditorGUILayout.PropertyField(UseInputAxis);

                //if (UseInputAxis.boolValue)
                //{
                //    EditorGUILayout.PropertyField(StartAxis);
                //    EditorGUILayout.PropertyField(EndAxis);
                //}


                EditorGUILayout.PropertyField(AlignToLink);

                if (AlignToLink.boolValue)
                {
                    EditorGUILayout.PropertyField(AlignTime);
                    EditorGUILayout.PropertyField(Start);
                    EditorGUILayout.PropertyField(End);
                }

                MalbersEditor.DrawSplitter();
                EditorGUILayout.PropertyField(StartReaction);
                //EditorGUILayout.Space();
                MalbersEditor.DrawSplitter();
                EditorGUILayout.PropertyField(EndReaction);
                serializedObject.ApplyModifiedProperties();
            }

            void OnSceneGUI()
            {
                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    if (M.start && M.start != M.transform)
                    {
                        var start = M.start.position;
                        start = Handles.PositionHandle(start, M.transform.rotation);

                        if (cc.changed)
                        {
                            Undo.RecordObject(M.start, "Move Start AI Link");
                            M.start.position = start;
                        }
                    }
                }

                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    if (M.end && M.end != M.transform)
                    {
                        var end = M.end.position;
                        end = Handles.PositionHandle(end, M.transform.rotation);

                        if (cc.changed)
                        {
                            Undo.RecordObject(M.end, "Move End AI Link");
                            M.end.position = end;
                        }
                    }
                }
            }
        }
#endif
    }
}