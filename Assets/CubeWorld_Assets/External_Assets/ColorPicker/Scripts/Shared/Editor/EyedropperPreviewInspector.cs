using UnityEngine;
using UnityEditor;
using System.Collections;
using AdvancedColorPicker;

namespace ColorPickerEditor
{
    [CustomEditor(typeof(ColorEyedropperPreview))]
    public class EyedropperPreviewInspector : Editor
    {
        ColorEyedropperPreview preview;

        private void OnEnable()
        {
            preview = target as ColorEyedropperPreview;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty backgroundColor = serializedObject.FindProperty("m_Color");
            SerializedProperty selectionBoxColor = serializedObject.FindProperty("selectionBoxColor");
            SerializedProperty activated = serializedObject.FindProperty("activated");

            EditorGUI.BeginChangeCheck();
            ColorEyedropperPreview.EyedropperPreviewType type = (ColorEyedropperPreview.EyedropperPreviewType)EditorGUILayout.EnumPopup("Type", preview.Type);
            if (EditorGUI.EndChangeCheck())
            {
                preview.Type = type;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            GUI.enabled = preview.Type == ColorEyedropperPreview.EyedropperPreviewType.PixelSize;
            float pixelSize = EditorGUILayout.FloatField(new GUIContent("Pixel size", "The size of one pixel in pixels/zoom multiplier"), preview.PixelSize);
            if (EditorGUI.EndChangeCheck())
            {
                preview.PixelSize = pixelSize;
            }

            EditorGUI.BeginChangeCheck();
            GUI.enabled = preview.Type == ColorEyedropperPreview.EyedropperPreviewType.PixelAmountHorizontal;
            float horizontalPixels = EditorGUILayout.FloatField(new GUIContent("Horizontal pixels", "The amount of pixels to the left and right of the center.\nTotal amount of horizontal pixels is: 1 + (2 * value)"), preview.HorizontalPixels);
            if (EditorGUI.EndChangeCheck())
            {
                preview.HorizontalPixels = horizontalPixels;
            }

            EditorGUI.BeginChangeCheck();
            GUI.enabled = preview.Type == ColorEyedropperPreview.EyedropperPreviewType.PixelAmountVertical;
            float verticalPixels = EditorGUILayout.FloatField(new GUIContent("Vertical pixels", "The amount of pixels above and under the center.\nTotal amount of vertical pixels is: 1 + (2 * value)"), preview.VerticalPixels);
            if (EditorGUI.EndChangeCheck())
            {
                preview.VerticalPixels = verticalPixels;
            }


            GUI.enabled = true;
            int xSize = (Mathf.CeilToInt(horizontalPixels) * 2) + 1;
            int ySize = (Mathf.CeilToInt(verticalPixels) * 2) + 1;
            EditorGUILayout.LabelField(new GUIContent("Visible pixels", "The estimated amount of pixels visible. For performance reasons, try to keep this value as low as possible"), new GUIContent((xSize * ySize).ToString()));
            EditorGUILayout.LabelField(new GUIContent("Vertex count", "The estimated vertex count with these settings. This value is 8 + (VisiblePixels * 4) For the best performance, try to keep this value low.\nNOTE: Must stay below 65000!"), new GUIContent(preview.ExpectedVertices.ToString()));
            if (preview.ExpectedVertices >= 65000)
            {
                string errorMessage = "Too many vertices! The current values would result into 65000 or more vertices.";
                switch (preview.Type)
                {
                    case ColorEyedropperPreview.EyedropperPreviewType.PixelSize:
                        errorMessage += " Please increase the pixelsize.";
                        break;
                    case ColorEyedropperPreview.EyedropperPreviewType.PixelAmountHorizontal:
                        errorMessage += " Please decrease the number of pixels horizontally.";
                        break;
                    case ColorEyedropperPreview.EyedropperPreviewType.PixelAmountVertical:
                        errorMessage += " Please decrease the number of pixels vertically.";
                        break;
                    default:
                        break;
                }
                errorMessage += " For this Component to work and this error message to go away, the vertex count needs to be less then 65000. Resizing the component can also impact the amount of vertices used!";

                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

            EditorGUILayout.Space();

            preview.BorderSize = EditorGUILayout.FloatField(new GUIContent("Border size", "The size of the border in pixels"), preview.BorderSize);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(backgroundColor, new GUIContent("Background color", "The background/border color"));
            EditorGUILayout.PropertyField(selectionBoxColor);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(activated);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
