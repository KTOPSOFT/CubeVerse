#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WorldSpaceManager))]
    public class WorldSpaceManagerEditor : Editor
    {
        private WorldSpaceManager wsTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            wsTarget = (WorldSpaceManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "WS Top Header");

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

            var onEnter = serializedObject.FindProperty("onEnter");
            var onEnterEnd = serializedObject.FindProperty("onEnterEnd");
            var onExit = serializedObject.FindProperty("onExit");
            var onExitEnd = serializedObject.FindProperty("onExitEnd");     
            var playerTag = serializedObject.FindProperty("playerTag");
            var getInKey = serializedObject.FindProperty("getInKey");
            var getOutKey = serializedObject.FindProperty("getOutKey");
            var transitionSpeed = serializedObject.FindProperty("transitionSpeed");
            var requiresOpening = serializedObject.FindProperty("requiresOpening");
            var autoGetIn = serializedObject.FindProperty("autoGetIn");
            var lockCursorWhenOut = serializedObject.FindProperty("lockCursorWhenOut");
            var mainCamera = serializedObject.FindProperty("mainCamera");
            var enterMount = serializedObject.FindProperty("enterMount");
            var projectorCam = serializedObject.FindProperty("projectorCam");
            var osCanvas = serializedObject.FindProperty("osCanvas");
            var useFloatingIcon = serializedObject.FindProperty("useFloatingIcon");
            var transitionInTimer = serializedObject.FindProperty("transitionInTimer");
            var transitionOutTimer = serializedObject.FindProperty("transitionOutTimer");
            var transitionMode = serializedObject.FindProperty("transitionMode");
            var selectedTagIndex = serializedObject.FindProperty("selectedTagIndex");
            var dynamicRTSize = serializedObject.FindProperty("dynamicRTSize");
            var rtWidth = serializedObject.FindProperty("rtWidth");
            var rtHeight = serializedObject.FindProperty("rtHeight");
            var rendererImage = serializedObject.FindProperty("rendererImage");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Events Header", 6);
                    EditorGUILayout.PropertyField(onEnter, new GUIContent("On Enter Events"), true);
                    EditorGUILayout.PropertyField(onEnterEnd, new GUIContent("On Enter End Events"), true);
                    EditorGUILayout.PropertyField(onExit, new GUIContent("On Exit Events"), true);
                    EditorGUILayout.PropertyField(onExitEnd, new GUIContent("On Exit End Events"), true);
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(mainCamera, customSkin, "Main Camera");
                    DreamOSEditorHandler.DrawProperty(projectorCam, customSkin, "Projector Cam");
                    DreamOSEditorHandler.DrawProperty(rendererImage, customSkin, "Renderer Image");
                    DreamOSEditorHandler.DrawProperty(enterMount, customSkin, "Enter Mount");
                    DreamOSEditorHandler.DrawProperty(osCanvas, customSkin, "OS Canvas");
                    DreamOSEditorHandler.DrawProperty(useFloatingIcon, customSkin, "Floating Icon");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    requiresOpening.boolValue = DreamOSEditorHandler.DrawToggle(requiresOpening.boolValue, customSkin, "Require Opening At First");
                    autoGetIn.boolValue = DreamOSEditorHandler.DrawToggle(autoGetIn.boolValue, customSkin, "Auto Get In On Trigger");
                    lockCursorWhenOut.boolValue = DreamOSEditorHandler.DrawToggle(lockCursorWhenOut.boolValue, customSkin, "Lock Cursor On Out");
                    dynamicRTSize.boolValue = DreamOSEditorHandler.DrawToggle(dynamicRTSize.boolValue, customSkin, "Dynamic Render Texture Size");

                    if (dynamicRTSize.boolValue == false)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.LabelField(new GUIContent("Render Width/Height"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(rtWidth, new GUIContent(""));
                        EditorGUILayout.PropertyField(rtHeight, new GUIContent(""));
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Player Tag"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    selectedTagIndex.intValue = EditorGUILayout.Popup(selectedTagIndex.intValue, UnityEditorInternal.InternalEditorUtility.tags);
                    playerTag.stringValue = UnityEditorInternal.InternalEditorUtility.tags[selectedTagIndex.intValue].ToString();
                    GUILayout.EndHorizontal();
                    DreamOSEditorHandler.DrawProperty(getInKey, customSkin, "Get In Key");
                    DreamOSEditorHandler.DrawProperty(getOutKey, customSkin, "Get Out Key");
                    DreamOSEditorHandler.DrawProperty(transitionMode, customSkin, "Transition Mode");
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(new GUIContent("Trans. In/Out Timer"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(transitionInTimer, new GUIContent(""));
                    EditorGUILayout.PropertyField(transitionOutTimer, new GUIContent(""));
                    GUILayout.EndHorizontal();
                    DreamOSEditorHandler.DrawProperty(transitionSpeed, customSkin, "Transition Speed");

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif