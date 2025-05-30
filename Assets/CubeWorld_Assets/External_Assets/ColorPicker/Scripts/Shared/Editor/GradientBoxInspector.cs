using UnityEngine;
using UnityEditor;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(GradientBox), true), CanEditMultipleObjects]
    public class GradientBoxInspector : GraphicalColorTypeInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            bool hasError = false;
            for (int i = 0; i < targets.Length; i++)
            {
                Slider2D slider = (target as GradientBox).GetComponent<Slider2D>();
                if (slider.minValue.x < 0 || slider.minValue.x > 1 || slider.maxValue.x < 0 || slider.maxValue.x > 1 ||
                    slider.minValue.y < 0 || slider.minValue.y > 1 || slider.maxValue.y < 0 || slider.maxValue.y > 1)
                {
                    hasError = true;
                    break;
                }
            }
            if (hasError)
            {
                if (serializedObject.isEditingMultipleObjects)
                    EditorGUILayout.HelpBox("One or more of the selected GradientBox has an invalid min or max value, please make sure all the min values and max values range between 0 and 1!", MessageType.Error);
                else
                    EditorGUILayout.HelpBox("The Slider2D's min and max value need to range between 0 and 1!", MessageType.Error);
            }
        }
    }
}
