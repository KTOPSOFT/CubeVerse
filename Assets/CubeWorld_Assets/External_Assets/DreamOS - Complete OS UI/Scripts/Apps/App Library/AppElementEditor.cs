//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(AppElement))]
    public class AppElementEditor : Editor
    {
        private AppElement aeTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            aeTarget = (AppElement)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "AE Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var appLibrary = serializedObject.FindProperty("appLibrary");
            var appID = serializedObject.FindProperty("appID");
            var elementType = serializedObject.FindProperty("elementType");
            var iconSize = serializedObject.FindProperty("iconSize");
            var useGradient = serializedObject.FindProperty("useGradient");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(appLibrary, customSkin, "App Library");
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 10);

                    if (aeTarget.appLibrary != null)
                    {
                        DreamOSEditorHandler.DrawProperty(appID, customSkin, "App ID");

                        if (aeTarget.tempAppIndex >= aeTarget.appLibrary.apps.Count
                            || aeTarget.appLibrary.apps[aeTarget.tempAppIndex].appTitle != aeTarget.appID)
                        {
                            EditorGUILayout.HelpBox("App ID cannot be found in the library.", MessageType.Error);

                            if (GUILayout.Button("Update"))
                            {
                                aeTarget.UpdateLibrary();
                                aeTarget.UpdateElement();
                            }
                        }

                        else
                        {
                            DreamOSEditorHandler.DrawProperty(elementType, customSkin, "Element Type");

                            if (elementType.enumValueIndex == 1)
                            {
                                DreamOSEditorHandler.DrawProperty(iconSize, customSkin, "Icon Size");
                                useGradient.boolValue = DreamOSEditorHandler.DrawToggle(useGradient.boolValue, customSkin, "Use Gradient");
                            }
                        }
                    }

                    else { EditorGUILayout.HelpBox("App Library should be assigned.", MessageType.Error); }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif