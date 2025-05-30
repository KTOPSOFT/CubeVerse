using UnityEngine;

namespace Michsky.DreamOS
{
    public class NavDrawerExpandButton : MonoBehaviour
    {
        [Header("Resources")]
        public Animator buttonAnimator;

        [Header("Settings")]
        public string appName = "App Name";

        void Awake()
        {
            if (buttonAnimator == null) { buttonAnimator = gameObject.GetComponent<Animator>(); }
        }

        void OnEnable()
        {
            if (PlayerPrefs.GetString(appName + "NavDrawer") == "true")
                buttonAnimator.Play("Minimize");
            else if (PlayerPrefs.GetString(appName + "NavDrawer") == "false"
                || !PlayerPrefs.HasKey(appName + "NavDrawer"))
                buttonAnimator.Play("Start");
        }

        public void AnimateButton()
        {
            if (PlayerPrefs.GetString(appName + "NavDrawer") == "true")
                buttonAnimator.Play("Expand");
            else if (PlayerPrefs.GetString(appName + "NavDrawer") == "false"
                || !PlayerPrefs.HasKey(appName + "NavDrawer"))
                buttonAnimator.Play("Minimize");
        }
    }
}