#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(VideoPlayerManager))]
    public class VideoPlayerManagerEditor : Editor
    {
        private VideoPlayerManager videoTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            videoTarget = (VideoPlayerManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "VP Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var libraryAsset = serializedObject.FindProperty("libraryAsset");
            var videoLibraryParent = serializedObject.FindProperty("videoLibraryParent");
            var videoLibraryButton = serializedObject.FindProperty("videoLibraryButton");
            var videoPlayerWindow = serializedObject.FindProperty("videoPlayerWindow");
            var videoControlsAnimator = serializedObject.FindProperty("videoControlsAnimator");
            var miniPlayerAnimator = serializedObject.FindProperty("miniPlayerAnimator");
            var sortListByName = serializedObject.FindProperty("sortListByName");
            var controlsOutTime = serializedObject.FindProperty("controlsOutTime");
            var seekTime = serializedObject.FindProperty("seekTime");
            var videoPanelName = serializedObject.FindProperty("videoPanelName"); 

            switch (currentTab)
            {             
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(libraryAsset, customSkin, "Library Asset");
                    DreamOSEditorHandler.DrawProperty(videoLibraryParent, customSkin, "Library Parent");
                    DreamOSEditorHandler.DrawProperty(videoLibraryButton, customSkin, "Library Button");
                    DreamOSEditorHandler.DrawProperty(videoPlayerWindow, customSkin, "Library Window");
                    DreamOSEditorHandler.DrawProperty(videoControlsAnimator, customSkin, "Controls Animator");
                    DreamOSEditorHandler.DrawProperty(miniPlayerAnimator, customSkin, "Mini Player Animator");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    sortListByName.boolValue = DreamOSEditorHandler.DrawToggle(sortListByName.boolValue, customSkin, "Sort Library By Name");
                    DreamOSEditorHandler.DrawProperty(controlsOutTime, customSkin, "Fade Controls (s)");
                    DreamOSEditorHandler.DrawProperty(seekTime, customSkin, "Seek Time (s)");
                    DreamOSEditorHandler.DrawProperty(videoPanelName, customSkin, "Video Panel Name");
                    break;            
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif