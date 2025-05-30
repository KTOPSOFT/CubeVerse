#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(GlobalTime))]
    public class GlobalTimeEditor : Editor
    {
        private GlobalTime timeTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            timeTarget = (GlobalTime)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Time Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Timed Events");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Time", "Time"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Timed Events", "Timed Events"), customSkin.FindStyle("Tab Content")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var timedEvents = serializedObject.FindProperty("timedEvents");
            var timeMultiplier = serializedObject.FindProperty("timeMultiplier");
            var currentDay = serializedObject.FindProperty("currentDay");
            var currentMonth = serializedObject.FindProperty("currentMonth");
            var currentYear = serializedObject.FindProperty("currentYear");
            var currentHour = serializedObject.FindProperty("currentHour");
            var currentMinute = serializedObject.FindProperty("currentMinute");
            var currentSecond = serializedObject.FindProperty("currentSecond");
            var saveAndGetValues = serializedObject.FindProperty("saveAndGetValues");
            var useShortClockFormat = serializedObject.FindProperty("useShortClockFormat");
            var defaultShortTime = serializedObject.FindProperty("defaultShortTime");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    DreamOSEditorHandler.DrawProperty(timeMultiplier, customSkin, "Time Multiplier");
                    DreamOSEditorHandler.DrawProperty(currentYear, customSkin, "Current Year");
                    DreamOSEditorHandler.DrawProperty(currentMonth, customSkin, "Current Month");
                    DreamOSEditorHandler.DrawProperty(currentDay, customSkin, "Current Day");
                    DreamOSEditorHandler.DrawProperty(currentHour, customSkin, "Current Hour");
                    DreamOSEditorHandler.DrawProperty(currentMinute, customSkin, "Current Minute");
                    DreamOSEditorHandler.DrawProperty(currentSecond, customSkin, "Current Second");

                    if (saveAndGetValues.boolValue == true)
                        EditorGUILayout.HelpBox("Save Data is enabled. Some of these variables won't be used if there's a stored data.", MessageType.Info);

                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.PropertyField(timedEvents, new GUIContent("Event List"), true);
                    timedEvents.isExpanded = true;

                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("+  Add a new event", customSkin.button))
                        timeTarget.AddTimedEvent();

                    GUILayout.EndVertical();
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    useShortClockFormat.boolValue = DreamOSEditorHandler.DrawToggle(useShortClockFormat.boolValue, customSkin, "Use Short Clock Format");
                    saveAndGetValues.boolValue = DreamOSEditorHandler.DrawToggle(saveAndGetValues.boolValue, customSkin, "Save Values");

                    if (useShortClockFormat.boolValue == true) { DreamOSEditorHandler.DrawProperty(defaultShortTime, customSkin, "Default Short Time"); }
                    if (saveAndGetValues.boolValue == true)
                    {
                        if (GUILayout.Button("Clear Stored Data", customSkin.button))
                            timeTarget.DeleteSavedData();
                    }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif