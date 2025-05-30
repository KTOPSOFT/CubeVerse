#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WidgetManager))]
    public class WidgetManagerEditor : Editor
    {
        private WidgetManager wmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            wmTarget = (WidgetManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Widget Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var widgetLibrary = serializedObject.FindProperty("widgetLibrary");
            var widgetParent = serializedObject.FindProperty("widgetParent");
            var widgetLibraryParent = serializedObject.FindProperty("widgetLibraryParent");
            var widgetButton = serializedObject.FindProperty("widgetButton");
            var sortListByName = serializedObject.FindProperty("sortListByName");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(widgetLibrary, customSkin, "Widget Library");
                    DreamOSEditorHandler.DrawProperty(widgetParent, customSkin, "Widget Parent");
                    DreamOSEditorHandler.DrawProperty(widgetLibraryParent, customSkin, "Library Parent");
                    DreamOSEditorHandler.DrawProperty(widgetButton, customSkin, "Widget Button");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    sortListByName.boolValue = DreamOSEditorHandler.DrawToggle(sortListByName.boolValue, customSkin, "Sort List By Name");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif