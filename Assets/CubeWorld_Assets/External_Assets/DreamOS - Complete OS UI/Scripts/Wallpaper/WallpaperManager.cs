using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class WallpaperManager : MonoBehaviour
    {
        // Resources
        public WallpaperLibrary wallpaperLibrary;
        public Image desktopSource;
        public Image lockScreenSource;
        public Image selectedWallpaper;
        public Transform wallpaperParent;
        public GameObject wallpaperItem;

        // Settings
        public int wallpaperIndex;
        public bool saveSelected = true;
        string saveKey = "DreamOS";

        // Multi Instance Support
        public UserManager userManager;

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            InitializeWallpapers();
        }

        void OnEnable()
        {
            if (saveSelected == true && PlayerPrefs.HasKey(saveKey + "CurrentWallpaper"))
            {
                wallpaperIndex = PlayerPrefs.GetInt(saveKey + "CurrentWallpaper");
                desktopSource.sprite = wallpaperLibrary.wallpapers[wallpaperIndex].wallpaperSprite;
            }

            else if (saveSelected == true && !PlayerPrefs.HasKey(saveKey + "CurrentWallpaper"))
            {
                PlayerPrefs.SetInt(saveKey + "CurrentWallpaper", wallpaperIndex);
                desktopSource.sprite = wallpaperLibrary.wallpapers[wallpaperIndex].wallpaperSprite;
            }

            else { desktopSource.sprite = wallpaperLibrary.wallpapers[wallpaperIndex].wallpaperSprite; }

            if (selectedWallpaper != null) { selectedWallpaper.sprite = desktopSource.sprite; }
            if (lockScreenSource != null) { lockScreenSource.sprite = desktopSource.sprite; }
        }

        public void InitializeWallpapers()
        {
            if (wallpaperParent == null || wallpaperItem == null)
                return;

            foreach (Transform child in wallpaperParent) { Destroy(child.gameObject); }
            for (int i = 0; i < wallpaperLibrary.wallpapers.Count; ++i)
            {
                GameObject go = Instantiate(wallpaperItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(wallpaperParent, false);
                go.name = wallpaperLibrary.wallpapers[i].wallpaperID;

                Image prevImage = go.transform.Find("Image Parent/Image").GetComponent<Image>();
                prevImage.sprite = wallpaperLibrary.wallpapers[i].wallpaperSprite;

                Button wpButton = go.GetComponent<Button>();
                wpButton.onClick.AddListener(delegate { SetWallpaper(go.transform.GetSiblingIndex()); });
            }
        }

        public void SetWallpaper(int index)
        {
            wallpaperIndex = index;
            desktopSource.sprite = wallpaperLibrary.wallpapers[wallpaperIndex].wallpaperSprite;

            if (saveSelected == true) { PlayerPrefs.SetInt(saveKey + "CurrentWallpaper", wallpaperIndex); }
            if (selectedWallpaper != null) { selectedWallpaper.sprite = desktopSource.sprite; }
            if (lockScreenSource != null) { lockScreenSource.sprite = desktopSource.sprite; }
        }

        public void AddWallpaperToLibrary(Sprite wImage, string wName)
        {
            if (wallpaperLibrary == null)
            {
                Debug.LogError("<b>[Wallpaper Manager]</b> Cannot add the wallpaper due to missing library.");
                return;
            }

            WallpaperLibrary.WallpaperItem item = new WallpaperLibrary.WallpaperItem();
            item.wallpaperSprite = wImage;
            item.wallpaperID = wName;
            wallpaperLibrary.wallpapers.Add(item);
        }
    }
}