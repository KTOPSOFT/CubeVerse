using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Music Playlist", menuName = "DreamOS/New Music Playlist")]
    public class MusicPlayerPlaylist : ScriptableObject
    {
        // Settings
        public Sprite coverImage;
        public string playlistName;

        // Playlist Content
        public List<MusicItem> playlist = new List<MusicItem>();

        [System.Serializable]
        public class MusicItem
        {
            public string musicTitle = "Music Title";
            public string artistTitle = "Artist Title";
            public string albumTitle = "Album Title";
            public AudioClip musicClip;
            public Sprite musicCover;
            [HideInInspector] public bool excludeFromLibrary = false;
            [HideInInspector] public bool isModContent = false;
            [HideInInspector] public bool modHelper = false;
        }
    }
}