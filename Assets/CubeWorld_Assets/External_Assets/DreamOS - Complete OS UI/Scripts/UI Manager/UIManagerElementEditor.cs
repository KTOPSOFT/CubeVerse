#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(UIManagerElement))]
    public class UIManagerElementEditor : Editor
    {
        private UIManagerElement tmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            tmTarget = (UIManagerElement)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "TM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var themeManagerAsset = serializedObject.FindProperty("themeManagerAsset");
            var objectType = serializedObject.FindProperty("objectType");
            var colorType = serializedObject.FindProperty("colorType");
            var fontType = serializedObject.FindProperty("fontType");
            var keepAlphaValue = serializedObject.FindProperty("keepAlphaValue");
            var useCustomFont = serializedObject.FindProperty("useCustomFont");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(themeManagerAsset, customSkin, "UI Manager");

                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 10);

                    if (tmTarget.themeManagerAsset != null)
                    {
                        DreamOSEditorHandler.DrawProperty(objectType, customSkin, "Object Type");
                        DreamOSEditorHandler.DrawProperty(colorType, customSkin, "Color Type");

                        if (tmTarget.objectType == UIManagerElement.ObjectType.Text)
                        {
                            DreamOSEditorHandler.DrawProperty(fontType, customSkin, "Font Type");
                            useCustomFont.boolValue = DreamOSEditorHandler.DrawToggle(useCustomFont.boolValue, customSkin, "Use Custom Font");
                        }

                        keepAlphaValue.boolValue = DreamOSEditorHandler.DrawToggle(keepAlphaValue.boolValue, customSkin, "Keep Alpha Value");
                    }

                    else { EditorGUILayout.HelpBox("Theme Manager should be assigned.", MessageType.Error); }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif