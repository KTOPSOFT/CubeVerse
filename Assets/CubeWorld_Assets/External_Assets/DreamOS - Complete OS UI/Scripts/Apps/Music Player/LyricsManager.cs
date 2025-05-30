using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.IO;

namespace Michsky.DreamOS
{
    public class LyricsManager : MonoBehaviour
    {
        [Header("Resources")]
        public MusicPlayerManager mpManager;
        public GameObject lyricItem;
        public Transform lyricParent;

        [Header("Settings")]
        public string subPath = "Lyrics";
        public string fileExtension = ".lrc";
        private string fullPath;

        [Header("Events")]
        public UnityEvent onLyricFound;
        public UnityEvent onLyricMissing;

        public bool lyricFound;
        private float secondsToNext;
        private int currentLine;
        private LyricsLine currentLyricItem;
        private LyricsLine upcomingLyricItem;
        private List<LineItem> lines = new List<LineItem>();

        [System.Serializable]
        public class LineItem
        {
            public int lrcMinute;
            public float lrcSeconds;
            public string lrcLine;
        }

        void CheckForDataFile()
        {
#if UNITY_EDITOR
            fullPath = Application.dataPath + "/" + subPath + "/";
#else
            string appPath = Application.dataPath;
            appPath = appPath.Replace(Application.productName + "_Data", "");
            fullPath = appPath + subPath + "/";
#endif
        }

        public void ReadLyricData(string songName)
        {
            CheckForDataFile();

            if (!File.Exists(fullPath + songName + fileExtension))
            {
                lyricFound = false;
                onLyricMissing.Invoke();
                return;
            }

            currentLine = 0;
            currentLyricItem = null;
            upcomingLyricItem = null;
            lyricFound = true;
            lines.Clear();

            foreach (Transform child in lyricParent)
                Destroy(child.gameObject);

            foreach (string option in File.ReadLines(fullPath + songName + fileExtension))
            {
                int tempMin;
                float tempSeconds;
                string tempLine = null;

                try
                {
                    tempMin = int.Parse(option[2].ToString());
                    string secHelper = option[4].ToString() + option[5].ToString() + "." + option[7].ToString() + option[8].ToString();
                    tempSeconds = float.Parse(secHelper);
                }

                catch { continue; }

                var splitPoint = option.IndexOf("]", 1);
                tempLine = option.Substring(splitPoint + 1);

                LineItem newItem = new LineItem();
                newItem.lrcMinute = tempMin;
                newItem.lrcSeconds = tempSeconds;
                newItem.lrcLine = tempLine;
                lines.Add(newItem);
            }

            onLyricFound.Invoke();
            Continue();
            UpdateCurrentLyric();
        }

        public void Continue()
        {
            if (lyricFound == false || currentLine >= lines.Count - 1)
                return;

            // Check for the current item
            CheckForLyricItems();

            if (lines[currentLine].lrcSeconds < lines[currentLine + 1].lrcSeconds)
                secondsToNext = lines[currentLine + 1].lrcSeconds - lines[currentLine].lrcSeconds;
            else
                secondsToNext = 60 - lines[currentLine].lrcSeconds + lines[currentLine + 1].lrcSeconds;

            StartCoroutine("ShowLyrics", secondsToNext);
        }

        void CheckForLyricItems()
        {
            // Setting current lyric
            if (upcomingLyricItem != null) { currentLyricItem = upcomingLyricItem; currentLyricItem.SetCurrent(); }
            else if (currentLyricItem == null)
            {
                GameObject currentLyric = Instantiate(lyricItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                currentLyric.transform.SetParent(lyricParent, false);
                currentLyricItem = currentLyric.GetComponent<LyricsLine>();
                currentLyricItem.textObject.text = lines[currentLine].lrcLine;
                currentLyricItem.SetCurrent();
            }

            // Setting upcoming lyric
            if (currentLine + 1 < lines.Count - 1)
            {
                GameObject newUpcomingLyric = Instantiate(lyricItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                newUpcomingLyric.transform.SetParent(lyricParent, false);
                upcomingLyricItem = newUpcomingLyric.GetComponent<LyricsLine>();
                upcomingLyricItem.textObject.text = lines[currentLine + 1].lrcLine;
                upcomingLyricItem.SetIn();
            }
        }

        void UpdateLyricItems()
        {
            GameObject currentLyric = Instantiate(lyricItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            currentLyric.transform.SetParent(lyricParent, false);
            currentLyricItem = currentLyric.GetComponent<LyricsLine>();
            currentLyricItem.textObject.text = lines[currentLine].lrcLine;
            currentLyricItem.SetCurrent();

            if (currentLine + 1 < lines.Count - 1)
            {
                GameObject newUpcomingLyric = Instantiate(lyricItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                newUpcomingLyric.transform.SetParent(lyricParent, false);
                upcomingLyricItem = newUpcomingLyric.GetComponent<LyricsLine>();
                upcomingLyricItem.textObject.text = lines[currentLine + 1].lrcLine;
                upcomingLyricItem.SetIn();
            }
        }

        public void Pause()
        {
            StopCoroutine("ShowLyrics");
        }

        public void UpdateCurrentLyric()
        {
            StopCoroutine("ShowLyrics");

            if (lyricFound == false) { Continue(); }

            foreach (Transform child in lyricParent)
                Destroy(child.gameObject);

            for (int i = 0; i < lines.Count; ++i)
            {
                if (lines[i].lrcMinute == mpManager.minutes && lines[i].lrcSeconds >= mpManager.secondsRaw && mpManager.secondsRaw >= 0)
                {
                    currentLine = i - 1;
                    if (currentLine < 0) { currentLine = 0; }
                    UpdateLyricItems();
                    secondsToNext = lines[currentLine + 1].lrcSeconds - mpManager.secondsRaw;
                    StartCoroutine("ShowLyrics", secondsToNext);
                    break;
                }
            }
        }

        IEnumerator ShowLyrics(float time)
        {
            yield return new WaitForSeconds(time);
            if (currentLyricItem != null) { currentLyricItem.SetOut(); }
            currentLine++;
            Continue();
        }
    }
}