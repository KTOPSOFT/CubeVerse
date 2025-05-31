using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.PathCreation
{
    [AddComponentMenu("Malbers/Animal Controller/Path Link (Straight)")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/path-constraint/path-link-straight")]
    public class PathLink_Straight : MonoBehaviour, IPath
    {
        public Vector3 EndPoint = Vector3.forward;
        [Tooltip("Rotation Along the Spline")]
        public float Roll;

        public bool ShowTangents;
        public float TangentLength = 0.2f;
        [Min(1)] public int TangentCount = 20;
        public Color TangentColor = Color.yellow;

        public Vector3 StartPath => transform.position;

        public Vector3 EndPath => transform.TransformPoint(EndPoint);

        public bool IsClosed => false;

        public Bounds bounds => CalculateBounds();

        public float GetClosestTimeOnPath(Vector3 position)
        {
            return position.ClosestTimeOnSegment(StartPath, EndPath);
        }



        public Quaternion GetPathRotation(float NormalizedTime)
        {
            //HERE IS THE CALCULATION OF THE PATH
            return Quaternion.Euler(0, 0, Roll * NormalizedTime) * transform.rotation;
        }

        public Vector3 GetPointAtTime(float NormalizedTime)
        {
            return StartPath + ((EndPath - StartPath) * NormalizedTime);
        }


        internal Bounds CalculateBounds()
        {
            Bounds bounds = new Bounds();
            bounds.Encapsulate(EndPoint);
            return bounds;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(StartPath, EndPath);

            Gizmos.DrawSphere(StartPath, 0.02f * transform.localScale.y);
            Gizmos.DrawSphere(EndPath, 0.02f * transform.localScale.y);

            if (ShowTangents)
            {

                Gizmos.color = TangentColor;

                for (int i = 0; i <= TangentCount; i++)
                {
                    float segment = (float)i / (float)TangentCount;

                    var RollVector = Quaternion.Euler(0, 0, Roll * segment) * (transform.up * TangentLength);

                    Gizmos.DrawRay(GetPointAtTime(segment), RollVector);
                }
            }
        }
    }

    #region INSPECTOR 
#if UNITY_EDITOR

    [CustomEditor(typeof(PathLink_Straight))]
    public class PathLink_StraightEd : Editor
    {
        PathLink_Straight M;

        SerializedProperty EndPoint;
        SerializedProperty Roll, TangentLength, ShowTangents, TangentCount, TangentColor;

        private void OnEnable()
        {
            M = (PathLink_Straight)target;
            Roll = serializedObject.FindProperty("Roll");
            EndPoint = serializedObject.FindProperty("EndPoint");
            ShowTangents = serializedObject.FindProperty("ShowTangents");
            TangentLength = serializedObject.FindProperty("TangentLength");
            TangentCount = serializedObject.FindProperty("TangentCount");
            TangentColor = serializedObject.FindProperty("TangentColor");

        }

        private bool EditPivot;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(EndPoint);
                    EditPivot = GUILayout.Toggle(EditPivot, MalbersEditor.Icon_Point, EditorStyles.miniButton, GUILayout.Width(20));
                }
                EditorGUILayout.PropertyField(Roll);
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ShowTangents.boolValue = MalbersEditor.Foldout(ShowTangents.boolValue, "Show Tangents");

                if (ShowTangents.boolValue)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(TangentLength);
                        EditorGUIUtility.labelWidth = 40;
                        EditorGUILayout.PropertyField(TangentCount, new GUIContent("Count"), GUILayout.Width(80));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.PropertyField(TangentColor, GUIContent.none, GUILayout.Width(50));
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (EditPivot)
            {
                Transform t = M.transform;

                using (var cc = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 piv = t.TransformPoint(M.EndPoint);

                    Vector3 NewPivPosition = Handles.PositionHandle(piv, Quaternion.identity);

                    if (cc.changed)
                    {
                        Undo.RecordObject(M, "Pivots");

                        // var NewPoint = t.InverseTransformPoint(NewPivPosition);

                        t.rotation = Quaternion.LookRotation(NewPivPosition - t.position, Vector3.up);

                        M.EndPoint = t.InverseTransformPoint(NewPivPosition).Round(5);

                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }
    }
#endif
    #endregion
}
