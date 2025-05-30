#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(CalculatorManager))]
    public class CalculatorManagerEditor : Editor
    {
        private CalculatorManager cTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            cTarget = (CalculatorManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Calculator Top Header");

            GUIContent[] toolbarTabs = new GUIContent[1];
            toolbarTabs[0] = new GUIContent("Resources");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;

            GUILayout.EndHorizontal();

            var displayText = serializedObject.FindProperty("displayText");
            var displayOperator = serializedObject.FindProperty("displayOperator");
            var displayResult = serializedObject.FindProperty("displayResult");
            var displayPreview = serializedObject.FindProperty("displayPreview");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(displayText, customSkin, "Display Text");
                    DreamOSEditorHandler.DrawProperty(displayOperator, customSkin, "Display Operator");
                    DreamOSEditorHandler.DrawProperty(displayResult, customSkin, "Display Result");
                    DreamOSEditorHandler.DrawProperty(displayPreview, customSkin, "Display Preview");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif