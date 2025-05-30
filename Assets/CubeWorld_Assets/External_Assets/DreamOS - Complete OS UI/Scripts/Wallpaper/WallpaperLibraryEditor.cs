//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WallpaperLibrary))]
    public class WallpaperLibraryEditor : Editor
    {
        private GUISkin customSkin;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            // Content
            var wallpapers = serializedObject.FindProperty("wallpapers");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(wallpapers, new GUIContent("Wallpaper List"), true);
            wallpapers.isExpanded = true;

            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+  Add a new wallpaper", customSkin.button))
                wallpapers.arraySize += 1;

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif