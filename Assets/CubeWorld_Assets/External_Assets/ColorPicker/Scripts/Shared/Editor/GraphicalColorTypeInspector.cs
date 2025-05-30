using UnityEngine;
using UnityEditor;
using System;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(GraphicalColorTypeComponent), true), CanEditMultipleObjects]
    public class GraphicalColorTypeInspector : GraphicalColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty valueType1 = serializedObject.FindProperty("valueType1");
            SerializedProperty valueType2 = serializedObject.FindProperty("valueType2");
            SerializedProperty valueType3 = serializedObject.FindProperty("valueType3");
            SerializedProperty fixedValue3 = serializedObject.FindProperty("fixedValue3");
            ColorType type1 = (ColorType)valueType1.intValue;
            ColorType type2 = (ColorType)valueType2.intValue;
            ColorType type3 = (ColorType)valueType3.intValue;

            // Draw type 1
            EditorGUI.BeginChangeCheck();
            int result1 = EditorGUILayout.Popup("Type 1", valueType1.enumValueIndex, valueType1.enumDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                valueType1.enumValueIndex = result1;
                GraphicalColorTypeComponent.CalculateNewValue((ColorType)valueType1.intValue, ref type1, ref type2, ref type3);
                valueType1.intValue = (int)type1;
                valueType2.intValue = (int)type2;
                valueType3.intValue = (int)type3;

                fixedValue3.floatValue = GetDefaultFixedValue(type3);
            }

            // Draw type 2
            EditorGUI.BeginChangeCheck();
            int result2 = EditorGUILayout.Popup("Type 2", valueType2.enumValueIndex, valueType2.enumDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                valueType2.enumValueIndex = result2;
                GraphicalColorTypeComponent.CalculateNewValue((ColorType)valueType2.intValue, ref type2, ref type1, ref type3);
                valueType1.intValue = (int)type1;
                valueType2.intValue = (int)type2;
                valueType3.intValue = (int)type3;

                fixedValue3.floatValue = GetDefaultFixedValue(type3);
            }

            // Draw type 3
            DrawFixed(fixedValue3, type1, type3);

            serializedObject.ApplyModifiedProperties();
        }

        private static float GetDefaultFixedValue(ColorType type)
        {
            switch (type)
            {
                case ColorType.RGB_R:
                case ColorType.RGB_G:
                case ColorType.RGB_B:
                case ColorType.HSV_H:
                case ColorType.HSL_H:
                    return -1f;
                case ColorType.HSV_V:
                case ColorType.HSV_S:
                case ColorType.HSL_S:
                    return 1f;
                case ColorType.HSL_L:
                    return 0.5f;
                default:
                    throw new ArgumentException(type + " is not a valid value for this component");
            }
        }

        private static void DrawFixed(SerializedProperty fixedProperty, ColorType type1, ColorType fixedType)
        {
            Rect position = EditorGUILayout.GetControlRect();
            Rect left = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect right = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

            EditorGUI.BeginChangeCheck();
            bool toggled = EditorGUI.ToggleLeft(left, "Fixed " + fixedType.ToString(), fixedProperty.floatValue >= 0);
            float defaultFixedValue = GetDefaultFixedValue(fixedType);
            if (EditorGUI.EndChangeCheck())
            {
                if (toggled)
                    fixedProperty.floatValue = Mathf.Max(0, defaultFixedValue);
                else
                    fixedProperty.floatValue = -1;
            }

			if (toggled)
			{
				fixedProperty.floatValue = EditorGUI.Slider(right, fixedProperty.floatValue, 0, 1);
			}
			else
			{
				bool guiEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUI.Slider(right, defaultFixedValue < 0 ? 0 : defaultFixedValue, 0, 1);
				GUI.enabled = guiEnabled;
			}
        }
    }
}
