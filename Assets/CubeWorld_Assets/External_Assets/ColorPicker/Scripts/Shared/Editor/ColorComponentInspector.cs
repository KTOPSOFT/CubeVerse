using UnityEngine;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorComponent), true), CanEditMultipleObjects]
    public class ColorComponentInspector : Editor
    {
        ColorComponent component
        {
            get
            {
                return target as ColorComponent;
            }
        }

        public override void OnInspectorGUI()
        {
            ColorPicker p = component.Picker;

            for (int i = 0; i < targets.Length; i++)
            {
                if (((ColorComponent)targets[i]).Picker != p)
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }


            EditorGUI.BeginChangeCheck();
            ColorPicker picker = (ColorPicker)EditorGUILayout.ObjectField("Picker", component.Picker, typeof(ColorPicker), true);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    ((ColorComponent)targets[i]).Picker = picker;
                }
            }
            EditorGUI.showMixedValue = false;

            if (picker == null)
            {
                EditorGUILayout.HelpBox("ColorPicker not set! In order to display the color of a ColorPicker, you need to specify the ColorPicker in the field above", MessageType.Warning);
            }
        }
    }
}
