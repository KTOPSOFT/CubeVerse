#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(NotificationCreator))]
    public class NotificationCreatorEditor : Editor
    {
        private NotificationCreator ntfmcTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            ntfmcTarget = (NotificationCreator)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "NC Top Header");

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

            var notificationIcon = serializedObject.FindProperty("notificationIcon");
            var notificationTitle = serializedObject.FindProperty("notificationTitle");
            var notificationDescription = serializedObject.FindProperty("notificationDescription");
            var popupDescription = serializedObject.FindProperty("popupDescription");
            var notificationButtons = serializedObject.FindProperty("notificationButtons");
            var managerScript = serializedObject.FindProperty("managerScript");
            var enableButtonIcon = serializedObject.FindProperty("enableButtonIcon");
            var showOnlyOnce = serializedObject.FindProperty("showOnlyOnce");
            var enablePopupNotification = serializedObject.FindProperty("enablePopupNotification");
            var enableNotificationSound = serializedObject.FindProperty("enableNotificationSound");
            var notificationSound = serializedObject.FindProperty("notificationSound");
            var notificationColor = serializedObject.FindProperty("notificationColor");
            var notificationType = serializedObject.FindProperty("notificationType");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    DreamOSEditorHandler.DrawProperty(notificationIcon, customSkin, "Icon");
                    DreamOSEditorHandler.DrawProperty(notificationColor, customSkin, "Main Color");
                    DreamOSEditorHandler.DrawProperty(notificationTitle, customSkin, "Title");
                    DreamOSEditorHandler.DrawPropertyCW(notificationDescription, customSkin, "Description", -3);

                    if (enablePopupNotification.boolValue == true)
                        DreamOSEditorHandler.DrawPropertyCW(popupDescription, customSkin, "Popup Description", -3);

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(notificationButtons, new GUIContent("Notification Buttons"), true);
                    EditorGUI.indentLevel = 0;

                    if (GUILayout.Button("+ Create a new button", customSkin.button))
                        ntfmcTarget.CreateNewButton();

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawPropertyCW(managerScript, customSkin, "Notification Manager", 130);
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    enablePopupNotification.boolValue = DreamOSEditorHandler.DrawToggle(enablePopupNotification.boolValue, customSkin, "Enable Popup");
                    enableButtonIcon.boolValue = DreamOSEditorHandler.DrawToggle(enableButtonIcon.boolValue, customSkin, "Enable Button Icon");
                    showOnlyOnce.boolValue = DreamOSEditorHandler.DrawToggle(showOnlyOnce.boolValue, customSkin, "Show Only Once");
                    enableNotificationSound.boolValue = DreamOSEditorHandler.DrawToggle(enableNotificationSound.boolValue, customSkin, "Enable Sound");

                    if (enableNotificationSound.boolValue == true)
                        DreamOSEditorHandler.DrawProperty(notificationSound, customSkin, "Alert Sound");

                    DreamOSEditorHandler.DrawProperty(notificationType, customSkin, "Notification Type");
                    break;
            }

            if (ntfmcTarget.managerScript == null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("'Notification Manager' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif