using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.IO;

namespace Michsky.DreamOS
{
    [DefaultExecutionOrder(-100)]
    public class ModManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private GameObject modLibraryElement;
        [SerializeField] private Transform modLibraryParent;
        [SerializeField] private GameObject noModsIndicator;

        // Modules
        public bool enableMusicPlayerModule = true;
        [SerializeField] private MusicPlayerManager musicPlayer;
        [SerializeField] private string musicPlayerID = "Music Player";
        private int mpModCount;
        public bool enableNotepadModule = true;
        [SerializeField] private NotepadManager notepad;
        [SerializeField] private string notepadID = "Notepad";
        private int npModCount;
        public bool enablePhotoGalleryModule = true;
        [SerializeField] private PhotoGalleryManager photoGallery;
        [SerializeField] private string photoGalleryID = "Photo Gallery";
        private int pgModCount;
        public bool enableVideoPlayerModule = true;
        [SerializeField] private VideoPlayerManager videoPlayer;
        [SerializeField] private string videoPlayerID = "Video Player";
        private int vpModCount;

        // Settings
        [SerializeField] private bool defaultModState = true;
        [SerializeField] private bool initOnEnable = true;
        private bool initialized = false;
        [SerializeField] private string subPath = "Mods";
        [SerializeField] private string dataName = "ModData";
        [SerializeField] private string fileExtension = ".data";
     
        private string fullPath;
        private int modCount;

        private string currentTitle;
        private string currentDescription;
        private string currentIcon;
        private Image currentIconImg;
        private string currentModuleType;
        private string currentModuleAsset;

        private List<ModItem> mods = new List<ModItem>();

        [System.Serializable]
        public class ModItem
        {
            public int modIndex;
            public string modTitle;
            public string modDescription;
            public Sprite modIcon;
            public string modAsset;
            public MusicPlayerPlaylist.MusicItem musicModule;
            public NotepadLibrary.NoteItem noteModule;
            public PhotoGalleryLibrary.PictureItem photoModule;
            public VideoPlayerLibrary.VideoItem videoModule;
            public WebBrowserLibrary.WebPage webPageModule;
            public ModuleType moduleType;
        }

        public enum ModuleType
        {
            MusicPlayer,
            Notepad,
            PhotoGallery,
            VideoPlayer,
            WebBrowser
        }

        void OnEnable()
        {
            if (initOnEnable == false)
                return;

            initialized = false;
            InitializeMods();
        }

        public void InitializeMods() { ReadModData(); }

        void DeleteModItems()
        {
            if (mpModCount != 0)
            {
                for (int i = 0; i < mods.Count; ++i)
                {
                    musicPlayer.libraryPlaylist.playlist.Remove(mods[i].musicModule);
                    musicPlayer.modPlaylist.playlist.Remove(mods[i].musicModule);
                }
            }

            if (npModCount != 0)
            {
                for (int i = 0; i < mods.Count; ++i)
                    notepad.libraryAsset.notes.Remove(mods[i].noteModule);
            }

            if (pgModCount != 0)
            {
                for (int i = 0; i < mods.Count; ++i)
                    photoGallery.libraryAsset.pictures.Remove(mods[i].photoModule);
            }

            if (vpModCount != 0)
            {
                for (int i = 0; i < mods.Count; ++i)
                    videoPlayer.libraryAsset.videos.Remove(mods[i].videoModule);
            }

            mods.Clear();
            modCount = 0;
            mpModCount = 0;
            npModCount = 0;
            pgModCount = 0;
            vpModCount = 0;
        }

        void CheckForDataFile()
        {
#if UNITY_EDITOR
            fullPath = Application.dataPath + "/" + subPath + "/";
#else
            string appPath = Application.dataPath;
            appPath = appPath.Replace(Application.productName + "_Data", "");
            fullPath = appPath + subPath + "/";
#endif
        }

        void ResetData()
        {
            File.WriteAllText(fullPath, "// You can add/modify available mods here." +
                                        "\n// Available modules: Music Player, Notead, Photo Gallery, Video Player" +
                                        "\n// FAQs and how to use: https://docs.michsky.com/docs/dreamos/mod-manager");
        }

        public void ReadModData()
        {
            DeleteModItems();
            CheckForDataFile();

            if (!Directory.Exists(fullPath))
                return;

            // Scan available mods
            int scannedModCount = 0;
            List<string> scannedMods = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            FileInfo[] info = dir.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
            scannedModCount = info.Length - 1;

            foreach (FileInfo file in info)
            { 
                if (file.Name != "ModLibrary" + fileExtension)
                    scannedMods.Add(file.DirectoryName + "/" + dataName + fileExtension);
            }

            // Clear the library parent
            foreach (Transform child in modLibraryParent)
                Destroy(child.gameObject);

            // Process mods
            for (int i = 0; i < scannedMods.Count; ++i)
            {
                foreach (string option in File.ReadLines(scannedMods[i]))
                {
                    if (option.Contains("[Title] "))
                    {
                        string tempType = option.Replace("[Title] ", "");
                        currentTitle = tempType;
                    }

                    else if (option.Contains("[Description] "))
                    {
                        string tempDesc = option.Replace("[Description] ", "");
                        currentDescription = tempDesc;
                    }

                    else if (option.Contains("[Icon] "))
                    {
                        string tempIcon = option.Replace("[Icon] ", "");
                        currentIcon = tempIcon;
                    }

                    else if (option.Contains("[ModuleType] "))
                    {
                        string tempMT = option.Replace("[ModuleType] ", "");
                        currentModuleType = tempMT;
                    }

                    else if (option.Contains("[ModuleAsset] "))
                    {
                        string tempMA = option.Replace("[ModuleAsset] ", "");
                        currentModuleAsset = tempMA;

                        GameObject modElement = Instantiate(modLibraryElement, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        modElement.transform.SetParent(modLibraryParent, false);
                        modElement.name = currentTitle;

                        TextMeshProUGUI titleText = modElement.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                        titleText.text = currentTitle;

                        TextMeshProUGUI descText = modElement.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                        descText.text = currentDescription;

                        Image iconImage = modElement.transform.Find("Icon").GetComponent<Image>();

                        try
                        {
                            iconImage.sprite = LoadNewSprite(fullPath + currentModuleType + "/" + currentTitle + "/" + currentIcon);
                            currentIconImg = iconImage;
                        }

                        catch { currentIconImg = iconImage; }

                        CreateModuleItem();
                        ModItem currentModule = mods[mods.Count - 1];

                        GameObject moduleIcon;
                        try {  moduleIcon = modElement.transform.Find("Modules/" + currentModuleType).gameObject;     }
                        catch { moduleIcon = modElement.transform.Find("Modules/Default").gameObject; } 
                        moduleIcon.SetActive(true);

                        SwitchManager modSwitch = modElement.transform.Find("Switch").GetComponent<SwitchManager>();
                        modSwitch.isOn = defaultModState;
                        modSwitch.switchTag = currentTitle;
                        modSwitch.OnEvents.AddListener(() => CreateModule(currentModule));
                        modSwitch.OffEvents.AddListener(() => DeleteModule(currentModule));
                        modSwitch.CheckForData();
                        modSwitch.InvokeEvents();

                        modCount++;
                    }
                }
            }

            if (modCount == 0) { noModsIndicator.SetActive(true); }      
            else 
            {
                noModsIndicator.SetActive(false);
                
                if (mpModCount != 0) { musicPlayer.modSupport = true; musicPlayer.InitializePlayer(); }
                if (npModCount != 0) { notepad.modSupport = true; notepad.InitializeNotes(); }
                if (pgModCount != 0) { photoGallery.modSupport = true; photoGallery.InitializePhotos(); }
                if (vpModCount != 0) { videoPlayer.modSupport = true; videoPlayer.InitializeVideoPlayer(); }
            }

            initialized = true;
        }

        #region Module Actions
        void CreateModuleItem()
        {
            ModItem newItem = new ModItem();
            newItem.modTitle = currentTitle;
            newItem.modDescription = currentDescription;
            newItem.modIcon = currentIconImg.sprite;
            newItem.modAsset = currentModuleAsset;

            if (currentModuleType == musicPlayerID) { newItem.moduleType = ModuleType.MusicPlayer; mpModCount++; newItem.modIndex = mpModCount; }
            else if (currentModuleType == notepadID) { newItem.moduleType = ModuleType.Notepad; npModCount++; newItem.modIndex = npModCount; }
            else if (currentModuleType == photoGalleryID) { newItem.moduleType = ModuleType.PhotoGallery; pgModCount++; newItem.modIndex = pgModCount; }
            else if (currentModuleType == videoPlayerID) { newItem.moduleType = ModuleType.VideoPlayer; vpModCount++; newItem.modIndex = vpModCount; }

            mods.Add(newItem);
        }

        void CreateModule(ModItem module) { StartCoroutine("ModifyModule", module); }

        void DeleteModule(ModItem module)
        {
            if (module.moduleType == ModuleType.MusicPlayer)
            {
                if (musicPlayer.source != null) { musicPlayer.StopMusic(); }

                musicPlayer.modPlaylist.playlist.Remove(module.musicModule);
                musicPlayer.libraryPlaylist.playlist.Remove(module.musicModule);
                if (initialized == true) { module.musicModule.modHelper = true; musicPlayer.InitializePlayer(); }
            }

            else if (module.moduleType == ModuleType.Notepad)
            {
                notepad.libraryAsset.notes.Remove(module.noteModule);
                if (initialized == true) { module.noteModule.modHelper = true; notepad.InitializeNotes(); }
            }

            else if (module.moduleType == ModuleType.PhotoGallery)
            {
                photoGallery.libraryAsset.pictures.Remove(module.photoModule);
                if (initialized == true) { module.photoModule.modHelper = true; photoGallery.InitializePhotos(); }
            }

            else if (module.moduleType == ModuleType.VideoPlayer)
            {
                if (videoPlayer.videoComponent != null) { videoPlayer.videoComponent.Stop(); }
              
                videoPlayer.libraryAsset.videos.Remove(module.videoModule);
                if (initialized == true) { module.videoModule.modHelper = true; videoPlayer.InitializeVideoPlayer(); }
            }
        }

        IEnumerator ModifyModule(ModItem module)
        {
            if (module.moduleType == ModuleType.MusicPlayer)
            {
                MusicPlayerPlaylist.MusicItem tempItem = new MusicPlayerPlaylist.MusicItem();
                tempItem.musicTitle = module.modTitle;
                tempItem.artistTitle = module.modDescription;
                tempItem.musicCover = module.modIcon;
                tempItem.isModContent = true;

                string mPath = fullPath + musicPlayerID + "/" + module.modTitle + "/" + module.modAsset;

                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(mPath, AudioType.MPEG);
                yield return www.SendWebRequest();

                tempItem.musicClip = DownloadHandlerAudioClip.GetContent(www);
                musicPlayer.modPlaylist.playlist.Add(tempItem);
                musicPlayer.libraryPlaylist.playlist.Add(tempItem);

                if (initialized == true) { musicPlayer.InitializePlayer(); }
              
                module.musicModule = tempItem;
            }

            else if (module.moduleType == ModuleType.Notepad)
            {
                NotepadLibrary.NoteItem tempItem = new NotepadLibrary.NoteItem();
                tempItem.noteTitle = module.modTitle;
                tempItem.noteContent = module.modAsset;
                tempItem.isModContent = true;

                notepad.libraryAsset.notes.Add(tempItem);

                if (initialized == true) { notepad.InitializeNotes(); }
               
                module.noteModule = tempItem;
            }

            else if (module.moduleType == ModuleType.PhotoGallery)
            {
                PhotoGalleryLibrary.PictureItem tempItem = new PhotoGalleryLibrary.PictureItem();
                tempItem.pictureTitle = module.modTitle;
                tempItem.pictureDescription = module.modDescription;
                tempItem.pictureSprite = module.modIcon;
                tempItem.isModContent = true;

                photoGallery.libraryAsset.pictures.Add(tempItem);

                if (initialized == true) { photoGallery.InitializePhotos(); }

                module.photoModule = tempItem;
            }

            else if (module.moduleType == ModuleType.VideoPlayer)
            {
                VideoPlayerLibrary.VideoItem tempItem = new VideoPlayerLibrary.VideoItem();
                tempItem.videoTitle = module.modTitle;
                tempItem.videoDescription = module.modDescription;
                tempItem.videoCover = module.modIcon;
                tempItem.playFromURL = true;
                tempItem.isModContent = true;

                string vPath = fullPath + videoPlayerID + "/" + module.modTitle + "/" + module.modAsset;

                tempItem.videoURL = vPath;

                videoPlayer.libraryAsset.videos.Add(tempItem);

                if (initialized == true) { videoPlayer.InitializeVideoPlayer(); }

                module.videoModule = tempItem;
            }
        }
        #endregion

        #region Mod Icon
        public Sprite LoadNewSprite(string filePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            Texture2D spriteTexture = LoadTexture(filePath);
            Sprite newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
            return newSprite;
        }

        public Texture2D LoadTexture(string filePath)
        {
            Texture2D Tex2D;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                Tex2D = new Texture2D(2, 2);

                if (Tex2D.LoadImage(fileData))
                    return Tex2D;
            }

            return null;
        }
        #endregion
    }
}