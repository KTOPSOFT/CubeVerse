#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MessageChat))]
    public class MessageChatEditor : Editor
    {
        private GUISkin customSkin;
        private MessageChat mcTarget;

        void OnEnable()
        {
            mcTarget = (MessageChat)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            // Settings
            var useDynamicMessages = serializedObject.FindProperty("useDynamicMessages");
            var useStoryTeller = serializedObject.FindProperty("useStoryTeller");

            DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 8);
            useDynamicMessages.boolValue = DreamOSEditorHandler.DrawToggle(useDynamicMessages.boolValue, customSkin, "Use Dynamic Messages");
            useStoryTeller.boolValue = DreamOSEditorHandler.DrawToggle(useStoryTeller.boolValue, customSkin, "Use StoryTeller [Beta]");

            // Content
            var messageList = serializedObject.FindProperty("messageList");
            var dynamicMessages = serializedObject.FindProperty("dynamicMessages");
            var storyTeller = serializedObject.FindProperty("storyTeller");

            DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 8);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(messageList, new GUIContent("Message List"), true);
            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+  Add a new message", customSkin.button))
                messageList.arraySize += 1;

            GUILayout.EndVertical();

            if (useDynamicMessages.boolValue == true)
            {
                GUILayout.Space(8);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(dynamicMessages, new GUIContent("Dynamic Messages"), true);
                EditorGUI.indentLevel = 0;

                if (GUILayout.Button("+  Add a new message", customSkin.button))
                    dynamicMessages.arraySize += 1;

                GUILayout.EndVertical();
            }

            if (useStoryTeller.boolValue == true)
            {
                GUILayout.Space(8);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(storyTeller, new GUIContent("StoryTeller"), true);
                EditorGUI.indentLevel = 0;

                if (GUILayout.Button("+  Add a new item", customSkin.button))
                    storyTeller.arraySize += 1;
               
                // Node graph testing
                /*
                if (GUILayout.Button("CHECK OUT", customSkin.button))
                {
                    EditorPrefs.SetString("DreamOS_Storyteller_Path", AssetDatabase.GetAssetPath(mcTarget));
                    EditorPrefs.SetString("DreamOS_Storyteller_Helper", mcTarget.name);
                    StorytellerGraph tempWindow = (StorytellerGraph)EditorWindow.GetWindow(typeof(StorytellerGraph));
                    tempWindow.RequestDataOperation(false);
                }*/

                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif