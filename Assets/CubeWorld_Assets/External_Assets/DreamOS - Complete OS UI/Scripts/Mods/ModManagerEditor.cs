#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(ModManager))]
    public class ModManagerEditor : Editor
    {
        private ModManager mmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            mmTarget = (ModManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Mod Manager Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var modLibraryElement = serializedObject.FindProperty("modLibraryElement");
            var modLibraryParent = serializedObject.FindProperty("modLibraryParent");
            var noModsIndicator = serializedObject.FindProperty("noModsIndicator");
            var musicPlayer = serializedObject.FindProperty("musicPlayer");
            var musicPlayerID = serializedObject.FindProperty("musicPlayerID");
            var notepad = serializedObject.FindProperty("notepad");
            var notepadID = serializedObject.FindProperty("notepadID");
            var photoGallery = serializedObject.FindProperty("photoGallery");
            var photoGalleryID = serializedObject.FindProperty("photoGalleryID");
            var videoPlayer = serializedObject.FindProperty("videoPlayer");
            var videoPlayerID = serializedObject.FindProperty("videoPlayerID");
            var defaultModState = serializedObject.FindProperty("defaultModState");
            var initOnEnable = serializedObject.FindProperty("initOnEnable");
            var subPath = serializedObject.FindProperty("subPath");
            var dataName = serializedObject.FindProperty("dataName");
            var fileExtension = serializedObject.FindProperty("fileExtension");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    mmTarget.enableMusicPlayerModule = DreamOSEditorHandler.DrawTogglePlain(mmTarget.enableMusicPlayerModule, customSkin, "Music Player Module");
                    GUILayout.Space(3);

                    if (mmTarget.enableMusicPlayerModule == true)
                    {
                        DreamOSEditorHandler.DrawProperty(musicPlayer, customSkin, "Music Player");
                        DreamOSEditorHandler.DrawProperty(musicPlayerID, customSkin, "Music Player ID");
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    mmTarget.enableNotepadModule = DreamOSEditorHandler.DrawTogglePlain(mmTarget.enableNotepadModule, customSkin, "Notepad Module");
                    GUILayout.Space(3);

                    if (mmTarget.enableNotepadModule == true)
                    {
                        DreamOSEditorHandler.DrawProperty(notepad, customSkin, "Notepad");
                        DreamOSEditorHandler.DrawProperty(notepadID, customSkin, "Notepad ID");
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    mmTarget.enablePhotoGalleryModule = DreamOSEditorHandler.DrawTogglePlain(mmTarget.enablePhotoGalleryModule, customSkin, "Photo Gallery Module");
                    GUILayout.Space(3);

                    if (mmTarget.enablePhotoGalleryModule == true)
                    {
                        DreamOSEditorHandler.DrawProperty(photoGallery, customSkin, "Photo Gallery Module");
                        DreamOSEditorHandler.DrawProperty(photoGalleryID, customSkin, "Photo Gallery ID");
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(-3);
                    mmTarget.enableVideoPlayerModule = DreamOSEditorHandler.DrawTogglePlain(mmTarget.enableVideoPlayerModule, customSkin, "Video Player Module");
                    GUILayout.Space(3);

                    if (mmTarget.enableVideoPlayerModule == true)
                    {
                        DreamOSEditorHandler.DrawProperty(videoPlayer, customSkin, "Video Player");
                        DreamOSEditorHandler.DrawProperty(videoPlayerID, customSkin, "Video Player ID");
                    }

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(modLibraryElement, customSkin, "Mod Library Element");
                    DreamOSEditorHandler.DrawProperty(modLibraryParent, customSkin, "Mod Library Parent");
                    DreamOSEditorHandler.DrawProperty(noModsIndicator, customSkin, "No Mods Indicator");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    defaultModState.boolValue = DreamOSEditorHandler.DrawToggle(defaultModState.boolValue, customSkin, "Default Mod State");
                    initOnEnable.boolValue = DreamOSEditorHandler.DrawToggle(initOnEnable.boolValue, customSkin, "Initialize On Enable");
                    DreamOSEditorHandler.DrawProperty(subPath, customSkin, "Sub Path");
                    DreamOSEditorHandler.DrawProperty(dataName, customSkin, "Data Name");
                    DreamOSEditorHandler.DrawProperty(fileExtension, customSkin, "File Extension");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif