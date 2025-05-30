#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(ReminderManager))]
    public class ReminderManagerEditor : Editor
    {
        private ReminderManager rTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            rTarget = (ReminderManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Reminder Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var globalTime = serializedObject.FindProperty("globalTime");
            var eventTitleObject = serializedObject.FindProperty("eventTitleObject");
            var eventHourObject = serializedObject.FindProperty("eventHourObject");
            var eventMinuteObject = serializedObject.FindProperty("eventMinuteObject");
            var hourSelector = serializedObject.FindProperty("hourSelector");
            var minuteSelector = serializedObject.FindProperty("minuteSelector");
            var reminderNotification = serializedObject.FindProperty("reminderNotification");
            var reminder1 = serializedObject.FindProperty("reminder1");
            var reminder2 = serializedObject.FindProperty("reminder2");
            var reminder3 = serializedObject.FindProperty("reminder3");
            var reminder4 = serializedObject.FindProperty("reminder4");
            var notificationTitle = serializedObject.FindProperty("notificationTitle");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(globalTime, customSkin, "Global Time");
                    DreamOSEditorHandler.DrawProperty(eventTitleObject, customSkin, "Event Title Object");
                    DreamOSEditorHandler.DrawProperty(eventHourObject, customSkin, "Event Hour Object");
                    DreamOSEditorHandler.DrawProperty(eventMinuteObject, customSkin, "Event Minute Object");
                    DreamOSEditorHandler.DrawProperty(hourSelector, customSkin, "Hour Selector");
                    DreamOSEditorHandler.DrawProperty(minuteSelector, customSkin, "Minute Selector");
                    DreamOSEditorHandler.DrawProperty(reminderNotification, customSkin, "Reminder Notification");
                    DreamOSEditorHandler.DrawProperty(reminder1, customSkin, "Reminder #1");
                    DreamOSEditorHandler.DrawProperty(reminder2, customSkin, "Reminder #2");
                    DreamOSEditorHandler.DrawProperty(reminder3, customSkin, "Reminder #3");
                    DreamOSEditorHandler.DrawProperty(reminder4, customSkin, "Reminder #4");         
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(notificationTitle, customSkin, "Notification Title");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif