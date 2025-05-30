#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(ContextMenuManager))]
    public class ContextMenuManagerEditor : Editor
    {
        private ContextMenuManager cmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            cmTarget = (ContextMenuManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "CM Top Header");

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

            var enableBlur = serializedObject.FindProperty("enableBlur");
            var contextContent = serializedObject.FindProperty("contextContent");
            var contextAnimator = serializedObject.FindProperty("contextAnimator");
            var contextButton = serializedObject.FindProperty("contextButton");
            var contextSeparator = serializedObject.FindProperty("contextSeparator");
            var contextSubMenu = serializedObject.FindProperty("contextSubMenu");
            var autoSubMenuPosition = serializedObject.FindProperty("autoSubMenuPosition");
            var subMenuBehaviour = serializedObject.FindProperty("subMenuBehaviour");
            var vBorderTop = serializedObject.FindProperty("vBorderTop");
            var vBorderBottom = serializedObject.FindProperty("vBorderBottom");
            var hBorderLeft = serializedObject.FindProperty("hBorderLeft");
            var hBorderRight = serializedObject.FindProperty("hBorderRight");
            var cameraSource = serializedObject.FindProperty("cameraSource");
            var targetCamera = serializedObject.FindProperty("targetCamera");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    DreamOSEditorHandler.DrawProperty(vBorderTop, customSkin, "Vertical Top");
                    DreamOSEditorHandler.DrawProperty(vBorderBottom, customSkin, "Vertical Bottom");
                    DreamOSEditorHandler.DrawProperty(hBorderLeft, customSkin, "Horizontal Left");
                    DreamOSEditorHandler.DrawProperty(hBorderRight, customSkin, "Horizontal Right");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(contextContent, customSkin, "Context Content");
                    DreamOSEditorHandler.DrawProperty(contextAnimator, customSkin, "Context Animator");
                    DreamOSEditorHandler.DrawProperty(contextButton, customSkin, "Button Preset");
                    DreamOSEditorHandler.DrawProperty(contextSeparator, customSkin, "Seperator Preset");
                    DreamOSEditorHandler.DrawProperty(contextSubMenu, customSkin, "Sub Menu Preset");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    enableBlur.boolValue = DreamOSEditorHandler.DrawToggle(enableBlur.boolValue, customSkin, "Enable Blur");
                    autoSubMenuPosition.boolValue = DreamOSEditorHandler.DrawToggle(autoSubMenuPosition.boolValue, customSkin, "Auto Sub Menu Position");
                    DreamOSEditorHandler.DrawProperty(subMenuBehaviour, customSkin, "Sub Menu Behaviour");
                    DreamOSEditorHandler.DrawProperty(cameraSource, customSkin, "Camera Source");

                    if (cmTarget.cameraSource == ContextMenuManager.CameraSource.Custom)
                        DreamOSEditorHandler.DrawProperty(targetCamera, customSkin, "Target Camera");

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif