using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Wallpaper Library", menuName = "DreamOS/New Wallpaper Library")]
    public class WallpaperLibrary : ScriptableObject
    {
        // Content
        public List<WallpaperItem> wallpapers = new List<WallpaperItem>();

        [System.Serializable]
        public class WallpaperItem
        {
            public string wallpaperID = "Wallpaper";
            public Sprite wallpaperSprite;
        }
    }
}