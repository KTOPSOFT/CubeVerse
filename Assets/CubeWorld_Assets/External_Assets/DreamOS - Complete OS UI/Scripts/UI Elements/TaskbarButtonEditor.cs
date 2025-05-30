#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TaskBarButton))]
    public class TaskbarButtonEditor : Editor
    {
        private TaskBarButton buttonTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            buttonTarget = (TaskBarButton)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Button Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Settings");
            toolbarTabs[1] = new GUIContent("Resources");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var buttonTitle = serializedObject.FindProperty("buttonTitle");
            var defaultPinState = serializedObject.FindProperty("defaultPinState");
            var appElements = serializedObject.FindProperty("appElements");
            var buttonAnimator = serializedObject.FindProperty("buttonAnimator");
            var windowManager = serializedObject.FindProperty("windowManager");
            var contextMenu = serializedObject.FindProperty("contextMenu");
            var contextBlur = serializedObject.FindProperty("contextBlur");
            var headerButton = serializedObject.FindProperty("headerButton");
            var closeButton = serializedObject.FindProperty("closeButton");
            var pinButton = serializedObject.FindProperty("pinButton");
            var unpinButton = serializedObject.FindProperty("unpinButton");
            var onClick = serializedObject.FindProperty("onClick");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(buttonTitle, customSkin, "App Title");
                    DreamOSEditorHandler.DrawProperty(defaultPinState, customSkin, "Default Pin State");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onClick, new GUIContent("On Click"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(appElements, new GUIContent("App Elements"), true);
                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();
                    DreamOSEditorHandler.DrawProperty(buttonAnimator, customSkin, "Button Animator");
                    DreamOSEditorHandler.DrawProperty(windowManager, customSkin, "Window Manager");
                    DreamOSEditorHandler.DrawProperty(contextMenu, customSkin, "Context Menu");
                    DreamOSEditorHandler.DrawProperty(contextBlur, customSkin, "Context Blur");
                    DreamOSEditorHandler.DrawProperty(headerButton, customSkin, "Header Button");
                    DreamOSEditorHandler.DrawProperty(closeButton, customSkin, "Close Button");
                    DreamOSEditorHandler.DrawProperty(pinButton, customSkin, "Pin Button");
                    DreamOSEditorHandler.DrawProperty(unpinButton, customSkin, "Unpin Button");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif