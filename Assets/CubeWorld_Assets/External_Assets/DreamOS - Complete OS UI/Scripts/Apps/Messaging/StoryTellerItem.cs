using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Button))]
    public class StoryTellerItem : MonoBehaviour
    {
        public int itemIndex;
        public int layoutIndex;
        public Transform layoutObj;
        public MessagingManager msgManager;

        void Start()
        {
            Button strButton = gameObject.GetComponent<Button>();
            strButton.onClick.AddListener(delegate
            {
                msgManager.storyTellerAnimator.Play("Out");
                msgManager.stItemIndex = itemIndex;
                msgManager.CreateMessage(layoutObj, 0, msgManager.chatList[layoutIndex].chatAsset.storyTeller[msgManager.storyTellerIndex].replies[itemIndex].replyContent);

                if (msgManager.chatList[layoutIndex].chatAsset.storyTeller[msgManager.storyTellerIndex].replies[itemIndex].replyFeedback != "")
                    StartCoroutine(msgManager.StoryTellerLatency(msgManager.chatList[layoutIndex].chatAsset.storyTeller[msgManager.storyTellerIndex].replies[itemIndex].feedbackLatency, layoutIndex, itemIndex));

                msgManager.isStoryTellerOpen = false;

                for (int i = 0; i < msgManager.storytellerReplyEvents.Count; ++i)
                {
                    if (msgManager.storytellerReplyEvents[i].replyID == gameObject.name)
                    {
                        msgManager.storytellerReplyEvents[i].onReplySelect.Invoke();
                        break;
                    }
                }
            });
        }
    }
}