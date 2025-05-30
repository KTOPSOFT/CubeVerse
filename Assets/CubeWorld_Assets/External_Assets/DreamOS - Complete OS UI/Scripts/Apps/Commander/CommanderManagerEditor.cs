#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(CommanderManager))]
    public class CommanderManagerEditor : Editor
    {
        private CommanderManager cTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            cTarget = (CommanderManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Commander Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Playlists", "Playlists"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var commands = serializedObject.FindProperty("commands");
            var errorText = serializedObject.FindProperty("errorText");
            var onEnableText = serializedObject.FindProperty("onEnableText");
            var textColor = serializedObject.FindProperty("textColor");
            var commandInput = serializedObject.FindProperty("commandInput");
            var commandHistory = serializedObject.FindProperty("commandHistory");
            var scrollbar = serializedObject.FindProperty("scrollbar");
            var getTimeData = serializedObject.FindProperty("getTimeData");
            var timeManager = serializedObject.FindProperty("timeManager");
            var timeColorCode = serializedObject.FindProperty("timeColorCode");
            var useTypewriterEffect = serializedObject.FindProperty("useTypewriterEffect");
            var typewriterDelay = serializedObject.FindProperty("typewriterDelay");
            var antiFlicker = serializedObject.FindProperty("antiFlicker");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(commands, new GUIContent("Commands"), true);
                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("+  Add a new command", customSkin.button))
                        cTarget.AddNewCommand();

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    DreamOSEditorHandler.DrawProperty(commandInput, customSkin, "Command Input");
                    DreamOSEditorHandler.DrawProperty(commandHistory, customSkin, "Command History");
                    DreamOSEditorHandler.DrawProperty(scrollbar, customSkin, "Scrollbar");
                    if (getTimeData.boolValue == true) { DreamOSEditorHandler.DrawProperty(timeManager, customSkin, "Time Manager"); }
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(textColor, customSkin, "Text Color");
                    DreamOSEditorHandler.DrawProperty(timeColorCode, customSkin, "Time Color");
                    DreamOSEditorHandler.DrawPropertyCW(errorText, customSkin, "Error Feedback", -3);
                    DreamOSEditorHandler.DrawPropertyCW(onEnableText, customSkin, "On Enable Feedback", -3);
                    getTimeData.boolValue = DreamOSEditorHandler.DrawToggle(getTimeData.boolValue, customSkin, "Get Time Data");
                    if (getTimeData.boolValue == true && timeManager == null)
                        EditorGUILayout.HelpBox("'Get Time Data' is enabled but 'Time Manager' is not assigned.", MessageType.Warning);

                    antiFlicker.boolValue = DreamOSEditorHandler.DrawToggle(antiFlicker.boolValue, customSkin, "Anti Flicker");
                    useTypewriterEffect.boolValue = DreamOSEditorHandler.DrawToggle(useTypewriterEffect.boolValue, customSkin, "Use Typewriter Effect");
                    if (useTypewriterEffect.boolValue == true) { DreamOSEditorHandler.DrawProperty(typewriterDelay, customSkin, "Typewriter Delay"); }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif