using UnityEngine;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Web Browser/Download Helper")]
    public class DownloadHelper : MonoBehaviour
    {
        [Header("Settings")]
        public bool openDownloadsOnStart = true;

        WebBrowserManager wbm;
        WindowManager wm;

        private void Start()
        {
            try
            {
                var wbmObj = (WebBrowserManager)GameObject.FindObjectsOfType(typeof(WebBrowserManager))[0];
                wbm = wbmObj;

                if (openDownloadsOnStart == true)
                    wm = wbm.downloadsWindow;
            }

            catch { Debug.Log("<b>[Download Helper]</b> Cannot find any available Web Browser Manager."); }
        }

        public void DownloadFile(string title)
        {
            if (wbm != null) { wbm.DownloadFile(title); }
            if (openDownloadsOnStart == true) { wm.OpenWindow(); }
        }
    }
}