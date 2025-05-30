#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WindowManager))]
    public class WindowManagerEditor : Editor
    {
        private WindowManager wmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            wmTarget = (WindowManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "WM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Chat List", "Chat List"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var normalizeImage = serializedObject.FindProperty("normalizeImage");
            var fullscreenImage = serializedObject.FindProperty("fullscreenImage");
            var taskbarButton = serializedObject.FindProperty("taskbarButton");
            var windowDragger = serializedObject.FindProperty("windowDragger");
            var windowResize = serializedObject.FindProperty("windowResize");
            var onEnableEvents = serializedObject.FindProperty("onEnableEvents");
            var onLaunchEvents = serializedObject.FindProperty("onLaunchEvents");
            var onQuitEvents = serializedObject.FindProperty("onQuitEvents");
            var enableBackgroundBlur = serializedObject.FindProperty("enableBackgroundBlur");
            var hasNavDrawer = serializedObject.FindProperty("hasNavDrawer");
            var enableMobileMode = serializedObject.FindProperty("enableMobileMode");
            var windowContent = serializedObject.FindProperty("windowContent");
            var navbarRect = serializedObject.FindProperty("navbarRect");
            var minNavbarWidth = serializedObject.FindProperty("minNavbarWidth");
            var maxNavbarWidth = serializedObject.FindProperty("maxNavbarWidth");
            var smoothness = serializedObject.FindProperty("smoothness");
            var defaultNavbarState = serializedObject.FindProperty("defaultNavbarState");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    if (wmTarget.GetComponent<CanvasGroup>().alpha == 0)
                    {
                        if (GUILayout.Button("Make It Visible", customSkin.button))
                            wmTarget.GetComponent<CanvasGroup>().alpha = 1;
                    }

                    else
                    {
                        if (GUILayout.Button("Make It Invisible", customSkin.button))
                            wmTarget.GetComponent<CanvasGroup>().alpha = 0;
                    }

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onEnableEvents, new GUIContent("On Enable"), true);
                    EditorGUILayout.PropertyField(onLaunchEvents, new GUIContent("On Launch"), true);
                    EditorGUILayout.PropertyField(onQuitEvents, new GUIContent("On Quit"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);

                    if (hasNavDrawer.boolValue == true)
                    {
                        DreamOSEditorHandler.DrawProperty(windowContent, customSkin, "Main Content");
                        DreamOSEditorHandler.DrawProperty(navbarRect, customSkin, "Navbar Panel");
                    }

                    DreamOSEditorHandler.DrawProperty(taskbarButton, customSkin, "Taskbar Button");

                    if (enableMobileMode.boolValue == false)
                    {
                        DreamOSEditorHandler.DrawProperty(normalizeImage, customSkin, "Normalize Image");
                        DreamOSEditorHandler.DrawProperty(fullscreenImage, customSkin, "Fullscreen Image");
                        DreamOSEditorHandler.DrawProperty(windowDragger, customSkin, "Window Dragger");
                        DreamOSEditorHandler.DrawProperty(windowResize, customSkin, "Window Resize");
                    }

                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    enableMobileMode.boolValue = DreamOSEditorHandler.DrawToggle(enableMobileMode.boolValue, customSkin, "Enable Mobile Mode");

                    if (enableMobileMode.boolValue == false)
                    {
                        enableBackgroundBlur.boolValue = DreamOSEditorHandler.DrawToggle(enableBackgroundBlur.boolValue, customSkin, "Enable Background Blur");

                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Space(-3);
                        hasNavDrawer.boolValue = DreamOSEditorHandler.DrawTogglePlain(hasNavDrawer.boolValue, customSkin, "Use Nav Drawer");
                        GUILayout.Space(4);

                        if (hasNavDrawer.boolValue == true)
                        {
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);

                            EditorGUILayout.LabelField(new GUIContent("Min Navbar Width"), customSkin.FindStyle("Text"), GUILayout.Width(130));
                            minNavbarWidth.floatValue = EditorGUILayout.Slider(minNavbarWidth.floatValue, 1, maxNavbarWidth.floatValue - 1);

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);

                            EditorGUILayout.LabelField(new GUIContent("Max Navbar Width"), customSkin.FindStyle("Text"), GUILayout.Width(130));
                            maxNavbarWidth.floatValue = EditorGUILayout.Slider(maxNavbarWidth.floatValue, minNavbarWidth.floatValue + 1, 1000);

                            GUILayout.EndHorizontal();

                            DreamOSEditorHandler.DrawProperty(smoothness, customSkin, "Smoothness");
                            DreamOSEditorHandler.DrawProperty(defaultNavbarState, customSkin, "Default Navbar State");

                            if (wmTarget.navbarRect == null || wmTarget.windowContent == null)
                            {
                                EditorGUILayout.HelpBox("There are missing resources!", MessageType.Warning);

                                DreamOSEditorHandler.DrawProperty(windowContent, customSkin, "Main Content");
                                DreamOSEditorHandler.DrawProperty(navbarRect, customSkin, "Navbar Panel");
                            }
                        }

                        GUILayout.EndVertical();
                    }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif