using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(AudioSource))]
    public class NotificationManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private Animator notificationListRect;
        public Transform notificationParent;
        public Transform popupNotificationParent;
        public GameObject notificationButton;

        // Desktop
        public GameObject popupNotification;
        public GameObject standardNotification;

        [HideInInspector] public Image popupIcon;
        [HideInInspector] public TextMeshProUGUI popupTitle;
        [HideInInspector] public TextMeshProUGUI popupDescription;

        // Popup
        [Range(0.1f, 3)] public float visibleTime = 0.5f;

        // Standard
        [HideInInspector] public Image standardIcon;
        [HideInInspector] public Image standardHeader;
        [HideInInspector] public TextMeshProUGUI standardTitle;
        [HideInInspector] public TextMeshProUGUI standardDescription;
        [HideInInspector] public Transform standardButtonParent;

        void Awake()
        {
            foreach (Transform child in notificationParent) { Destroy(child.gameObject); }
            foreach (Transform child in popupNotificationParent) { Destroy(child.gameObject); }
        }

        public void ClearNotificationList()
        {
            notificationListRect.Play("Clear");
        }
    }
}