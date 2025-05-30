using UnityEngine;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorPicker))]
    public class ColorPickerInspector : Editor
    {
        ColorPicker picker;

        private void OnEnable()
        {
            picker = target as ColorPicker;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Color resultColor = EditorGUILayout.ColorField("Color", picker.CurrentColor);
            if (EditorGUI.EndChangeCheck())
            {
                picker.CurrentColor = resultColor;
            }

            EditorGUI.BeginChangeCheck();
            byte alpha = (byte)EditorGUILayout.IntSlider("Aplha", picker.Alpha, 0, 255);
            if (EditorGUI.EndChangeCheck())
            {
                picker.Alpha = alpha;
            }

            EditorGUILayout.Space();

            RGBColor rgb = picker.rgb;
            EditorGUI.BeginChangeCheck();
            rgb.BR = (byte)EditorGUILayout.IntSlider("R", rgb.BR, 0, 255);
            rgb.BG = (byte)EditorGUILayout.IntSlider("G", rgb.BG, 0, 255);
            rgb.BB = (byte)EditorGUILayout.IntSlider("B", rgb.BB, 0, 255);
            if (EditorGUI.EndChangeCheck())
            {
                picker.rgb = rgb;
            }

            EditorGUILayout.Space();

            HSVColor hsv = picker.hsv;
            EditorGUI.BeginChangeCheck();
            hsv.H = EditorGUILayout.Slider("H", (float)hsv.H, 0, 360);
            hsv.S = EditorGUILayout.Slider("S", (float)hsv.S, 0, 1);
            hsv.V = EditorGUILayout.Slider("V", (float)hsv.V, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                picker.hsv = hsv;
            }

            EditorGUILayout.Space();

            HSLColor hsl = picker.hsl;
            EditorGUI.BeginChangeCheck();
            hsl.H = EditorGUILayout.Slider("H", (float)hsl.H, 0, 360);
            hsl.S = EditorGUILayout.Slider("S", (float)hsl.S, 0, 1);
            hsl.L = EditorGUILayout.Slider("L", (float)hsl.L, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                picker.hsl = hsl;
            }

            EditorGUILayout.Space();

            serializedObject.Update();
            SerializedProperty colorChanged = serializedObject.FindProperty("OnColorChanged");
            EditorGUILayout.PropertyField(colorChanged);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
