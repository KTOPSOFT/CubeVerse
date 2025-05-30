#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(RebootManager))]
    public class RebootManagerEditor : Editor
    {
        private RebootManager rebootTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            rebootTarget = (RebootManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Reboot Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var mainCanvas = serializedObject.FindProperty("mainCanvas");
            var bootManager = serializedObject.FindProperty("bootManager");
            var restartScreen = serializedObject.FindProperty("restartScreen");
            var waitTime = serializedObject.FindProperty("waitTime");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(mainCanvas, customSkin, "Main Canvas");
                    DreamOSEditorHandler.DrawProperty(bootManager, customSkin, "Boot Manager");
                    DreamOSEditorHandler.DrawProperty(restartScreen, customSkin, "Reboot Screen");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(waitTime, customSkin, "Wait Time");
                    break;            
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif