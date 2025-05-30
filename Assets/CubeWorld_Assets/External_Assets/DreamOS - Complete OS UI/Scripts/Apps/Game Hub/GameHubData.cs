using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Michsky.DreamOS
{
    // [AddComponentMenu("DreamOS/Game Hub/Game Hub Data")]
    public class GameHubData : MonoBehaviour
    {
        [Header("Settings")]
        public string subPath = "Data/Game Hub";
        public string fileName = "GameHub";
        public string fileExtension = ".data";
        string fullPath;

        [Header("Debug")]
        [TextArea] public string fileData;

        StreamReader reader = null;
        string currentName;
        string currentDownloadState;
        string currentShortcutState;
        public List<GameData> gameData = new List<GameData>();

        public class GameData
        {
            public string gameName;
            public string downloadState;
            public string shortcutState;
        }

        void Start()
        {
            fullPath = Application.dataPath + "/" + subPath + "/" + fileName + fileExtension;
            CheckForDataFile();

            // Debug
            FileInfo tempFile = new FileInfo(fullPath);
            reader = tempFile.OpenText();
            fileData = reader.ReadToEnd();
            reader.Close();
        }

        public void CheckForDataFile()
        {
            if (!File.Exists(fullPath))
            {
                FileInfo dataFile = new FileInfo(fullPath);
                dataFile.Directory.Create();
                File.WriteAllText(fullPath, "GAME_DATA");
            }
        }

        public void ReadGameData()
        {
            CheckForDataFile();

            foreach (string option in File.ReadLines(fullPath))
            {
                if (option.Contains("GameName:"))
                {
                    string tempName = option.Replace("GameName:", "");
                    currentName = tempName;
                }

                else if (option.Contains("[Downloaded]"))
                {
                    string tempState = option.Replace("[Downloaded]", "");
                    currentDownloadState = tempState;
                }

                else if (option.Contains("[ShortcutCreated]"))
                {
                    string tempState = option.Replace("[ShortcutCreated]", "");
                    currentShortcutState = tempState;

                    GameData dataItem = new GameData();
                    dataItem.gameName = currentName;
                    dataItem.downloadState = currentDownloadState;
                    dataItem.shortcutState = currentShortcutState;
                    gameData.Add(dataItem);
                }
            }
        }

        public void ApplyGameData(string gameName, string downloadState, string shortcutState)
        {
            File.AppendAllText(fullPath, "\n\nGameName:" + gameName +
                "\n{" +
                "\n[Downloaded]" + downloadState +
                "\n[ShortcutCreated]" + shortcutState +
                "\n}");
        }

        public void ResetData()
        {
            File.WriteAllText(fullPath, "GAME_DATA");
        }
    }
}