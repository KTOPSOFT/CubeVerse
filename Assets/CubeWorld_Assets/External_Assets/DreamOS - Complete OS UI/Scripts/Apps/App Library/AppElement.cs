using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("DreamOS/Apps/App Element")]
    public class AppElement : MonoBehaviour
    {
        // Resources
        public AppLibrary appLibrary;

        // Settings
        public string appID;
        public ElementType elementType;
        public IconSize iconSize;
        public bool useGradient = true;

        public enum ElementType { None, Icon, Title, Gradient }
        public enum IconSize { Small, Medium, Big }

        public int tempAppIndex;
        UIGradient imageGradient;
        Image imageObject;
        TextMeshProUGUI textObject;

        void Awake()
        {
            try
            {
                if (appLibrary == null)
                    appLibrary = Resources.Load<AppLibrary>("Apps/App Library");

                if (elementType == ElementType.Icon && imageObject == null)
                {
                    imageObject = gameObject.GetComponent<Image>();
                    if (useGradient == true) { imageGradient = gameObject.GetComponent<UIGradient>(); }
                }

                else if (elementType == ElementType.Gradient && imageGradient == null)
                    imageGradient = gameObject.GetComponent<UIGradient>();
                else if (elementType == ElementType.Title && textObject == null)
                    textObject = gameObject.GetComponent<TextMeshProUGUI>();


                UpdateLibrary();
                UpdateElement();
            }

            catch { Debug.LogWarning("<b>App Library</b> is missing but it should be assigned.", this); }
        }

        void Update()
        {
            if (appLibrary != null && appLibrary.alwaysUpdate == true) { UpdateElement(); }
            if (Application.isPlaying == true && appLibrary.optimizeUpdates == true) { this.enabled = false; }
        }

        public void UpdateLibrary()
        {
            for (int i = 0; i < appLibrary.apps.Count; i++)
            {
                if (appID == appLibrary.apps[i].appTitle) { tempAppIndex = i; break; }
            }

            this.enabled = true;
        }

        public void UpdateElement()
        {
            if (tempAppIndex >= appLibrary.apps.Count || appLibrary.apps[tempAppIndex].appTitle != appID)
            {
                // Debug.LogWarning("<b>[App Element]</b> Specified app item was not found in the library. Please double check the App ID.", this);
                return;
            }

            if (elementType == ElementType.Icon && imageObject != null)
            {
                if (iconSize == IconSize.Small) { imageObject.sprite = appLibrary.apps[tempAppIndex].appIconSmall; }
                else if (iconSize == IconSize.Medium) { imageObject.sprite = appLibrary.apps[tempAppIndex].appIconMedium; }
                else if (iconSize == IconSize.Big) { imageObject.sprite = appLibrary.apps[tempAppIndex].appIconBig; }

                if (useGradient == true && imageGradient != null)
                {
                    imageGradient.color1 = appLibrary.apps[tempAppIndex].gradientLeft;
                    imageGradient.color2 = appLibrary.apps[tempAppIndex].gradientRight;
                    imageGradient.enabled = false;
                    imageGradient.enabled = true;
                }
            }

            else if (elementType == ElementType.Gradient && imageGradient != null)
            {
                imageGradient.color1 = appLibrary.apps[tempAppIndex].gradientLeft;
                imageGradient.color2 = appLibrary.apps[tempAppIndex].gradientRight;
                imageGradient.enabled = false;
                imageGradient.enabled = true;
            }

            else if (elementType == ElementType.Title && textObject != null)
                textObject.text = appLibrary.apps[tempAppIndex].appTitle;
        }
    }
}