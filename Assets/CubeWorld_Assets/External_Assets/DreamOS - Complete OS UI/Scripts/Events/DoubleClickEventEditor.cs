#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(DoubleClickEvent))]
    public class DoubleClickEventEditor : Editor
    {
        private DoubleClickEvent dkeTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            dkeTarget = (DoubleClickEvent)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "DCE Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var enableSingleClick = serializedObject.FindProperty("enableSingleClick");
            var timeFactor = serializedObject.FindProperty("timeFactor");
            var doubleClickEvents = serializedObject.FindProperty("doubleClickEvents");
            var singleClickEvents = serializedObject.FindProperty("singleClickEvents");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    DreamOSEditorHandler.DrawProperty(timeFactor, customSkin, "Time Factor");
                    enableSingleClick.boolValue = DreamOSEditorHandler.DrawToggle(enableSingleClick.boolValue, customSkin, "Enable Single Click");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(doubleClickEvents, new GUIContent("Double Click Events"), true);

                    if(enableSingleClick.boolValue == true)
                        EditorGUILayout.PropertyField(singleClickEvents, new GUIContent("Single Click Events"), true);

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif