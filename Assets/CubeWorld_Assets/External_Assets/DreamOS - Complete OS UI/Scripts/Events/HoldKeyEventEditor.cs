#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(HoldKeyEvent))]
    public class HoldKeyEventEditor : Editor
    {
        private HoldKeyEvent hkeTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            hkeTarget = (HoldKeyEvent)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "HKE Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var hotkey = serializedObject.FindProperty("hotkey");
            var holdAction = serializedObject.FindProperty("holdAction");
            var releaseAction = serializedObject.FindProperty("releaseAction");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(hotkey, customSkin, "Hotkey");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(holdAction, new GUIContent("Hold Events"), true);
                    EditorGUILayout.PropertyField(releaseAction, new GUIContent("Release Events"), true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif