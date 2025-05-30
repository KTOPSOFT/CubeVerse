//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WebBrowserManager))]
    public class WebBrowserManagerEditor : Editor
    {
        private WebBrowserManager webTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            webTarget = (WebBrowserManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "WB Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");
            
            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var networkManager = serializedObject.FindProperty("networkManager");
            var webLibrary = serializedObject.FindProperty("webLibrary");
            var tabTitle = serializedObject.FindProperty("tabTitle");
            var tabIcon = serializedObject.FindProperty("tabIcon");
            var pageViewer = serializedObject.FindProperty("pageViewer");
            var urlField = serializedObject.FindProperty("urlField");
            var progressBar = serializedObject.FindProperty("progressBar");
            var previousButton = serializedObject.FindProperty("previousButton");
            var nextButton = serializedObject.FindProperty("nextButton");
            var favAnimator = serializedObject.FindProperty("favAnimator");
            var favsParent = serializedObject.FindProperty("favsParent");
            var favListButton = serializedObject.FindProperty("favListButton");
            var downloadedFile = serializedObject.FindProperty("downloadedFile");
            var downloadFolder = serializedObject.FindProperty("downloadFolder");
            var downloadsWindow = serializedObject.FindProperty("downloadsWindow");
            var downloadEmpty = serializedObject.FindProperty("downloadEmpty");
            var loadingTime = serializedObject.FindProperty("loadingTime");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(networkManager, customSkin, "Network Manager");
                    DreamOSEditorHandler.DrawProperty(webLibrary, customSkin, "Web Library");
                    DreamOSEditorHandler.DrawProperty(tabTitle, customSkin, "Tab Title");
                    DreamOSEditorHandler.DrawProperty(tabIcon, customSkin, "Tab Icon");
                    DreamOSEditorHandler.DrawProperty(pageViewer, customSkin, "Page Viewer");
                    DreamOSEditorHandler.DrawProperty(urlField, customSkin, "URL Field");
                    DreamOSEditorHandler.DrawProperty(progressBar, customSkin, "Progress Bar");
                    DreamOSEditorHandler.DrawProperty(previousButton, customSkin, "Previous Button");
                    DreamOSEditorHandler.DrawProperty(nextButton, customSkin, "Next Button");
                    DreamOSEditorHandler.DrawProperty(favAnimator, customSkin, "Fav Animator");
                    DreamOSEditorHandler.DrawProperty(favsParent, customSkin, "Fav Parent");
                    DreamOSEditorHandler.DrawProperty(favListButton, customSkin, "Fav List Button");
                    DreamOSEditorHandler.DrawProperty(downloadedFile, customSkin, "Downloaded File");
                    DreamOSEditorHandler.DrawProperty(downloadFolder, customSkin, "Download Folder");
                    DreamOSEditorHandler.DrawProperty(downloadEmpty, customSkin, "Downloads Empty");
                    DreamOSEditorHandler.DrawProperty(downloadsWindow, customSkin, "Downloads Window");         
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(loadingTime, customSkin, "Page Loading TIme");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif