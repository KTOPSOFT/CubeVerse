using UnityEngine;
using UnityEditor;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(PreviewRenderer)), CanEditMultipleObjects]
    public class PreviewRendererInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty material = serializedObject.FindProperty("_material");

            EditorGUILayout.PropertyField(material);

            serializedObject.ApplyModifiedProperties();
        }
    }
}