#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(PhotoGalleryManager))]
    public class PhotoGalleryManagerEditor : Editor
    {
        private PhotoGalleryManager photoTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            photoTarget = (PhotoGalleryManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "PG Top Header");

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
            var pictureLibraryParent = serializedObject.FindProperty("pictureLibraryParent");
            var pictureLibraryButton = serializedObject.FindProperty("pictureLibraryButton");
            var photoGalleryWindow = serializedObject.FindProperty("photoGalleryWindow");
            var imageViewer = serializedObject.FindProperty("imageViewer");
            var viewerTitle = serializedObject.FindProperty("viewerTitle");
            var viewerDescription = serializedObject.FindProperty("viewerDescription");
            var sortListByName = serializedObject.FindProperty("sortListByName");
            var viewerPanelName = serializedObject.FindProperty("viewerPanelName");
            var nextButton = serializedObject.FindProperty("nextButton");
            var previousButton = serializedObject.FindProperty("previousButton");
            var allowArrowNavigation = serializedObject.FindProperty("allowArrowNavigation");

            switch (currentTab)
            {             
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(libraryAsset, customSkin, "Library Asset");
                    DreamOSEditorHandler.DrawProperty(pictureLibraryParent, customSkin, "Library Parent");
                    DreamOSEditorHandler.DrawProperty(pictureLibraryButton, customSkin, "Library Button");
                    DreamOSEditorHandler.DrawProperty(photoGalleryWindow, customSkin, "Gallery Window");
                    DreamOSEditorHandler.DrawProperty(imageViewer, customSkin, "Image Viewer");
                    DreamOSEditorHandler.DrawProperty(viewerTitle, customSkin, "Viewer Title");
                    DreamOSEditorHandler.DrawProperty(viewerDescription, customSkin, "Viewer Description");
                    DreamOSEditorHandler.DrawProperty(nextButton, customSkin, "Next Button");
                    DreamOSEditorHandler.DrawProperty(previousButton, customSkin, "Previous Button");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    sortListByName.boolValue = DreamOSEditorHandler.DrawToggle(sortListByName.boolValue, customSkin, "Sort List By Name");
                    allowArrowNavigation.boolValue = DreamOSEditorHandler.DrawToggle(allowArrowNavigation.boolValue, customSkin, "Allow Navigation With Arrow Keys");
                    DreamOSEditorHandler.DrawProperty(viewerPanelName, customSkin, "Viewer Panel Name");
                    break;            
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif