using UnityEngine;
using UnityEditor;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorInput_TMP)), CanEditMultipleObjects]
    public class ColorInput_TMPInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty type = serializedObject.FindProperty("type");
            SerializedProperty minValue = serializedObject.FindProperty("minValue");
            SerializedProperty maxValue = serializedObject.FindProperty("maxValue");
            SerializedProperty formatter = serializedObject.FindProperty("formatter");

            type.intValue = (int)(ColorValueType)EditorGUILayout.EnumPopup("Type", (ColorValueType)type.intValue);
            EditorGUILayout.PropertyField(minValue);
            EditorGUILayout.PropertyField(maxValue);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = formatter.hasMultipleDifferentValues;
            string newFormat = EditorGUILayout.TextField(new GUIContent(formatter.displayName, "How the number is formatted, see the readme for more info on valid formatters"), formatter.stringValue);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck() && 0f.IsFormatValid(newFormat))
            {
                formatter.stringValue = newFormat;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}