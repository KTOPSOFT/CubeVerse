#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(NotificationManager))]
    public class NotificationManagerEditor : Editor
    {
        private NotificationManager nmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            nmTarget = (NotificationManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Notification Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var notificationListRect = serializedObject.FindProperty("notificationListRect");
            var notificationParent = serializedObject.FindProperty("notificationParent");
            var popupNotificationParent = serializedObject.FindProperty("popupNotificationParent");
            var notificationButton = serializedObject.FindProperty("notificationButton");
            var popupNotification = serializedObject.FindProperty("popupNotification");
            var standardNotification = serializedObject.FindProperty("standardNotification");
            var visibleTime = serializedObject.FindProperty("visibleTime");

            switch (currentTab)
            {             
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(notificationListRect, customSkin, "List Rect");
                    DreamOSEditorHandler.DrawProperty(notificationParent, customSkin, "Parent");
                    DreamOSEditorHandler.DrawProperty(popupNotificationParent, customSkin, "Popup Parent");
                    DreamOSEditorHandler.DrawProperty(notificationButton, customSkin, "Button");
                    DreamOSEditorHandler.DrawProperty(standardNotification, customSkin, "Standard Prefab");
                    DreamOSEditorHandler.DrawProperty(popupNotification, customSkin, "Popup Prefab");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(visibleTime, customSkin, "Popup Visible Time");
                    break;            
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif