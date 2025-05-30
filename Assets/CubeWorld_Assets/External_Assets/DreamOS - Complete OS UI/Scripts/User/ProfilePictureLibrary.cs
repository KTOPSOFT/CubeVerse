using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Profile Picture Library", menuName = "DreamOS/New Profile Picture Library")]
    public class ProfilePictureLibrary : ScriptableObject
    {
        // Content
        public List<PPItem> pictures = new List<PPItem>();

        [System.Serializable]
        public class PPItem
        {
            public string pictureID = "Picture";
            public Sprite pictureSprite;
        }
    }
}