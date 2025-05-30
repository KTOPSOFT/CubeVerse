#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.DreamOS
{
    [CustomEditor(typeof(WallpaperManager))]
    public class WallpaperManagerEditor : Editor
    {
        private WallpaperManager wmTarget;
        private GUISkin customSkin;
        private int currentTab;

        private void OnEnable()
        {
            wmTarget = (WallpaperManager)target;

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\Glass Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            Color defaultColor = GUI.color;
            DreamOSEditorHandler.DrawComponentHeader(customSkin, "Wallpaper Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Settings");
            toolbarTabs[1] = new GUIContent("Resources");

            currentTab = DreamOSEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 0;

            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var wallpaperLibrary = serializedObject.FindProperty("wallpaperLibrary");
            var desktopSource = serializedObject.FindProperty("desktopSource");
            var lockScreenSource = serializedObject.FindProperty("lockScreenSource");
            var wallpaperParent = serializedObject.FindProperty("wallpaperParent");
            var wallpaperItem = serializedObject.FindProperty("wallpaperItem");
            var wallpaperIndex = serializedObject.FindProperty("wallpaperIndex");
            var saveSelected = serializedObject.FindProperty("saveSelected");

            switch (currentTab)
            {
                case 0:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    DreamOSEditorHandler.DrawProperty(wallpaperLibrary, customSkin, "Wallpaper Library");    
                    saveSelected.boolValue = DreamOSEditorHandler.DrawToggle(saveSelected.boolValue, customSkin, "Save Selected");

                    if (wmTarget.wallpaperLibrary != null)
                    {
                        if (wmTarget.wallpaperLibrary.wallpapers.Count != 0)
                        {
                            GUILayout.Space(-2);
                            GUILayout.BeginHorizontal();      
                            GUILayout.FlexibleSpace();
                            GUI.backgroundColor = Color.clear;

                            GUILayout.Box(TextureFromSprite(wmTarget.wallpaperLibrary.wallpapers[wallpaperIndex.intValue].wallpaperSprite), GUILayout.Width(245), GUILayout.Height(140));

                            GUI.backgroundColor = defaultColor;
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            GUILayout.Space(-2);
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(2);

                            GUI.enabled = false;
                            EditorGUILayout.LabelField(new GUIContent("Selected Wallpaper:"), customSkin.FindStyle("Text"), GUILayout.Width(112));
                            GUI.enabled = true;
                            EditorGUILayout.LabelField(new GUIContent(wmTarget.wallpaperLibrary.wallpapers[wallpaperIndex.intValue].wallpaperID), customSkin.FindStyle("Text"), GUILayout.Width(112));

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(2);

                            wallpaperIndex.intValue = EditorGUILayout.IntSlider(wallpaperIndex.intValue, 0, wmTarget.wallpaperLibrary.wallpapers.Count - 1);

                            GUILayout.Space(2);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(2);

                            if (wmTarget.desktopSource != null && GUILayout.Button("Update Desktop Source"))
                            {
                                wmTarget.desktopSource.sprite = wmTarget.wallpaperLibrary.wallpapers[wallpaperIndex.intValue].wallpaperSprite;
                                wmTarget.enabled = false;
                                wmTarget.enabled = true;
                            }

                            GUILayout.Space(2);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        else { EditorGUILayout.HelpBox("There is no item in the library.", MessageType.Warning); }
                    }

                    else { EditorGUILayout.HelpBox("Wallpaper Library is missing.", MessageType.Error); }

                    GUILayout.EndVertical();
                    break;

                case 1:
                    DreamOSEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    DreamOSEditorHandler.DrawProperty(desktopSource, customSkin, "Desktop Source");
                    DreamOSEditorHandler.DrawProperty(lockScreenSource, customSkin, "Lock Screen Source");
                    DreamOSEditorHandler.DrawProperty(wallpaperParent, customSkin, "Wallpaper Parent");
                    DreamOSEditorHandler.DrawProperty(wallpaperItem, customSkin, "Wallpaper Item");
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            this.Repaint();
        }

        public static Texture2D TextureFromSprite(Sprite sprite)
        {
            if (sprite == null) { return null; }

            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }

            else { return sprite.texture; }
        }
    }
}
#endif