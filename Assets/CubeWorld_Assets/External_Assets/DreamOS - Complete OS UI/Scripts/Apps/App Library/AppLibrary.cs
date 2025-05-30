using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New App Library", menuName = "DreamOS/New App Library")]
    public class AppLibrary : ScriptableObject
    {
        // Settings
        public bool alwaysUpdate = false;
        public bool optimizeUpdates = true;

        // Content
        public List<AppItem> apps = new List<AppItem>();

        [System.Serializable]
        public class AppItem
        {
            public string appTitle = "App Name";
            public Texture2D appIconPreview;
            public Sprite appIconBig;
            public Sprite appIconMedium;
            public Sprite appIconSmall;
            public Color gradientLeft = new Color32(215, 215, 215, 255);
            public Color gradientRight = new Color32(255, 255, 255, 255);
        }
    }
}