//This asset was uploaded by https://unityassetcollection.com

using UnityEngine;

namespace Michsky.DreamOS
{
    public class WebBrowserEventHelper : MonoBehaviour
    {
        WebBrowserManager wbm;

        void Start()
        {
            var wbmObj = (WebBrowserManager)GameObject.FindObjectsOfType(typeof(WebBrowserManager))[0];
            wbm = wbmObj;
        }
        public void TryAgain()
        {
            if (wbm != null)
            {
                try
                {
                    wbm.previousSites.RemoveAt(wbm.previousSites.Count - 1);
                    wbm.dirtIndex = wbm.previousSites.Count - 1;
                    wbm.Refresh();
                }

                catch { }       
            }
        }

        public void GoBack() { if (wbm != null) { wbm.GoBack(); } }
        public void GoForward() { if (wbm != null) { wbm.GoForward(); } }
        public void GoHome() { if (wbm != null) { wbm.OpenPage(":home"); } }
        public void Refresh() { if (wbm != null) { wbm.Refresh(); } }
        public void GoSite(string newSite) { if (wbm != null) { wbm.OpenPage(newSite); } }
    }
}