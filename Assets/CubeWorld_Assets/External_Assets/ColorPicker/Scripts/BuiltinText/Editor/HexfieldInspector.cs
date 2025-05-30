using UnityEngine;
using UnityEditor;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorHexField))]
    public class HexfieldInspector : ColorComponentInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SerializedProperty displayAlpha = serializedObject.FindProperty("displayAlpha");
            SerializedProperty displayHashtag = serializedObject.FindProperty("displayHashtag");
            SerializedProperty acceptedInput = serializedObject.FindProperty("acceptedInput");
            SerializedProperty editableChannels = serializedObject.FindProperty("editableChannels");

            EditorGUILayout.PropertyField(displayAlpha);
            EditorGUILayout.PropertyField(displayHashtag);

            acceptedInput.intValue = (int)(HexfieldType)EditorGUILayout.EnumFlagsField(new GUIContent(acceptedInput.displayName, "Enables which type of hexes are allowed/parsed, if the user enters a type of hex that is not allowed, the entire hex is ignored"), (HexfieldType)acceptedInput.intValue);

            EditorGUILayout.PropertyField(editableChannels);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
