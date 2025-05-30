using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MultiInstanceManager))]
    public class MultiInstanceManagerEditor : Editor
    {
        private GUISkin customSkin;
        private MultiInstanceManager mimTarget;
        private int currentTab;
        private bool createdComp;

        private void OnEnable()
        {
            mimTarget = (MultiInstanceManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "MIM Top Header");

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

            // Foldout style
            GUIStyle foldoutStyle = customSkin.FindStyle("UIM Foldout");

            var playerCamera = serializedObject.FindProperty("playerCamera");
            var manageProjectors = serializedObject.FindProperty("manageProjectors");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    for (int i = 0; i < mimTarget.instances.Count; i++)
                    {
                        // Draw Action Buttons
                        GUILayout.Space(6);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (mimTarget.instances[i].worldSpaceManager == null) { GUI.enabled = false; }
                        else { GUI.enabled = true; }

                        if (GUILayout.Button("Select", customSkin.button, GUILayout.Width(50)))
                        {
                            Selection.activeObject = mimTarget.instances[i].worldSpaceManager.transform.parent;
                        }

                        GUI.enabled = true;

                        if (GUILayout.Button("Delete", customSkin.button, GUILayout.Width(50)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Instance #" + i, "Are you sure you want to delete the instance? " +
                                "This will delete ALL of the instance resources/objects and cannot be undone.", "Yes", "Cancel"))
                            {
                                try
                                {
                                    DeleteInstance(i);

                                    for (int x = 0; x < mimTarget.instances.Count; x++)
                                    {
                                        if (mimTarget.instances[x] == null) continue;
                                        mimTarget.SetMachineID(x);
                                    }
                                }
                                catch { Debug.LogWarning("<b>[Multi Instance Manager]</b> Something went wrong while deleting the instance."); }
                            }
                        }

                        GUILayout.Space(6);
                        GUILayout.EndHorizontal();

                        // Start Item Background
                        GUILayout.Space(-30);
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        GUILayout.BeginVertical();
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        mimTarget.instances[i].isExpanded = EditorGUILayout.Foldout(mimTarget.instances[i].isExpanded, "Instance #" + i.ToString(), true, foldoutStyle);
                        mimTarget.instances[i].isExpanded = GUILayout.Toggle(mimTarget.instances[i].isExpanded, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
                        GUILayout.EndHorizontal();
                        GUILayout.Space(2);

                        if (mimTarget.instances[i].isExpanded)
                        {
                            // Core Resources
                            GUILayout.Label("Core Resources", customSkin.FindStyle("Text"), GUILayout.Width(140));

                            // World Space Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("World Space Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].worldSpaceManager = EditorGUILayout.ObjectField(mimTarget.instances[i].worldSpaceManager, typeof(WorldSpaceManager), true) as WorldSpaceManager;
                            GUILayout.EndHorizontal();

                            // Instance Canvas
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Instance Canvas", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].instanceCanvas = EditorGUILayout.ObjectField(mimTarget.instances[i].instanceCanvas, typeof(Canvas), true) as Canvas;
                            GUILayout.EndHorizontal();

                            if (mimTarget.instances[i].worldSpaceManager != null && mimTarget.instances[i].instanceCanvas)
                            {
                                if (GUILayout.Button("Initialize System Resources", customSkin.button))
                                {
                                    mimTarget.AutoWizard(i);
                                }
                            }

                            else
                            {
                                EditorGUILayout.HelpBox("World Space Manager and/or Instance Canvas is missing.", MessageType.Warning);
                                GUI.enabled = false;
                            }

                            // Manager Resources
                            GUILayout.Space(10);
                            GUILayout.Label("System Resources", customSkin.FindStyle("Text"), GUILayout.Width(140));

                            // User Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("User Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].userManager = EditorGUILayout.ObjectField(mimTarget.instances[i].userManager, typeof(UserManager), true) as UserManager;
                            GUILayout.EndHorizontal();

                            // Settings Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Settings Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].settingsManager = EditorGUILayout.ObjectField(mimTarget.instances[i].settingsManager, typeof(SettingsManager), true) as SettingsManager;
                            GUILayout.EndHorizontal();

                            // Reminder Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Reminder Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].reminderManager = EditorGUILayout.ObjectField(mimTarget.instances[i].reminderManager, typeof(ReminderManager), true) as ReminderManager;
                            GUILayout.EndHorizontal();

                            // Network Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Network Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].networkManager = EditorGUILayout.ObjectField(mimTarget.instances[i].networkManager, typeof(NetworkManager), true) as NetworkManager;
                            GUILayout.EndHorizontal();

                            // Wallpaper Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Wallpaper Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].wallpaperManager = EditorGUILayout.ObjectField(mimTarget.instances[i].wallpaperManager, typeof(WallpaperManager), true) as WallpaperManager;
                            GUILayout.EndHorizontal();

                            // Widget Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Widget Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].widgetManager = EditorGUILayout.ObjectField(mimTarget.instances[i].widgetManager, typeof(WidgetManager), true) as WidgetManager;
                            GUILayout.EndHorizontal();

                            // Web Browser Manager
                            GUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label("Web Browser Manager", customSkin.FindStyle("Text"), GUILayout.Width(140));
                            mimTarget.instances[i].webBrowserManager = EditorGUILayout.ObjectField(mimTarget.instances[i].webBrowserManager, typeof(WebBrowserManager), true) as WebBrowserManager;
                            GUILayout.EndHorizontal();

                            // *Sigh* temporary try/catch fix as target doesn't work as it should be on some Unity versions
                            // Get User Info
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("User Info items found: " + mimTarget.instances[i].userInfoItems.Count);
                            GUILayout.EndHorizontal();

                            // Switches
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Switches found: " + mimTarget.instances[i].switches.Count);
                            GUILayout.EndHorizontal();

                            // Music Data
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Music Data items found: " + mimTarget.instances[i].musicDataItems.Count);
                            GUILayout.EndHorizontal();

                            // Video Data
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Video Data items found: " + mimTarget.instances[i].videoDataItems.Count);
                            GUILayout.EndHorizontal();

                            GUI.enabled = true;
                            GUILayout.Space(4);
                        }

                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    if (mimTarget.instances.Count < 10)
                    {
                        if (mimTarget.instances.Count > 0 && GUILayout.Button("Initialize All", customSkin.button))
                        {
                            for (int i = 0; i < mimTarget.instances.Count; i++)
                            {
                                mimTarget.AutoWizard(i);
                            }
                        }

                        if (GUILayout.Button("+ Add a new instance item", customSkin.button))
                        {
                            MultiInstanceManager.InstanceItem item = new MultiInstanceManager.InstanceItem();
                            mimTarget.instances.Add(item);
                            EditorUtility.SetDirty(this);
                            return;
                        }
                    }

                    else { EditorGUILayout.HelpBox("You've reached to the max instance limit (10).", MessageType.Info); }
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(playerCamera, customSkin, "Player Camera");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    manageProjectors.boolValue = DreamOSEditorHandler.DrawToggle(manageProjectors.boolValue, customSkin, "Manage Projectors");
                    break;
            }

            GUILayout.Space(10);
            if (mimTarget.playerCamera == null) { EditorGUILayout.HelpBox("Player Camera is missing.", MessageType.Warning); }
            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }

        private void DeleteInstance(int index)
        {
            if (mimTarget.instances[index].worldSpaceManager == null) { mimTarget.instances.RemoveAt(index); }
            else if (mimTarget.instances[index].worldSpaceManager.transform.parent.gameObject != null)
            {
                DestroyImmediate(mimTarget.instances[index].worldSpaceManager.transform.parent.gameObject);
                mimTarget.instances.RemoveAt(index);
            }

            // Undo.RecordObject(this, "Removed DreamOS instance");
            EditorSceneManager.MarkSceneDirty(mimTarget.gameObject.scene);
        }
    }
}
#endif