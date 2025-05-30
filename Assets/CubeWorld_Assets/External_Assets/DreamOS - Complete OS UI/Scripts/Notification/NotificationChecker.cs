using UnityEngine;

namespace Michsky.DreamOS
{
    public class NotificationChecker : MonoBehaviour
    {
        [Header("Resources")]
        public PopupPanelManager popupManager;
        public Transform notificationList;
        public Animator closeButton;
        public Animator emptyIndicator;

        int childs;
        bool isCleaned = false;

        void Start()
        {
            if (childs == 0)
                isCleaned = true;
            else
                CountNotifications();
        }

        void Update()
        {
            if (notificationList != null && popupManager.isOn == true)
                CountNotifications();
        }

        public void CountNotifications()
        {
            childs = notificationList.childCount;

            if (childs == 0)
            {
                emptyIndicator.Play("Fade-in");
                closeButton.Play("Fade-out");
                isCleaned = true;
            }

            else if (childs >= 0 && isCleaned == true)
            {
                emptyIndicator.Play("Fade-out");
                closeButton.Play("Fade-in");
            }
        }
    }
}