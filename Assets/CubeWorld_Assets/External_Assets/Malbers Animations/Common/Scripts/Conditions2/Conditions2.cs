using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    public struct Conditions2
    {
        [SerializeReference] public ConditionCore[] conditions;

        /// <summary>  Conditions can be used the list has some conditions </summary>
        public readonly bool Valid => conditions != null && conditions.Length > 0;

        public readonly bool Evaluate(Object target)
        {
            if (!Valid) return true; //by default return true

            if (conditions[0] == null)
            {
                Debug.LogError($"[Null] Condition not Allowed. Please Check your conditions.", target);
                return false;
            }

            bool result = conditions[0].Evaluate(target); //Get the first one

            for (int i = 1; i < conditions.Length; i++) //start from the 2nd one
            {
                try
                {
                    bool nextResult = conditions[i].Evaluate(target);
                    result = conditions[i].OrAnd ? (result || nextResult) : (result && nextResult);
                }
                catch
                {
                    Debug.LogError($"[Null] Condition [{i}]. Please Check your conditions.", target);
                }
            }
            return result;
        }

        public readonly void Gizmos(Component comp)
        {
            if (conditions == null || conditions.Length == 0) return;

            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].DebugCondition) conditions[i].DrawGizmos(comp);
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Conditions2))]
    public class Conditions2Drawer : PropertyDrawer
    {
        private SerializedProperty conditions;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            conditions ??= property.FindPropertyRelative("conditions");
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, conditions, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            conditions ??= property.FindPropertyRelative("conditions");
            return EditorGUI.GetPropertyHeight(conditions, label);
        }
    }
#endif
}