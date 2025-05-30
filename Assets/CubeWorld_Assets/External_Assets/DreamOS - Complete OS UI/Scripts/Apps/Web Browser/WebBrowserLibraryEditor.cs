#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WebBrowserLibrary))]
    public class WebBrowserLibraryEditor : Editor
    {
        private GUISkin customSkin;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            // Content Header
            var appPages = serializedObject.FindProperty("appPages");

            var homePage = serializedObject.FindProperty("homePage");
            var notFoundPage = serializedObject.FindProperty("notFoundPage");
            var noConnectionPage = serializedObject.FindProperty("noConnectionPage");
            var webPages = serializedObject.FindProperty("webPages");
            var dlFiles = serializedObject.FindProperty("dlFiles");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(homePage, new GUIContent("Home Page"), true);
            EditorGUI.indentLevel = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(notFoundPage, new GUIContent("Not Found Page"), true);
            EditorGUI.indentLevel = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(noConnectionPage, new GUIContent("No Connection Page"), true);
            EditorGUI.indentLevel = 0;
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(webPages, new GUIContent("Web Pages"), true);

            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+  Add a new page", customSkin.button))
                webPages.arraySize += 1;

            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(dlFiles, new GUIContent("Downloadble File List"), true);

            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+  Add a new file", customSkin.button))
                dlFiles.arraySize += 1;

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif