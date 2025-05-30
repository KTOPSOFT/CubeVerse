using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class ImageMessage : MonoBehaviour
    {
        [Header("Resources")]
        public Image imageObject;
        public Button viewButton;

        // Hidden vars
        [HideInInspector] public PhotoGalleryManager pgm;
        [HideInInspector] public WindowManager pgmwm;
        [HideInInspector] public Sprite spriteVar;
        [HideInInspector] public string title;
        [HideInInspector] public string description;

        void Start()
        {
            try
            {
                var pgmObj = (PhotoGalleryManager)GameObject.FindObjectsOfType(typeof(PhotoGalleryManager))[0];
                pgm = pgmObj;
                pgmwm = pgm.wManager.gameObject.GetComponent<WindowManager>();
            }

            catch { return; }

            viewButton.onClick.AddListener(delegate 
            {
                pgmwm.OpenWindow();
                pgm.wManager.OpenPanel(pgm.viewerPanelName);
                pgm.OpenCustomSprite(spriteVar, title, description); 
            });
        }
    }
}