//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(MusicPlayerManager))]
    public class MusicPlayerManagerEditor : Editor
    {
        private MusicPlayerManager mpTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            mpTarget = (MusicPlayerManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "MP Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Playlists", "Playlists"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            var libraryPlaylist = serializedObject.FindProperty("libraryPlaylist");
            var currentPlaylist = serializedObject.FindProperty("currentPlaylist");
            var modPlaylist = serializedObject.FindProperty("modPlaylist");
            var playlists = serializedObject.FindProperty("playlists");
            var musicLibraryParent = serializedObject.FindProperty("musicLibraryParent");
            var musicLibraryButton = serializedObject.FindProperty("musicLibraryButton");
            var musicPanelManager = serializedObject.FindProperty("musicPanelManager");
            var nowPlayingListTitle = serializedObject.FindProperty("nowPlayingListTitle");
            var playlistParent = serializedObject.FindProperty("playlistParent");
            var playlistContentParent = serializedObject.FindProperty("playlistContentParent");
            var playlistButton = serializedObject.FindProperty("playlistButton");
            var playlistTitle = serializedObject.FindProperty("playlistTitle");
            var playlistDescription = serializedObject.FindProperty("playlistDescription");
            var playlistCover = serializedObject.FindProperty("playlistCover");
            var playlistCoverBanner = serializedObject.FindProperty("playlistCoverBanner");
            var playlistPlayAllButton = serializedObject.FindProperty("playlistPlayAllButton");
            var notificationCreator = serializedObject.FindProperty("notificationCreator");
            var repeat = serializedObject.FindProperty("repeat");
            var shuffle = serializedObject.FindProperty("shuffle");
            var sortListByName = serializedObject.FindProperty("sortListByName");
            var enablePopupNotification = serializedObject.FindProperty("enablePopupNotification");
            var playlistSingularLabel = serializedObject.FindProperty("playlistSingularLabel");
            var playlistPluralLabel = serializedObject.FindProperty("playlistPluralLabel");
            var lyricsManager = serializedObject.FindProperty("lyricsManager");
            var enableLyrics = serializedObject.FindProperty("enableLyrics");
            var source = serializedObject.FindProperty("source");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    DreamOSEditorHandler.DrawProperty(libraryPlaylist, customSkin, "Library Playlist");
                    DreamOSEditorHandler.DrawProperty(currentPlaylist, customSkin, "Default Playlist");
                    DreamOSEditorHandler.DrawProperty(modPlaylist, customSkin, "Mod Playlist");

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.PropertyField(playlists, new GUIContent("Playlists"), true);

                    EditorGUI.indentLevel = 0;
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("+  Add a new playlist", customSkin.button))
                        mpTarget.AddPlaylist();

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(source, customSkin, "Audio Source");
                    DreamOSEditorHandler.DrawProperty(musicLibraryParent, customSkin, "Library Parent");
                    DreamOSEditorHandler.DrawProperty(musicLibraryButton, customSkin, "Library Button");
                    DreamOSEditorHandler.DrawProperty(musicPanelManager, customSkin, "Panel Manager");
                    DreamOSEditorHandler.DrawProperty(nowPlayingListTitle, customSkin, "Now Playing Title");
                    DreamOSEditorHandler.DrawProperty(playlistParent, customSkin, "Playlist Parent");
                    DreamOSEditorHandler.DrawProperty(playlistContentParent, customSkin, "Playlist Content Parent");
                    DreamOSEditorHandler.DrawProperty(playlistButton, customSkin, "Playlist Button");
                    DreamOSEditorHandler.DrawProperty(playlistTitle, customSkin, "Playlist Title");
                    DreamOSEditorHandler.DrawProperty(playlistDescription, customSkin, "Playlist Description");
                    DreamOSEditorHandler.DrawProperty(playlistCover, customSkin, "Playlist Cover");
                    DreamOSEditorHandler.DrawProperty(playlistCoverBanner, customSkin, "Playlist Cover Banner");
                    DreamOSEditorHandler.DrawProperty(playlistPlayAllButton, customSkin, "Playlist Play Button");
                    DreamOSEditorHandler.DrawProperty(notificationCreator, customSkin, "Notification Creator");
                    DreamOSEditorHandler.DrawProperty(lyricsManager, customSkin, "Lyrics Manager");
                    break;

                case 2:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    repeat.boolValue = DreamOSEditorHandler.DrawToggle(repeat.boolValue, customSkin, "Repeat");
                    shuffle.boolValue = DreamOSEditorHandler.DrawToggle(shuffle.boolValue, customSkin, "Shuffle");
                    sortListByName.boolValue = DreamOSEditorHandler.DrawToggle(sortListByName.boolValue, customSkin, "Sort List By Name");
                    enableLyrics.boolValue = DreamOSEditorHandler.DrawToggle(enableLyrics.boolValue, customSkin, "Enable Lyrics");
                    enablePopupNotification.boolValue = DreamOSEditorHandler.DrawToggle(enablePopupNotification.boolValue, customSkin, "Enable Popup Notification");       

                    if (enablePopupNotification.boolValue == true && notificationCreator == null)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.HelpBox("'Notification Creator' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Warning);
                        GUILayout.EndHorizontal();
                    }

                    DreamOSEditorHandler.DrawProperty(playlistSingularLabel, customSkin, "Singular Label");
                    DreamOSEditorHandler.DrawProperty(playlistPluralLabel, customSkin, "Plural Label");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }
    }
}
#endif