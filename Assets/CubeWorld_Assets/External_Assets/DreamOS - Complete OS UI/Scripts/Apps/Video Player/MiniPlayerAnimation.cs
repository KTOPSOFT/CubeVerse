using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    public class MiniPlayerAnimation : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Resources")]
        public Animator playerAnimator;

        [Header("Events")]
        public UnityEvent onClickEvents;

        void Start()
        {
            if (playerAnimator == null)
                playerAnimator = gameObject.GetComponent<Animator>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            playerAnimator.Play("Out");
            onClickEvents.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("In")
                || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover Out"))
                playerAnimator.Play("Hover In");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover In"))
                playerAnimator.Play("Hover Out");
        }
    }
}