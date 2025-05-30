#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : Editor
    {
        private SettingsManager settingsTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            settingsTarget = (SettingsManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Settings Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var themeManager = serializedObject.FindProperty("themeManager");
            var rebootManager = serializedObject.FindProperty("rebootManager");
            var userManager = serializedObject.FindProperty("userManager");
            var setupManager = serializedObject.FindProperty("setupManager");
            var lockscreenImage = serializedObject.FindProperty("lockscreenImage");
            var desktopDragger = serializedObject.FindProperty("desktopDragger");
            var accentColorList = serializedObject.FindProperty("accentColorList");
            var accentReversedColorList = serializedObject.FindProperty("accentReversedColorList");
            var lockDesktopItems = serializedObject.FindProperty("lockDesktopItems");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(themeManager, customSkin, "Theme Manager");
                    DreamOSEditorHandler.DrawProperty(rebootManager, customSkin, "Reboot Manager");
                    DreamOSEditorHandler.DrawProperty(userManager, customSkin, "User Manager");
                    DreamOSEditorHandler.DrawProperty(setupManager, customSkin, "Setup Manager");
                    DreamOSEditorHandler.DrawProperty(lockscreenImage, customSkin, "Lockscreen Image");
                    DreamOSEditorHandler.DrawProperty(desktopDragger, customSkin, "Desktop Dragger");
                    DreamOSEditorHandler.DrawProperty(accentColorList, customSkin, "Accent Color List");
                    DreamOSEditorHandler.DrawProperty(accentReversedColorList, customSkin, "Accent Color R List");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    lockDesktopItems.boolValue = DreamOSEditorHandler.DrawToggle(lockDesktopItems.boolValue, customSkin, "Lock Desktop Items");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif