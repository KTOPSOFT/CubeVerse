using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class FetchWallpaperItems : MonoBehaviour
    {
        [Header("Resources")]
        public WallpaperManager wallpaperManager;
        public Transform wallpaperParent;
        public Image wallpaperPreview;

        public void FetchItems()
        {
            if (wallpaperManager == null)
            {
                try { wallpaperManager = (WallpaperManager)GameObject.FindObjectsOfType(typeof(WallpaperManager))[0]; }
                catch { Debug.Log("<b>[Fetch Wallpaper Items]</b> Wallpaper Manager is missing.", this); return; }
            }

            foreach (Transform child in wallpaperParent)
                Destroy(child.gameObject);

            for (int i = 0; i < wallpaperManager.wallpaperLibrary.wallpapers.Count; ++i)
            {
                GameObject go = Instantiate(wallpaperManager.wallpaperItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(wallpaperParent, false);
                go.name = wallpaperManager.wallpaperLibrary.wallpapers[i].wallpaperID;

                Image prevImage = go.transform.Find("Image Parent/Image").GetComponent<Image>();
                prevImage.sprite = wallpaperManager.wallpaperLibrary.wallpapers[i].wallpaperSprite;

                Button wpButton = go.GetComponent<Button>();
                wpButton.onClick.AddListener(delegate { SetWallpaper(go.transform.GetSiblingIndex()); });
            }
        }

        public void SetWallpaper(int index)
        {
            wallpaperManager.wallpaperIndex = index;
            wallpaperManager.desktopSource.sprite = wallpaperManager.wallpaperLibrary.wallpapers[wallpaperManager.wallpaperIndex].wallpaperSprite;

            if (wallpaperManager.saveSelected == true) { PlayerPrefs.SetInt("DreamOS" + "CurrentWallpaper", wallpaperManager.wallpaperIndex); }
            if (wallpaperManager.selectedWallpaper != null) { wallpaperManager.selectedWallpaper.sprite = wallpaperManager.desktopSource.sprite; }
            if (wallpaperPreview != null) { wallpaperPreview.sprite = wallpaperManager.desktopSource.sprite; }
            if (wallpaperManager.lockScreenSource != null) { wallpaperManager.lockScreenSource.sprite = wallpaperManager.desktopSource.sprite; }
        }
    }
}