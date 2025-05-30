#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(BSODManager))]
    public class BSODManagerEditor : Editor
    {
        private BSODManager bmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            bmTarget = (BSODManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "BSOD Top Header");

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

            var steps = serializedObject.FindProperty("steps");
            var BSODScreen = serializedObject.FindProperty("BSODScreen");
            var dreamOSCanvas = serializedObject.FindProperty("dreamOSCanvas");
            var progressText = serializedObject.FindProperty("progressText");
            var errorText = serializedObject.FindProperty("errorText");
            var onCrash = serializedObject.FindProperty("onCrash");
            var onCrashEnd = serializedObject.FindProperty("onCrashEnd");
            var progressSuffix = serializedObject.FindProperty("progressSuffix");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.PropertyField(steps, new GUIContent("Steps"));

                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onCrash, new GUIContent("On Crash"), true);
                    EditorGUILayout.PropertyField(onCrashEnd, new GUIContent("On Crash End"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(BSODScreen, customSkin, "BSOD Screen");
                    DreamOSEditorHandler.DrawProperty(dreamOSCanvas, customSkin, "DreamOS Canvas");
                    DreamOSEditorHandler.DrawProperty(progressText, customSkin, "Progress Text");
                    DreamOSEditorHandler.DrawProperty(errorText, customSkin, "Error Text");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(progressSuffix, customSkin, "Progress Suffix");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif