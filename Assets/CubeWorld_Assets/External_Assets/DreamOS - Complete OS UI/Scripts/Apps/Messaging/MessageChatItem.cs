using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class MessageChatItem : MonoBehaviour
    {
        [Header("Resources")]
        public GameObject notificationBadge;
        public TextMeshProUGUI latestMessage;
        public TextMeshProUGUI timeLabel;

        public void EnableNotificationBadge(bool value)
        {
            if (notificationBadge == null)
                return;

            if (value == true) { notificationBadge.SetActive(true); }
            else { notificationBadge.SetActive(false); }
        }

        public void UpdateLatestMessage(string newText, string time)
        {
            latestMessage.text = newText;
            timeLabel.text = time;
        }
    }
}