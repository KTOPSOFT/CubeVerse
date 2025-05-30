//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(TimedEvent))]
    public class TimedEventEditor : Editor
    {
        private TimedEvent teTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            teTarget = (TimedEvent)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "TE Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var timer = serializedObject.FindProperty("timer");
            var enableAtStart = serializedObject.FindProperty("enableAtStart");
            var timerAction = serializedObject.FindProperty("timerAction");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(timer, customSkin, "Timer (s)");
                    enableAtStart.boolValue = DreamOSEditorHandler.DrawToggle(enableAtStart.boolValue, customSkin, "Enable At Start");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 6);
                    EditorGUILayout.PropertyField(timerAction, new GUIContent("Timer Events"), true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif