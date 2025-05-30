#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(NotepadManager))]
    public class NotepadManagerEditor : Editor
    {
        private NotepadManager notepadTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            notepadTarget = (NotepadManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Notepad Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Resources");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var libraryAsset = serializedObject.FindProperty("libraryAsset");
            var noteLibraryParent = serializedObject.FindProperty("noteLibraryParent");
            var noteLibraryButton = serializedObject.FindProperty("noteLibraryButton");
            var notepadWindow = serializedObject.FindProperty("notepadWindow");
            var viewerAnimator = serializedObject.FindProperty("viewerAnimator");
            var viewerTitle = serializedObject.FindProperty("viewerTitle");
            var viewerContent = serializedObject.FindProperty("viewerContent");
            var deleteButton = serializedObject.FindProperty("deleteButton");
            var sortListByName = serializedObject.FindProperty("sortListByName");
            var notepadStoring = serializedObject.FindProperty("notepadStoring");
            var saveCustomNotes = serializedObject.FindProperty("saveCustomNotes");

            switch (currentTab)
            {             
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(libraryAsset, customSkin, "Library Asset");
                    DreamOSEditorHandler.DrawProperty(noteLibraryParent, customSkin, "Library Parent");
                    DreamOSEditorHandler.DrawProperty(notepadWindow, customSkin, "Window");
                    DreamOSEditorHandler.DrawProperty(viewerAnimator, customSkin, "Viewer Animator");
                    DreamOSEditorHandler.DrawProperty(viewerTitle, customSkin, "Viewer Title");
                    DreamOSEditorHandler.DrawProperty(viewerContent, customSkin, "Viewer Content");
                    DreamOSEditorHandler.DrawProperty(deleteButton, customSkin, "Delete Button");
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    sortListByName.boolValue = DreamOSEditorHandler.DrawToggle(sortListByName.boolValue, customSkin, "Sort Note List By Name");
                    saveCustomNotes.boolValue = DreamOSEditorHandler.DrawToggle(saveCustomNotes.boolValue, customSkin, "Save Custom Notes");

                    if (saveCustomNotes.boolValue == true)
                    {
                        DreamOSEditorHandler.DrawProperty(notepadStoring, customSkin, "Notepad Storing");

                        if (notepadTarget.notepadStoring == null)
                        {
                            EditorGUILayout.HelpBox("'Save Custom Notes' is enabled but 'Notepad Storing' is not assigned. " +
                                "Please add and/or assign 'Notepad Storing' component.", MessageType.Error);

                            if (GUILayout.Button("+  Create Notepad Storing", customSkin.button))
                            {
                                NotepadStoring tempNS = notepadTarget.gameObject.AddComponent<NotepadStoring>();
                                notepadTarget.notepadStoring = tempNS;
                                tempNS.notepadManager = notepadTarget;

                                PrefabUtility.RecordPrefabInstancePropertyModifications(notepadTarget);
                                Undo.RecordObject(this, "Created notepad storing");
                                EditorUtility.SetDirty(this);
                                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(notepadTarget.gameObject.scene);
                            }
                        }
                    }

                    break;            
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif