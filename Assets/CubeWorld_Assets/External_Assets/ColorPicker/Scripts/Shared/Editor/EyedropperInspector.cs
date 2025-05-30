using UnityEngine;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorEyedropper))]
    public class EyedropperInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty changeColorInstant = serializedObject.FindProperty("changesColorInstantly");
            SerializedProperty onActivated = serializedObject.FindProperty("OnActivated");
            SerializedProperty onDeactivated = serializedObject.FindProperty("OnDeactivated");

            EditorGUILayout.PropertyField(changeColorInstant);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(onActivated);
            EditorGUILayout.PropertyField(onDeactivated);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
