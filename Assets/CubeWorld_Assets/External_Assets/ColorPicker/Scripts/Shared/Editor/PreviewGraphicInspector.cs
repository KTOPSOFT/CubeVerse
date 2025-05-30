using UnityEngine;
using UnityEditor;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(PreviewGraphic)), CanEditMultipleObjects]
    public class PreviewGraphicInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty alphaIsFixed = serializedObject.FindProperty("alphaIsFixed");
            SerializedProperty fixedAlpha = serializedObject.FindProperty("fixedAlpha");

            EditorGUILayout.PropertyField(alphaIsFixed);
            GUI.enabled = alphaIsFixed.boolValue;
            EditorGUILayout.PropertyField(fixedAlpha);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}