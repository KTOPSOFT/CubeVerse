using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class FloatingIconManager : MonoBehaviour
    {
        [Header("Resources")]
        public Camera cameraObject;
        public Transform iconParent;
        public GameObject iconObject;
        public Sprite iconSprite;
        public Renderer objectRenderer;

        [Header("Settings")]
        public float fadingMultiplier = 0.1f;
        public string playerTag = "Player";

        [HideInInspector] public bool enableUpdating;
        [HideInInspector] public GameObject iconObj;
        Image iconImgObj;
        CanvasGroup iconObjCG;
        Vector3 velocity = Vector3.zero;
        float smooth;

        void Awake()
        {
            if (cameraObject == null) { cameraObject = Camera.main; }
            CreateIcon();
        }

        void OnDestroy()
        {
            Destroy(iconObj);
        }

        void Update()
        {
            if (enableUpdating == true && objectRenderer != null && objectRenderer.isVisible == false)
            {
                iconObjCG.alpha = 0;
                return;
            }

            if (enableUpdating == true)
            {
                Vector3 screenPos = cameraObject.WorldToScreenPoint(gameObject.transform.position);
                iconObj.transform.position = Vector3.SmoothDamp(iconObj.transform.position, screenPos, ref velocity, Time.deltaTime * smooth);

                if (iconObjCG.alpha != 1)
                    iconObjCG.alpha += fadingMultiplier;
            }

            else if (enableUpdating == false && iconObjCG.alpha != 0)
            {
                Vector3 screenPos = cameraObject.WorldToScreenPoint(gameObject.transform.position);
                iconObj.transform.position = Vector3.SmoothDamp(iconObj.transform.position, screenPos, ref velocity, Time.deltaTime * smooth);
                iconObjCG.alpha -= fadingMultiplier;
            }
        }

        public void CreateIcon()
        {
            iconObj = Instantiate(iconObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            iconObj.name = iconObject.name;
            iconObj.transform.SetParent(iconParent, false);
            iconImgObj = iconObj.transform.Find("Icon").GetComponent<Image>();
            iconImgObj.sprite = iconSprite;
            iconObjCG = iconObj.GetComponent<CanvasGroup>();
            iconObjCG.alpha = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == playerTag)
                enableUpdating = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == playerTag)
                enableUpdating = false;
        }
    }
}