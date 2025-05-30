using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Game Hub Library", menuName = "DreamOS/New Game Hub Library")]
    public class GameHubLibrary : ScriptableObject
    {
        // Library Content
        public List<GameHubItem> games = new List<GameHubItem>();

        [System.Serializable]
        public class GameHubItem
        {
            public string gameTitle = "Game Title";
            [TextArea] public string gameDescription = "Description";
            [Tooltip("Calculation: megabytes")] public float gameSize = 128;
            public Sprite gameIcon;
            public Sprite gameSmallIcon;
            public Sprite gameArtwork;
            public Sprite gameBanner;
            public GameObject gamePrefab;
            public bool gameDownloaded;
            [HideInInspector] public bool isDownloading;
        }
    }
}