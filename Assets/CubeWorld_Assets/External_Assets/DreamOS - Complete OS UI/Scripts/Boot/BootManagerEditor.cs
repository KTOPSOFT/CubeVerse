#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(BootManager))]
    public class BootManagerEditor : Editor
    {
        private BootManager bootTarget;
        private GUISkin customSkin;

        private void OnEnable()
        {
            bootTarget = (BootManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Boot Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Events");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            bootTarget.currentEditorTab = DreamOSEditorHandler.DrawTabs(bootTarget.currentEditorTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Events", "Events"), customSkin.FindStyle("Tab Content")))
                bootTarget.currentEditorTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                bootTarget.currentEditorTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                bootTarget.currentEditorTab = 2;

            GUILayout.EndHorizontal();

            var bootAnimator = serializedObject.FindProperty("bootAnimator");
            var bootTime = serializedObject.FindProperty("bootTime");
            var userManager = serializedObject.FindProperty("userManager");
            var eventsAfterBoot = serializedObject.FindProperty("eventsAfterBoot");
            var onBootStart = serializedObject.FindProperty("onBootStart");

            switch (bootTarget.currentEditorTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 6);
                    EditorGUILayout.PropertyField(onBootStart, new GUIContent("On Boot Start"));
                    GUILayout.Space(8);
                    EditorGUILayout.PropertyField(eventsAfterBoot, new GUIContent("On Boot End"));
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(bootAnimator, customSkin, "Boot Animator");
                    DreamOSEditorHandler.DrawProperty(userManager, customSkin, "User Manager");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(bootTime, customSkin, "Boot Time");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif