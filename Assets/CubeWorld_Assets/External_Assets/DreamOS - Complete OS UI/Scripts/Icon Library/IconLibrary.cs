using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Icon Library", menuName = "DreamOS/New Icon Library")]
    public class IconLibrary : ScriptableObject
    {
        // Settings
        public bool alwaysUpdate = false;
        public bool optimizeUpdates = true;
        public Texture2D searchIcon;

        // Content
        public List<IconItem> icons = new List<IconItem>();

        [System.Serializable]
        public class IconItem
        {
            public string iconTitle = "Icon";
            public Texture2D iconPreview;
            public Sprite iconSprite32;
            public Sprite iconSprite64;
            public Sprite iconSprite128;
            public Sprite iconSprite256;
        }
    }
}