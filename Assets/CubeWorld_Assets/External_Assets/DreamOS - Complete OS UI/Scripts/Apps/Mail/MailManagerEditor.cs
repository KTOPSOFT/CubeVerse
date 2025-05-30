#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MailManager))]
    public class MailManagerEditor : Editor
    {
        private MailManager mmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            mmTarget = (MailManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Mail Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Chat List", "Chat List"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var mailViewer = serializedObject.FindProperty("mailViewer");
            var inboxParent = serializedObject.FindProperty("inboxParent");
            var sentParent = serializedObject.FindProperty("sentParent");
            var junkParent = serializedObject.FindProperty("junkParent");
            var attachmentPanel = serializedObject.FindProperty("attachmentPanel");
            var attachmentParent = serializedObject.FindProperty("attachmentParent");
            var itemTemplate = serializedObject.FindProperty("itemTemplate");
            var mailTemplate = serializedObject.FindProperty("mailTemplate");
            var attachmentItem = serializedObject.FindProperty("attachmentItem");
            var musicManager = serializedObject.FindProperty("musicManager");
            var noteManager = serializedObject.FindProperty("noteManager");
            var pictureManager = serializedObject.FindProperty("pictureManager");
            var videoManager = serializedObject.FindProperty("videoManager");
            var fromPrefix = serializedObject.FindProperty("fromPrefix");
            var fromSuffix = serializedObject.FindProperty("fromSuffix");
            var mailList = serializedObject.FindProperty("mailList");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(mailList, new GUIContent("Mail Items"), true);
                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("+  Add a new mail item", customSkin.button))
                        mailList.arraySize += 1;

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(mailViewer, customSkin, "Mail Viewer");
                    DreamOSEditorHandler.DrawProperty(inboxParent, customSkin, "Inbox Parent");
                    DreamOSEditorHandler.DrawProperty(sentParent, customSkin, "Sent Parent");
                    DreamOSEditorHandler.DrawProperty(junkParent, customSkin, "Junk Parent");
                    DreamOSEditorHandler.DrawProperty(attachmentPanel, customSkin, "Attachment Panel");
                    DreamOSEditorHandler.DrawProperty(attachmentParent, customSkin, "Attachment Parent");
                    DreamOSEditorHandler.DrawProperty(itemTemplate, customSkin, "Item Template");
                    DreamOSEditorHandler.DrawProperty(mailTemplate, customSkin, "Mail Template");
                    DreamOSEditorHandler.DrawProperty(attachmentItem, customSkin, "Attachment Item");
                    DreamOSEditorHandler.DrawProperty(musicManager, customSkin, "Music Manager");
                    DreamOSEditorHandler.DrawProperty(noteManager, customSkin, "Note Manager");
                    DreamOSEditorHandler.DrawProperty(pictureManager, customSkin, "Picture Manager");
                    DreamOSEditorHandler.DrawProperty(videoManager, customSkin, "Video Manager");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(fromPrefix, customSkin, "From Prefix");
                    DreamOSEditorHandler.DrawProperty(fromSuffix, customSkin, "From Suffix");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif