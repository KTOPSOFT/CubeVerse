//This asset was uploaded by https://unityassetcollection.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Michsky.DreamOS
{
    public class CreateMenu : Editor
    {
        static string objectPath;

        #region Methods & Helpers
        static void GetObjectPath()
        {
            objectPath = AssetDatabase.GetAssetPath(Resources.Load("UI Manager/UI Manager"));
            objectPath = objectPath.Replace("Resources/UI Manager/UI Manager.asset", "").Trim();
            objectPath = objectPath + "Prefabs/";
        }

        static void MakeSceneDirty(GameObject source, string sourceName)
        {
            if (Application.isPlaying == false)
            {
                Undo.RegisterCreatedObjectUndo(source, sourceName);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        static void ShowErrorDialog()
        {
            EditorUtility.DisplayDialog("DreamOS", "Cannot create the object due to missing manager file. " +
                    "Make sure you have 'UI Manager' file in DreamOS > Resources > UI Manager folder.", "Okay");
        }

        static void UpdateCustomEditorPath()
        {
            string mainPath = AssetDatabase.GetAssetPath(Resources.Load("UI Manager/UI Manager"));
            mainPath = mainPath.Replace("Resources/UI Manager/UI Manager.asset", "").Trim();
            string darkPath = mainPath + "Editor/Glass Skin Dark.guiskin";
            string lightPath = mainPath + "Editor/Glass Skin Light.guiskin";

            EditorPrefs.SetString("DreamOS.CustomEditorDark", darkPath);
            EditorPrefs.SetString("DreamOS.CustomEditorLight", lightPath);
        }
        #endregion

        #region Tools Menu
        [MenuItem("Tools/DreamOS/Create Overlay Resources", false, 12)]
        static void CreateOverlayResources()
        {
            try
            {
                GetObjectPath();
                GameObject clone = AssetDatabase.LoadAssetAtPath(objectPath + "Main Sources/DreamOS Canvas.prefab", typeof(GameObject)) as GameObject;
                PrefabUtility.InstantiatePrefab(clone);
                Undo.RegisterCreatedObjectUndo(clone, "Created an object");
                clone.name = clone.name.Replace("(Clone)", "").Trim();
                MakeSceneDirty(clone, clone.name);
            }

            catch { ShowErrorDialog(); }
        }

        [MenuItem("Tools/DreamOS/Create World Space Resources", false, 12)]
        static void CreateWorldSpaceResources()
        {
            try
            {
                GetObjectPath();
                GameObject clone = AssetDatabase.LoadAssetAtPath(objectPath + "Main Sources/World Space Resources.prefab", typeof(GameObject)) as GameObject;
                PrefabUtility.InstantiatePrefab(clone);
                Undo.RegisterCreatedObjectUndo(clone, "Created an object");
                clone.name = clone.name.Replace("(Clone)", "").Trim();
                MakeSceneDirty(clone, clone.name);
            }

            catch { ShowErrorDialog(); }
        }

        [MenuItem("Tools/DreamOS/Create Multi Instance Manager", false, 12)]
        static void CreateMultiInstanceManager()
        {
            GameObject tempObj = new GameObject("Multi Instance Manager");
            tempObj.AddComponent<MultiInstanceManager>();
            Selection.activeObject = tempObj;
            MakeSceneDirty(tempObj, tempObj.name);
        }

        [MenuItem("Tools/DreamOS/Show App Library")]
        static void ShowAppLibrary()
        {
            Selection.activeObject = Resources.Load("Apps/App Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'App Library'. Make sure you have 'App Library' asset in Resources/Apps folder.");
        }

        [MenuItem("Tools/DreamOS/Show Chat Library")]
        static void ShowChatList()
        {
            Selection.activeObject = Resources.Load("Chats/Example Chat");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Example Chat'. Make sure you have 'Example Chat' asset in Resources/Chats folder.");
        }

        [MenuItem("Tools/DreamOS/Show Game Hub Library")]
        static void ShowGameHubLibrary()
        {
            Selection.activeObject = Resources.Load("Game Hub/Game Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Game Library'. Make sure you have 'Example Chat' asset in Resources/Game Hub folder.");
        }

        [MenuItem("Tools/DreamOS/Show Icon Library")]
        static void ShowIconLibrary()
        {
            Selection.activeObject = Resources.Load("Icons/Icon Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Icon Library'. Make sure you have 'Library' asset in Resources/Icons folder.");
        }

        [MenuItem("Tools/DreamOS/Show Mail Items")]
        static void ShowMailLibrary()
        {
            Selection.activeObject = Resources.Load("Mail/Example Mail");

            //  if (Selection.activeObject == null)
            //       Debug.Log("Can't find an asset named 'Example Mail'. Make sure you have 'Example Mail' asset in Resources/Mail folder.");
        }

        [MenuItem("Tools/DreamOS/Show Music Library")]
        static void ShowMusicLibrary()
        {
            Selection.activeObject = Resources.Load("Music Player/Music Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Music Library'. Make sure you have 'Library' asset in Resources/Music Player folder.");
        }

        [MenuItem("Tools/DreamOS/Show Notepad Library")]
        static void ShowNotepadLibrary()
        {
            Selection.activeObject = Resources.Load("Notepad/Note Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Note Library'. Make sure you have 'Library' asset in Resources/Notepad folder.");
        }

        [MenuItem("Tools/DreamOS/Show Photo Library")]
        static void ShowPhotoLibrary()
        {
            Selection.activeObject = Resources.Load("Photo Gallery/Photo Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Photo Library'. Make sure you have 'Library' asset in Resources/Gallery folder.");
        }

        [MenuItem("Tools/DreamOS/Show Profile Picture Library")]
        static void ShowPPLibrary()
        {
            Selection.activeObject = Resources.Load("Profile Pictures/Profile Picture Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Profile Picture Library'. Make sure you have 'Library' asset in Resources/Profile Pictures folder.");
        }

        [MenuItem("Tools/DreamOS/Show UI Manager")]
        static void ShowUIManager()
        {
            Selection.activeObject = Resources.Load("UI Manager/UI Manager");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'UI Manager'. Make sure you have 'UI Manager' asset in Resources/UI Manager folder. " +
                    "You can create a new Theme Manager asset or re-import the pack if you can't see the file.");
        }

        [MenuItem("Tools/DreamOS/Show Video Library")]
        static void ShowVideoLibrary()
        {
            Selection.activeObject = Resources.Load("Video Player/Video Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Video Library'. Make sure you have 'Library' asset in Resources/Video Player folder.");
        }

        [MenuItem("Tools/DreamOS/Show Web Library")]
        static void ShowWebLibrary()
        {
            Selection.activeObject = Resources.Load("Web Browser/Web Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Web Library'. Make sure you have 'Library' asset in Resources/Web Browser folder.");
        }

        [MenuItem("Tools/DreamOS/Show Widget Library")]
        static void ShowWidgetLibrary()
        {
            Selection.activeObject = Resources.Load("Widgets/Widget Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Widget Library'. Make sure you have 'Library' asset in Resources/Widgets folder.");
        }

        [MenuItem("Tools/DreamOS/Show Wallpaper Library")]
        static void ShowWPLibrary()
        {
            Selection.activeObject = Resources.Load("Wallpapers/Wallpaper Library");

            if (Selection.activeObject == null)
                Debug.Log("Can't find an asset named 'Wallpaper Library'. Make sure you have 'Wallpaper Library' asset in Resources/Wallpapers folder.");
        }
        #endregion

        #region Object Creating
        [System.Obsolete]
        static void CreateObject(string resourcePath)
        {
            try
            {
                GetObjectPath();
                UpdateCustomEditorPath();
                GameObject clone = Instantiate(AssetDatabase.LoadAssetAtPath(objectPath + resourcePath + ".prefab", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;

                try
                {
                    if (Selection.activeGameObject == null)
                    {
                        var canvas = (Canvas)GameObject.FindObjectsOfType(typeof(Canvas))[0];
                        clone.transform.SetParent(canvas.transform, false);
                    }

                    else { clone.transform.SetParent(Selection.activeGameObject.transform, false); }

                    clone.name = clone.name.Replace("(Clone)", "").Trim();
                    MakeSceneDirty(clone, clone.name);
                }

                catch
                {
                    CreateCanvas();
                    var canvas = (Canvas)GameObject.FindObjectsOfType(typeof(Canvas))[0];
                    clone.transform.SetParent(canvas.transform, false);
                    clone.name = clone.name.Replace("(Clone)", "").Trim();
                    MakeSceneDirty(clone, clone.name);
                }

                Selection.activeObject = clone;
            }

            catch { ShowErrorDialog(); }
        }

        [MenuItem("GameObject/DreamOS/Canvas", false, -1)]
        static void CreateCanvas()
        {
            try
            {
                GetObjectPath();
                UpdateCustomEditorPath();
                GameObject clone = Instantiate(AssetDatabase.LoadAssetAtPath(objectPath + "UI Elements/Other/Canvas.prefab", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                clone.name = clone.name.Replace("(Clone)", "").Trim();
                Selection.activeObject = clone;
                MakeSceneDirty(clone, clone.name);
            }

            catch { ShowErrorDialog(); }
        }
        #endregion

        #region Button
        [MenuItem("GameObject/DreamOS/Button/Desktop Button", false, 0)]
        [System.Obsolete]
        static void DesktopButton()
        {
            CreateObject("UI Elements/Button/Desktop Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Main Button", false, 0)]
        [System.Obsolete]
        static void MainButton()
        {
            CreateObject("UI Elements/Button/Main Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Main Button (Only Icon)", false, 0)]
        [System.Obsolete]
        static void MainButtonOnlyIcon()
        {
            CreateObject("UI Elements/Button/Main Button (Only Icon)");
        }

        [MenuItem("GameObject/DreamOS/Button/Main Button (With Icon)", false, 0)]
        [System.Obsolete]
        static void MainButtonWithIcon()
        {
            CreateObject("UI Elements/Button/Main Button (With Icon)");
        }

        [MenuItem("GameObject/DreamOS/Button/Nav Drawer Button", false, 0)]
        [System.Obsolete]
        static void NavDrawerButton()
        {
            CreateObject("UI Elements/Button/Nav Drawer Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Quick Center App Button", false, 0)]
        [System.Obsolete]
        static void QuickCenterAppButton()
        {
            CreateObject("UI Elements/Button/Quick Center App Button");
        }

        [MenuItem("GameObject/DreamOS/Button/Taskbar Button", false, 0)]
        [System.Obsolete]
        static void TaskbarButton()
        {
            CreateObject("UI Elements/Button/Taskbar Button");
        }
        #endregion

        #region Horizontal Selector
        [MenuItem("GameObject/DreamOS/Horizontal Selector/Standard", false, 0)]
        [System.Obsolete]
        static void HorizontalSelector()
        {
            CreateObject("UI Elements/Horizontal Selector/Horizontal Selector");
        }
        #endregion

        #region Input Field
        [MenuItem("GameObject/DreamOS/Input Field/Fading Input Field", false, 0)]
        [System.Obsolete]
        static void FadingInputField()
        {
            CreateObject("UI Elements/Input Field/Fading Input Field");
        }

        [MenuItem("GameObject/DreamOS/Input Field/Standard Input Field", false, 0)]
        [System.Obsolete]
        static void StandardInputField()
        {
            CreateObject("UI Elements/Input Field/Standard Input Field");
        }
        #endregion

        #region Loader
        [MenuItem("GameObject/DreamOS/Spinners/Default Spinner", false, 0)]
        [System.Obsolete]
        static void LoaderMaterial()
        {
            CreateObject("UI Elements/Spinner/Default Spinner");
        }
        #endregion

        #region Modal Window
        [MenuItem("GameObject/DreamOS/Modal Window/Standard", false, 0)]
        [System.Obsolete]
        static void ModalWindow()
        {
            CreateObject("UI Elements/Modal Window/Standard Modal Window");
        }
        #endregion

        #region Scrollbar
        [MenuItem("GameObject/DreamOS/Scrollbar/Standard", false, 0)]
        [System.Obsolete]
        static void Scrollbar()
        {
            CreateObject("UI Elements/Scrollbar/Scrollbar");
        }
        #endregion

        #region Slider
        [MenuItem("GameObject/DreamOS/Slider/Standard", false, 0)]
        [System.Obsolete]
        static void Slider()
        {
            CreateObject("UI Elements/Slider/Slider");
        }
        #endregion

        #region Switch
        [MenuItem("GameObject/DreamOS/Switch/Standard", false, 0)]
        [System.Obsolete]
        static void Switch()
        {
            CreateObject("UI Elements/Switch/Switch");
        }
        #endregion

        #region Vertical Selector
        [MenuItem("GameObject/DreamOS/Vertical Selector/Standard", false, 0)]
        [System.Obsolete]
        static void VerticalSelector()
        {
            CreateObject("UI Elements/Vertical Selector/Vertical Selector");
        }
        #endregion
    }
}
#endif