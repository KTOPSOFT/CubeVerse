using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Gizmos/Axis Visualizer")]

    public class AxisVisualizer : MonoBehaviour
    {
#if UNITY_EDITOR
        [Min(0)] public float size = 0.5f;
        [Range(0, 1)]
        public float Opacity = 1f;

        public bool x = true;
        public bool y = true;
        public bool z = true;

        private void OnEnable() { }

        void OnDrawGizmos()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) return;

            if (!enabled) return;

            UnityEditor.Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            if (z)
            {
                UnityEditor.Handles.color = new Color(0, 0, 1, Opacity);  //Blue
                UnityEditor.Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.identity, size, EventType.Repaint);
            }

            if (x)
            {
                UnityEditor.Handles.color = new Color(1, 0, 0, Opacity);  //red
                UnityEditor.Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.Euler(0, 90, 0), size, EventType.Repaint);
            }

            if (y)
            {
                UnityEditor.Handles.color = new Color(0, 1, 0, Opacity);  //Green
                UnityEditor.Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.Euler(-90, 0, 0), size, EventType.Repaint);
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1, 1, 0, Opacity);
            Gizmos.DrawWireCube(Vector3.zero, 0.1f * size * Vector3.one);
            Gizmos.color = new Color(1, 1, 0, Opacity * 0.5f);
            Gizmos.DrawCube(Vector3.zero, 0.1f * size * Vector3.one);


        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AxisVisualizer)), UnityEditor.CanEditMultipleObjects]
    public class AxisEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Opacity, AxisSize, x, y, z;

        private void OnEnable()
        {
            AxisSize = serializedObject.FindProperty("size");
            Opacity = serializedObject.FindProperty("Opacity");

            x = serializedObject.FindProperty("x");
            y = serializedObject.FindProperty("y");
            z = serializedObject.FindProperty("z");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                UnityEditor.EditorGUIUtility.labelWidth = 45;
                UnityEditor.EditorGUILayout.PropertyField(AxisSize);
                UnityEditor.EditorGUILayout.PropertyField(Opacity);

                UnityEditor.EditorGUIUtility.labelWidth = 10;
                UnityEditor.EditorGUILayout.PropertyField(x, GUILayout.Width(26));
                UnityEditor.EditorGUILayout.PropertyField(y, GUILayout.Width(26));
                UnityEditor.EditorGUILayout.PropertyField(z, GUILayout.Width(26));
                UnityEditor.EditorGUIUtility.labelWidth = 0;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}