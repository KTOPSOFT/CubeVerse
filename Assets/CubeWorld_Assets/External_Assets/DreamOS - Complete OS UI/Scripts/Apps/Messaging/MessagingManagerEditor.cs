#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MessagingManager))]
    public class MessagingManagerEditor : Editor
    {
        private MessagingManager mesTarget;
        private GUISkin customSkin;

        private void OnEnable()
        {
            mesTarget = (MessagingManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Messaging Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            mesTarget.currentEditorTab = DreamOSEditorHandler.DrawTabs(mesTarget.currentEditorTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Chat List", "Chat List"), customSkin.FindStyle("Tab Content")))
                mesTarget.currentEditorTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                mesTarget.currentEditorTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                mesTarget.currentEditorTab = 2;

            GUILayout.EndHorizontal();

            var chatList = serializedObject.FindProperty("chatList");
            var chatsParent = serializedObject.FindProperty("chatsParent");
            var chatViewer = serializedObject.FindProperty("chatViewer");
            var chatMessageButton = serializedObject.FindProperty("chatMessageButton");
            var chatMessageSent = serializedObject.FindProperty("chatMessageSent");
            var chatMessageRecieved = serializedObject.FindProperty("chatMessageRecieved");
            var audioMessageSent = serializedObject.FindProperty("audioMessageSent");
            var audioMessageRecieved = serializedObject.FindProperty("audioMessageRecieved");
            var imageMessageSent = serializedObject.FindProperty("imageMessageSent");
            var imageMessageRecieved = serializedObject.FindProperty("imageMessageRecieved");
            var chatMessageTimer = serializedObject.FindProperty("chatMessageTimer");
            var chatLayout = serializedObject.FindProperty("chatLayout");
            var messageDate = serializedObject.FindProperty("messageDate");
            var beginningIndicator = serializedObject.FindProperty("beginningIndicator");
            var messageInput = serializedObject.FindProperty("messageInput");
            var timeManager = serializedObject.FindProperty("timeManager");
            var audioPlayer = serializedObject.FindProperty("audioPlayer");
            var storyTellerAnimator = serializedObject.FindProperty("storyTellerAnimator");
            var storyTellerList = serializedObject.FindProperty("storyTellerList");
            var storyTellerObject = serializedObject.FindProperty("storyTellerObject");
            var debugStoryTeller = serializedObject.FindProperty("debugStoryTeller");
            var notificationCreator = serializedObject.FindProperty("notificationCreator");
            var useNotifications = serializedObject.FindProperty("useNotifications");
            var dynamicSorting = serializedObject.FindProperty("dynamicSorting");
            var receivedMessageSound = serializedObject.FindProperty("receivedMessageSound");
            var sentMessageSound = serializedObject.FindProperty("sentMessageSound");
            var messageStoring = serializedObject.FindProperty("messageStoring");
            var saveSentMessages = serializedObject.FindProperty("saveSentMessages");

            switch (mesTarget.currentEditorTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(chatList, new GUIContent("List"), true);
                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("+  Add a new chat", customSkin.button))
                        mesTarget.AddChat();
                   
                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(chatsParent, customSkin, "Chats Parent");
                    DreamOSEditorHandler.DrawProperty(chatViewer, customSkin, "Chat Viewer");
                    DreamOSEditorHandler.DrawProperty(chatLayout, customSkin, "Chat Layout");
                    DreamOSEditorHandler.DrawProperty(chatMessageButton, customSkin, "Chat Message Button");
                    DreamOSEditorHandler.DrawProperty(chatMessageSent, customSkin, "Chat Message Sent");
                    DreamOSEditorHandler.DrawProperty(chatMessageRecieved, customSkin, "Chat Message Recieved");
                    DreamOSEditorHandler.DrawProperty(audioMessageSent, customSkin, "Audio Message Sent");
                    DreamOSEditorHandler.DrawProperty(audioMessageRecieved, customSkin, "Audio Message Recieved");
                    DreamOSEditorHandler.DrawProperty(imageMessageSent, customSkin, "Image Message Sent");
                    DreamOSEditorHandler.DrawProperty(imageMessageRecieved, customSkin, "Image Message Recieved");
                    DreamOSEditorHandler.DrawProperty(chatMessageTimer, customSkin, "Chat Message Timer");
                    DreamOSEditorHandler.DrawProperty(messageDate, customSkin, "Message Date");
                    DreamOSEditorHandler.DrawProperty(beginningIndicator, customSkin, "Beginning Indicator");
                    DreamOSEditorHandler.DrawProperty(messageInput, customSkin, "Message Input");
                    DreamOSEditorHandler.DrawProperty(timeManager, customSkin, "Time Manager");
                    DreamOSEditorHandler.DrawProperty(audioPlayer, customSkin, "Audio Player");
                    DreamOSEditorHandler.DrawProperty(storyTellerAnimator, customSkin, "StoryTeller Animator");
                    DreamOSEditorHandler.DrawProperty(storyTellerList, customSkin, "StoryTeller List");
                    DreamOSEditorHandler.DrawProperty(storyTellerObject, customSkin, "StoryTeller Object");
                    DreamOSEditorHandler.DrawProperty(messageStoring, customSkin, "Message Storing");        

                    if (useNotifications.boolValue == true)
                        DreamOSEditorHandler.DrawProperty(notificationCreator, customSkin, "Notification Creator");

                    break;
                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(sentMessageSound, customSkin, "Sent Message Sound");
                    DreamOSEditorHandler.DrawProperty(receivedMessageSound, customSkin, "Received Message Sound");
                    debugStoryTeller.boolValue = DreamOSEditorHandler.DrawToggle(debugStoryTeller.boolValue, customSkin, "Debug StoryTeller");
                    useNotifications.boolValue = DreamOSEditorHandler.DrawToggle(useNotifications.boolValue, customSkin, "Use Notifications");
                    dynamicSorting.boolValue = DreamOSEditorHandler.DrawToggle(dynamicSorting.boolValue, customSkin, "Dynamic Sorting");
                    saveSentMessages.boolValue = DreamOSEditorHandler.DrawToggle(saveSentMessages.boolValue, customSkin, "Save Sent Messages");
                    break;
            }

            if (saveSentMessages.boolValue == true && mesTarget.messageStoring == null)
            {
                EditorGUILayout.HelpBox("'Save Sent Messages' is enabled but 'Message Storing' is not assigned. " +
                    "Please add and/or assign 'Message Storing' component via Resources tab.", MessageType.Error);

                if (GUILayout.Button("+  Create Message Storing", customSkin.button))
                {
                    MessageStoring tempMS = mesTarget.gameObject.AddComponent<MessageStoring>();
                    mesTarget.messageStoring = tempMS;
                    tempMS.messagingManager = mesTarget;

                    PrefabUtility.RecordPrefabInstancePropertyModifications(mesTarget);
                    Undo.RecordObject(this, "Created message storing");
                    EditorUtility.SetDirty(this);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mesTarget.gameObject.scene);
                }
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif