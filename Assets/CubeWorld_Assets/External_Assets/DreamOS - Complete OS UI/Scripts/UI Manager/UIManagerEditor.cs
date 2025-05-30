#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(UIManager))]
    [System.Serializable]
    public class UIManagerEditor : Editor
    {
        GUISkin customSkin;
        protected static string buildID = "B16-20220524";
        protected static float foldoutItemSpace = 2;
        protected static float foldoutTopSpace = 5;
        protected static float foldoutBottomSpace = 2;

        protected static bool showColors = false;
        protected static bool showFonts = false;

        void OnEnable()
        {
            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            if (customSkin == null)
            {
                EditorGUILayout.HelpBox("Editor variables are missing. You can manually fix this by deleting " +
                    "DreamOS > Resources > Editor folder and then re-import the package. \n\nIf you're still seeing this " +
                    "dialog even after the re-import, contact me with this ID: " + buildID, MessageType.Error);

                if (GUILayout.Button("Contact")) { Email(); }
                return;
            }

            // Foldout style
            GUIStyle foldoutStyle = customSkin.FindStyle("UIM Foldout");

            // UIM Header
            DreamOSEditorHandler.DrawHeader(customSkin, "UIM Header", 8);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Colors
            var windowBGColorDark = serializedObject.FindProperty("windowBGColorDark");
            var backgroundColorDark = serializedObject.FindProperty("backgroundColorDark");
            var primaryColorDark = serializedObject.FindProperty("primaryColorDark");
            var secondaryColorDark = serializedObject.FindProperty("secondaryColorDark");
            var highlightedColorDark = serializedObject.FindProperty("highlightedColorDark");
            var highlightedColorSecondaryDark = serializedObject.FindProperty("highlightedColorSecondaryDark");
            var taskBarColorDark = serializedObject.FindProperty("taskBarColorDark");
            var highlightedColorCustom = serializedObject.FindProperty("highlightedColorCustom");
            var highlightedColorSecondaryCustom = serializedObject.FindProperty("highlightedColorSecondaryCustom");

            GUILayout.Space(foldoutTopSpace);
            GUILayout.BeginHorizontal();
            showColors = EditorGUILayout.Foldout(showColors, "Theme Colors", true, foldoutStyle);
            showColors = GUILayout.Toggle(showColors, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
            GUILayout.EndHorizontal();
            GUILayout.Space(foldoutBottomSpace);

            if (showColors)
            {
                GUILayout.Label("Default Theme", EditorStyles.boldLabel);
                DreamOSEditorHandler.DrawProperty(highlightedColorDark, customSkin, "Accent Color");
                DreamOSEditorHandler.DrawProperty(highlightedColorSecondaryDark, customSkin, "Accent Reversed");
                DreamOSEditorHandler.DrawProperty(primaryColorDark, customSkin, "Primary Color");
                DreamOSEditorHandler.DrawProperty(secondaryColorDark, customSkin, "Secondary Color");
                DreamOSEditorHandler.DrawProperty(windowBGColorDark, customSkin, "Window BG Color");
                DreamOSEditorHandler.DrawProperty(backgroundColorDark, customSkin, "Background Color");
                DreamOSEditorHandler.DrawProperty(taskBarColorDark, customSkin, "Task Bar Color");

                GUILayout.Space(12);
                GUILayout.Label("Custom Theme", EditorStyles.boldLabel);
                DreamOSEditorHandler.DrawProperty(highlightedColorCustom, customSkin, "Accent Color");
                DreamOSEditorHandler.DrawProperty(highlightedColorSecondaryCustom, customSkin, "Accent Reversed");
            }

            GUILayout.EndVertical();
            GUILayout.Space(foldoutItemSpace);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Fonts
            var systemFontThin = serializedObject.FindProperty("systemFontThin");
            var systemFontLight = serializedObject.FindProperty("systemFontLight");
            var systemFontRegular = serializedObject.FindProperty("systemFontRegular");
            var systemFontSemiBold = serializedObject.FindProperty("systemFontSemiBold");
            var systemFontBold = serializedObject.FindProperty("systemFontBold");

            GUILayout.Space(foldoutTopSpace);
            GUILayout.BeginHorizontal();
            showFonts = EditorGUILayout.Foldout(showFonts, "Fonts", true, foldoutStyle);
            showFonts = GUILayout.Toggle(showFonts, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
            GUILayout.EndHorizontal();
            GUILayout.Space(foldoutBottomSpace);

            if (showFonts)
            {
                DreamOSEditorHandler.DrawProperty(systemFontThin, customSkin, "Font Thin");
                DreamOSEditorHandler.DrawProperty(systemFontLight, customSkin, "Font Light");
                DreamOSEditorHandler.DrawProperty(systemFontRegular, customSkin, "Font Regular");
                DreamOSEditorHandler.DrawProperty(systemFontSemiBold, customSkin, "Font Semibold");
                DreamOSEditorHandler.DrawProperty(systemFontBold, customSkin, "Font Bold");
            }

            // Settings
            GUILayout.EndVertical();
            DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 14);

            var enableDynamicUpdate = serializedObject.FindProperty("enableDynamicUpdate");
            enableDynamicUpdate.boolValue = DreamOSEditorHandler.DrawToggle(enableDynamicUpdate.boolValue, customSkin, "Update Values");

            var enableExtendedColorPicker = serializedObject.FindProperty("enableExtendedColorPicker");
            enableExtendedColorPicker.boolValue = DreamOSEditorHandler.DrawToggle(enableExtendedColorPicker.boolValue, customSkin, "Extended Color Picker");

            if (enableExtendedColorPicker.boolValue == true) { EditorPrefs.SetInt("UIManager.EnableExtendedColorPicker", 1); }
            else { EditorPrefs.SetInt("UIManager.EnableExtendedColorPicker", 0); }

            var editorHints = serializedObject.FindProperty("editorHints");
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(-3);
            editorHints.boolValue = DreamOSEditorHandler.DrawTogglePlain(editorHints.boolValue, customSkin, "UI Manager Hints");
            GUILayout.Space(3);

            if (editorHints.boolValue == true)
            {
                EditorGUILayout.HelpBox("These values are universal and affect all objects containing the 'UI Manager' component.", MessageType.Info);
                EditorGUILayout.HelpBox("If want to assign unique values, remove the 'UI Manager' component from the object.", MessageType.Info);
            }

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            this.Repaint();

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset to defaults", customSkin.button))
                ResetToDefaults();

            GUILayout.EndHorizontal();

            // Add-ons
            GUILayout.Space(14);
            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Add-on Header"));
            GUILayout.BeginVertical();
            if (GUILayout.Button("Steam Messaging", customSkin.button)) { Application.OpenURL("https://u3d.as/2QTF"); }
            GUILayout.EndVertical();

            // Support
            GUILayout.Space(14);
            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Support Header"));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Need help? Contact me via:", customSkin.FindStyle("Text"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Discord", customSkin.button)) { Discord(); }
            if (GUILayout.Button("Twitter", customSkin.button)) { Twitter(); }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("E-mail", customSkin.button)) { Email(); }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("ID: " + buildID);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        void Discord() { Application.OpenURL("https://discord.gg/VXpHyUt"); }
        void Email() { Application.OpenURL("https://www.michsky.com/contact/"); }
        void Twitter() { Application.OpenURL("https://www.twitter.com/michskyHQ"); }

        void ResetToDefaults()
        {
            if (EditorUtility.DisplayDialog("Reset to defaults", "Are you sure you want to reset UI Manager values to default?", "Yes", "Cancel"))
            {
                try
                {
                    if (EditorPrefs.HasKey("DreamOS.PipelineUpgrader"))
                    {
                        Preset defaultPreset = Resources.Load<Preset>("UI Manager/Presets/SRP Default");
                        defaultPreset.ApplyTo(Resources.Load("UI Manager/UI Manager"));
                    }

                    else
                    {
                        Preset defaultPreset = Resources.Load<Preset>("UI Manager/Presets/Default");
                        defaultPreset.ApplyTo(Resources.Load("UI Manager/UI Manager"));
                    }

                    Selection.activeObject = null;
                    Debug.Log("<b>[UI Manager]</b> Resetting successful.");
                }

                catch { Debug.LogWarning("<b>[UI Manager]</b> Resetting failed."); }
            }
        }
    }
}
#endif