using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    /// <summary> Quick Class to Change the values of a Animator Parameter  </summary>
    [AddComponentMenu("Malbers/Utilities/Animator/Set Animator Parameter")]

    public class SetAnimatorParameter : MonoBehaviour
    {
        public Animator animator;
        public List<MAnimatorParameter> parameters = new();

        /// <summary> Set all the Parameters in the Animator  </summary>
        public void Set()
        {
            foreach (var param in parameters)
                param.Set(animator);
        }

        public void Set(Animator anim)
        {
            foreach (var param in parameters)
                param.Set(anim);
        }

        public void Set(Component comp) => Set(comp.FindComponent<Animator>());

        public void Set(GameObject comp) => Set(comp.FindComponent<Animator>());

    }

    [System.Serializable]
    public struct MAnimatorParameter
    {
        [Tooltip("Name of the Parameter in the Animator")]
        public string param;
        [Tooltip("Type of the Animator Parameter")]
        public AnimatorControllerParameterType type;
        [Tooltip("Value to set on the Parameter. Float and Int parameters are represented by this variable. Bool is calculated if this value is not equal to 0")]
        public float Value;

        public int ParamHash { get; set; }

        public void GetHashValue() => ParamHash = Animator.StringToHash(param);

        public void Set(Animator anim)
        {
            if (ParamHash == 0) GetHashValue(); //Find the hash first
            if (anim == null) return;

            if (ParamHash == 0) return; //If the hash is not found return

            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    anim.SetFloat(ParamHash, Value);
                    break;
                case AnimatorControllerParameterType.Int:
                    anim.SetInteger(ParamHash, (int)Value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    anim.SetBool(ParamHash, Value != 0);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    anim.SetTrigger(ParamHash);
                    break;
                default:
                    break;
            }
        }

        public void Set(Component comp) => Set(comp.FindComponent<Animator>());
        public void Set(GameObject go) => Set(go.FindComponent<Animator>());
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(MAnimatorParameter))]
    public class MAnimatorParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                var height = EditorGUIUtility.singleLineHeight;
                var param = property.FindPropertyRelative("param");
                var type = property.FindPropertyRelative("type");
                var Value = property.FindPropertyRelative("Value");

                var rectName = new Rect(position) { height = height };
                var typeRect = new Rect(rectName);
                var valueRect = new Rect(rectName);

                var widthName = rectName.width * 0.50f;
                var widthType = rectName.width * 0.25f;
                var widthVal = widthType;

                rectName.width = widthName;
                typeRect.width = widthType - 5;
                valueRect.width = widthVal - 5;

                typeRect.x += widthName + 5;
                valueRect.x += widthName + widthType + 5;



                EditorGUIUtility.labelWidth = 40;
                EditorGUI.PropertyField(rectName, param);
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.PropertyField(typeRect, type, GUIContent.none);


                AnimatorControllerParameterType t = (AnimatorControllerParameterType)type.intValue;
                if (t == AnimatorControllerParameterType.Trigger)
                    typeRect.width = (widthType * 2) - 5;

                switch (t)
                {
                    case AnimatorControllerParameterType.Float:
                        EditorGUI.PropertyField(valueRect, Value, GUIContent.none);
                        break;
                    case AnimatorControllerParameterType.Int:
                        Value.floatValue = EditorGUI.IntField(valueRect, GUIContent.none, (int)Value.floatValue);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        bool val = Value.floatValue != 0f;
                        val = EditorGUI.ToggleLeft(valueRect, new GUIContent(val ? "True" : "False"), val);
                        Value.floatValue = val ? 1 : 0;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        Value.floatValue = 0;
                        break;
                    default:
                        break;
                }
                EditorGUIUtility.labelWidth = 0;
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}
