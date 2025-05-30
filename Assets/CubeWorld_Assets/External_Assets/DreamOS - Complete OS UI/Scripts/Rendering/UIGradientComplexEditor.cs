#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Michsky.DreamOS;

namespace UnityEngine.UI
{
    namespace Michsky.DreamOS
    {
        [CustomEditor(typeof(UIGradientComplex))]
        public class UIGradientComplexEditor : Editor
        {
            private int currentTab;
            private GUISkin customSkin;

            private void OnEnable()
            {
                if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
                else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
            }

            public override void OnInspectorGUI()
            {
                DreamOSEditorHandler.DrawComponentHeader(customSkin, "Gradient Top Header");

                GUIContent[] toolbarTabs = new GUIContent[1];
                toolbarTabs[0] = new GUIContent("Settings");

                currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

                if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                    currentTab = 0;

                GUILayout.EndHorizontal();

                var _effectGradient = serializedObject.FindProperty("_effectGradient");
                var gradientType = serializedObject.FindProperty("gradientType");
                var offset = serializedObject.FindProperty("offset");
                var zoom = serializedObject.FindProperty("zoom");
                var complexGradient = serializedObject.FindProperty("complexGradient");

                switch (currentTab)
                {
                    case 0:
                        DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                        DreamOSEditorHandler.DrawProperty(_effectGradient, customSkin, "Gradient");
                        DreamOSEditorHandler.DrawProperty(gradientType, customSkin, "Type");
                        DreamOSEditorHandler.DrawProperty(offset, customSkin, "Offset");
                        DreamOSEditorHandler.DrawProperty(zoom, customSkin, "Zoom");
                        complexGradient.boolValue = DreamOSEditorHandler.DrawToggle(complexGradient.boolValue, customSkin, "Complex Gradient");
                        break;              
                }

                serializedObject.ApplyModifiedProperties();
                this.Repaint();
            }
        }
    }
}
#endif