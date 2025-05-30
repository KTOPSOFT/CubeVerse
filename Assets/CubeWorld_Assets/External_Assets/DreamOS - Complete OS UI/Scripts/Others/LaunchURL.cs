using UnityEngine;

namespace Michsky.DreamOS
{
    public class LaunchURL : MonoBehaviour
    {
        public void OpenURL(string urlLink)
        {
            Application.OpenURL(urlLink);
        }
    }
}