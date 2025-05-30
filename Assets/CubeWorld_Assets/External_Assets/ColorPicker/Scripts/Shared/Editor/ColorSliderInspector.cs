using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;
using System;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorSlider)), CanEditMultipleObjects]
    public class ColorSliderInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty type = serializedObject.FindProperty("type");

            type.intValue = (int)(ColorValueType)EditorGUILayout.EnumPopup("Type", (ColorValueType)type.intValue);

            serializedObject.ApplyModifiedProperties();

            bool hasError = false;
            for (int i = 0; i < targets.Length; i++)
            {
                Slider slider = (target as ColorSlider).GetComponent<Slider>();
                if (slider.minValue < 0 || slider.minValue > 1 || slider.maxValue < 0 || slider.maxValue > 1)
                {
                    hasError = true;
                    break;
                }
            }
            if (hasError)
            {
                if (serializedObject.isEditingMultipleObjects)
                    EditorGUILayout.HelpBox("One or more of the selected sliders has an invalid min or max value, please make sure all the min values and max values range between 0 and 1!", MessageType.Error);
                else
                    EditorGUILayout.HelpBox("The slider's min and max value need to range between 0 and 1!", MessageType.Error);
            }
        }
    }
}
