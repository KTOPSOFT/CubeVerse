using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Video Library", menuName = "DreamOS/New Video Library")]
    public class VideoPlayerLibrary : ScriptableObject
    {
        // Library Content
        public List<VideoItem> videos = new List<VideoItem>();

        [System.Serializable]
        public class VideoItem
        {
            public string videoTitle = "Video Title";
            public string videoDescription = "Video Description";
            public Sprite videoCover;
            public VideoClip videoClip;
            public bool playFromURL;
            public string videoURL;
            [HideInInspector] public bool excludeFromLibrary = false;
            [HideInInspector] public bool isModContent = false;
            [HideInInspector] public bool modHelper = false;
        }
    }
}