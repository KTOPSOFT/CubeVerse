#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(HorizontalSelector))]
    public class HorizontalSelectorEditor : Editor
    {
        private HorizontalSelector hsTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            hsTarget = (HorizontalSelector)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "HS Top Header");

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

            var label = serializedObject.FindProperty("label");
            var labelHelper = serializedObject.FindProperty("labelHelper");
            var indicatorParent = serializedObject.FindProperty("indicatorParent");
            var indicatorObject = serializedObject.FindProperty("indicatorObject");
            var selectorAnimator = serializedObject.FindProperty("selectorAnimator");
            var saveValue = serializedObject.FindProperty("saveValue");
            var selectorTag = serializedObject.FindProperty("selectorTag");
            var enableIndicators = serializedObject.FindProperty("enableIndicators");
            var selectorEvent = serializedObject.FindProperty("selectorEvent");
            var itemList = serializedObject.FindProperty("itemList");
            var invokeAtStart = serializedObject.FindProperty("invokeAtStart");
            var invertAnimation = serializedObject.FindProperty("invertAnimation");
            var loopSelection = serializedObject.FindProperty("loopSelection");
            var defaultIndex = serializedObject.FindProperty("defaultIndex");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    if (hsTarget.itemList.Count != 0)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Selected Item Index:"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        defaultIndex.intValue = EditorGUILayout.IntSlider(defaultIndex.intValue, 0, hsTarget.itemList.Count - 1);

                        GUILayout.Space(2);
                        EditorGUILayout.LabelField(new GUIContent(hsTarget.itemList[defaultIndex.intValue].itemTitle), customSkin.FindStyle("Text"));
                        GUILayout.EndVertical();

                        if (saveValue.boolValue == true)
                            EditorGUILayout.HelpBox("Save Selection is enabled. This option won't be used if there's a stored value.", MessageType.Info);
                    }

                    else { EditorGUILayout.HelpBox("There is no item in the list.", MessageType.Warning); }

                    if (Application.isPlaying == true)
                    {
                        GUI.enabled = false;
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Current Index"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.IntSlider(hsTarget.index, 0, hsTarget.itemList.Count - 1);

                        GUILayout.Space(2);
                        GUILayout.EndHorizontal();
                        GUI.enabled = true;
                    }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                 
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(itemList, new GUIContent("Selector Items"), true);
                    EditorGUI.indentLevel = 1;
                  
                    if (GUILayout.Button("+  Add a new item", customSkin.button))
                        hsTarget.AddNewItem();

                    GUILayout.EndVertical();

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(selectorEvent, new GUIContent("Selector Event"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(label, customSkin, "Label");
                    DreamOSEditorHandler.DrawProperty(labelHelper, customSkin, "Label Helper");
                    DreamOSEditorHandler.DrawProperty(indicatorParent, customSkin, "Indicator Parent");
                    DreamOSEditorHandler.DrawProperty(indicatorObject, customSkin, "Indicator Object");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    enableIndicators.boolValue = DreamOSEditorHandler.DrawToggle(enableIndicators.boolValue, customSkin, "Enable Indicators");

                    GUILayout.BeginHorizontal();

                    if (enableIndicators.boolValue == true)
                    {
                        if (hsTarget.indicatorObject == null)
                            EditorGUILayout.HelpBox("'Indicator Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);

                        if (hsTarget.indicatorParent == null)
                            EditorGUILayout.HelpBox("'Indicator Parent' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                        else
                            hsTarget.indicatorParent.gameObject.SetActive(true);
                    }

                    else
                    {
                        if (hsTarget.indicatorParent != null)
                            hsTarget.indicatorParent.gameObject.SetActive(false);
                    }

                    GUILayout.EndHorizontal();

                    invokeAtStart.boolValue = DreamOSEditorHandler.DrawToggle(invokeAtStart.boolValue, customSkin, "Invoke On Awake");
                    invertAnimation.boolValue = DreamOSEditorHandler.DrawToggle(invertAnimation.boolValue, customSkin, "Invert Animation");
                    loopSelection.boolValue = DreamOSEditorHandler.DrawToggle(loopSelection.boolValue, customSkin, "Loop Selection");
                    saveValue.boolValue = DreamOSEditorHandler.DrawToggle(saveValue.boolValue, customSkin, "Save Selection");

                    if (saveValue.boolValue == true)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(35);

                        EditorGUILayout.LabelField(new GUIContent("Tag:"), customSkin.FindStyle("Text"), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(selectorTag, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Each selector should has its own unique tag.", MessageType.Info);
                    }

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif