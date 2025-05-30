using UnityEngine;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Messaging/Storyteller Reply Event")]
    public class StorytellerReplyEvent : MonoBehaviour
    {
        [Header("Settings")]
        public MessagingManager messagingManager;
        public string replyID;

        [Header("Events")]
        public UnityEvent onReplySelect;

        void Awake()
        {
            if (messagingManager == null)
            {
                Debug.LogError("<b>[Storyteller Reply Event]</b> Messaging Manager is missing.", this);
                return;
            }

            MessagingManager.StorytellerReplyEvent item = new MessagingManager.StorytellerReplyEvent();
            item.replyID = replyID;
            item.onReplySelect.AddListener(() => onReplySelect.Invoke());
            messagingManager.storytellerReplyEvents.Add(item);
        }
    }
}