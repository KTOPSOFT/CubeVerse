using UnityEngine;
using System.IO;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Notepad/Notepad Storing")]
    public class NotepadStoring : MonoBehaviour
    {
        [Header("Resources")]
        public NotepadManager notepadManager;

        [Header("Settings")]
        public string subPath = "Data";
        public string fileName = "StoredNotes";
        public string fileExtension = ".data";

        string fullPath;
        int noteIndex;
        string currentTitle;
        string currentContent;

        void Awake()
        {
            if (notepadManager == null)
                notepadManager = gameObject.GetComponent<NotepadManager>();
        }

        public void CheckForDataFile()
        {
#if UNITY_EDITOR
            fullPath = Application.dataPath + "/" + subPath + "/" + fileName + fileExtension;
#else
            string appPath = Application.dataPath;
            appPath = appPath.Replace(Application.productName + "_Data", "");
            fullPath = appPath + subPath + "/" + "/" + fileName + fileExtension;
#endif

            if (!File.Exists(fullPath))
            {
                FileInfo dataFile = new FileInfo(fullPath);
                dataFile.Directory.Create();
                File.WriteAllText(fullPath, "NOTE_DATA");
            }
        }

        public void UpdateData()
        {
            File.WriteAllText(fullPath, "NOTE_DATA");

            for (int i = 0; i < notepadManager.libraryAsset.notes.Count; ++i)
            {
                if (notepadManager.libraryAsset.notes[i].isCustom == true)
                {
                    noteIndex = i;
                    WriteNoteData(i);
                }
            }
        }

        public void WriteNoteData(int tempIndex)
        {
            File.AppendAllText(fullPath, "\n\nNoteIndex: " + noteIndex.ToString() +
              "\n{" +
              "\n[Title] " + notepadManager.libraryAsset.notes[tempIndex].noteTitle +
              "\n[Content] " + notepadManager.libraryAsset.notes[tempIndex].noteContent +
              "\n}");
        }

        public void ReadNoteData()
        {
            CheckForDataFile();

            foreach (string option in File.ReadLines(fullPath))
            {
                if (option.Contains("NoteIndex: "))
                {
                    int tempIndex = int.Parse(option.Replace("NoteIndex: ", ""));
                    noteIndex = tempIndex;
                }

                else if (option.Contains("[Title] "))
                {
                    string tempTitle = option.Replace("[Title] ", "");
                    currentTitle = tempTitle;
                }

                else if (option.Contains("[Content] "))
                {
                    string tempContent = option.Replace("[Content] ", "");
                    currentContent = tempContent;
                    notepadManager.CreateStoredNote(currentTitle, currentContent);
                }
            }
        }
    }
}