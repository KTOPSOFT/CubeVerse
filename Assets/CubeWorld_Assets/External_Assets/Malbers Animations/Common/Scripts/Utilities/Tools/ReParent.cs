using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    /// <summary> Simple script to reparent a bone on enable </summary>
    [AddComponentMenu("Malbers/Utilities/Tools/Parent")]
    public class ReParent : MonoBehaviour
    {
        [Tooltip("Reparent this gameObject to a new Transform. Use this to have more organized GameObjects on the hierarchy")]
        [ContextMenuItem("Use Bone Name", nameof(SetUseName))]
        public Transform newParent;
        [Tooltip("Reparent this gameObject to a new Transform. Use this to have more organized GameObjects on the hierarchy")]
        [ContextMenuItem("Use Transform", nameof(SetUseTransform))]
        public string NewParentName;

        public bool ResetLocal = false;

        [SerializeField, HideInInspector] private bool UseName = false;
        private void OnEnable()
        {
            if (UseName)
            {
                newParent = transform.FindObjectCore().FindGrandChild(NewParentName);
                transform.SetParent(newParent, true);
            }
            else
            {
                if (newParent == null)
                    transform.parent = null;
                else

                    transform.SetParent(newParent, true);
            }

            if (ResetLocal) transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        private void Reset()
        {
            newParent = transform.parent;

        }

        void SetUseName() => UseName = true;
        void SetUseTransform() => UseName = false;


    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ReParent)), CanEditMultipleObjects]
    public class ReParentEditor : Editor
    {
        SerializedProperty newParent, ResetLocal, NewParentName, UseName;
        private void OnEnable()
        {
            newParent = serializedObject.FindProperty("newParent");
            ResetLocal = serializedObject.FindProperty("ResetLocal");
            NewParentName = serializedObject.FindProperty("NewParentName");
            UseName = serializedObject.FindProperty("UseName");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new GUILayout.HorizontalScope())
            {
                if (UseName.boolValue)
                {
                    EditorGUILayout.PropertyField(NewParentName);
                }
                else
                {
                    EditorGUILayout.PropertyField(newParent);
                }

                ResetLocal.boolValue = GUILayout.Toggle(ResetLocal.boolValue, new GUIContent("R", "Reset Local Position and Rotation after parenting"),
                    EditorStyles.miniButton, GUILayout.Width(23));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}