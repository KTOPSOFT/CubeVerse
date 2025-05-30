#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(ModalWindowManager))]
    public class ModalWindowManagerEditor : Editor
    {
        private ModalWindowManager mwTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            mwTarget = (ModalWindowManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "MW Top Header");

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

            var windowIcon = serializedObject.FindProperty("windowIcon");
            var titleText = serializedObject.FindProperty("titleText");
            var descriptionText = serializedObject.FindProperty("descriptionText");
            var mwAnimator = serializedObject.FindProperty("mwAnimator");
            var iconImage = serializedObject.FindProperty("iconImage");
            var windowTitle = serializedObject.FindProperty("windowTitle");
            var windowDescription = serializedObject.FindProperty("windowDescription");
            var confirmButton = serializedObject.FindProperty("confirmButton");
            var cancelButton = serializedObject.FindProperty("cancelButton");
            var blurManager = serializedObject.FindProperty("blurManager");
            var useBlur = serializedObject.FindProperty("useBlur");
            var useCustomValues = serializedObject.FindProperty("useCustomValues");
            var onConfirm = serializedObject.FindProperty("onConfirm");
            var onCancel = serializedObject.FindProperty("onCancel");
            var closeBehaviour = serializedObject.FindProperty("closeBehaviour");
            var startBehaviour = serializedObject.FindProperty("startBehaviour");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    if (mwTarget.useCustomValues == false)
                    {
                        DreamOSEditorHandler.DrawProperty(windowIcon, customSkin, "Icon");

                        if (mwTarget.iconImage != null) { mwTarget.iconImage.sprite = mwTarget.windowIcon; }
                        else
                        {
                            if (mwTarget.iconImage == null)
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.HelpBox("'Icon Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                                GUILayout.EndHorizontal();
                            }
                        }

                        DreamOSEditorHandler.DrawProperty(titleText, customSkin, "Title");

                        if (mwTarget.windowTitle != null) { mwTarget.windowTitle.text = titleText.stringValue; }
                        else
                        {
                            if (mwTarget.windowTitle == null)
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.HelpBox("'Title Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                                GUILayout.EndHorizontal();
                            }
                        }

                        DreamOSEditorHandler.DrawPropertyCW(descriptionText, customSkin, "Description", -3);

                        if (mwTarget.windowDescription != null) { mwTarget.windowDescription.text = descriptionText.stringValue; }
                        else
                        {
                            if (mwTarget.windowDescription == null)
                            {
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.HelpBox("'Description Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }

                    else { EditorGUILayout.HelpBox("'Use Custom Content' is enabled.", MessageType.Info); }

                    if (mwTarget.GetComponent<CanvasGroup>().alpha == 0)
                    {
                        if (GUILayout.Button("Make It Visible", customSkin.button))
                            mwTarget.GetComponent<CanvasGroup>().alpha = 1;
                    }

                    else
                    {
                        if (GUILayout.Button("Make It Invisible", customSkin.button))
                            mwTarget.GetComponent<CanvasGroup>().alpha = 0;
                    }

                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onConfirm, new GUIContent("On Confirm"), true);
                    EditorGUILayout.PropertyField(onCancel, new GUIContent("On Cancel"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(iconImage, customSkin, "Icon Object");
                    DreamOSEditorHandler.DrawProperty(windowTitle, customSkin, "Title Object");
                    DreamOSEditorHandler.DrawProperty(windowDescription, customSkin, "Description Object");
                    DreamOSEditorHandler.DrawProperty(confirmButton, customSkin, "Confirm Button");
                    DreamOSEditorHandler.DrawProperty(cancelButton, customSkin, "Cancel Button");
                    DreamOSEditorHandler.DrawProperty(mwAnimator, customSkin, "Animator");

                    if (mwTarget.useBlur == true)
                        DreamOSEditorHandler.DrawProperty(blurManager, customSkin, "Blur Manager");

                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    useCustomValues.boolValue = DreamOSEditorHandler.DrawToggle(useCustomValues.boolValue, customSkin, "Use Custom Content");
                    useBlur.boolValue = DreamOSEditorHandler.DrawToggle(useBlur.boolValue, customSkin, "Use Blur Background");
                    DreamOSEditorHandler.DrawProperty(startBehaviour, customSkin, "Start Behaviour");
                    DreamOSEditorHandler.DrawProperty(closeBehaviour, customSkin, "Close Behaviour");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif