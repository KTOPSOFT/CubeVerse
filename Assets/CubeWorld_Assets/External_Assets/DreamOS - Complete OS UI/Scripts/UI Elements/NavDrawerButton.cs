using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    public class NavDrawerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Resources")]
        public Animator buttonAnimator;

        [Header("Events")]
        public UnityEvent onClickEvents;

        void Awake()
        {
            if (buttonAnimator == null) { buttonAnimator = gameObject.GetComponent<Animator>(); }
        }

        public void ClickButton()
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inactive to Highlighted")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active to Highlighted"))
                buttonAnimator.Play("Highlighted to Active");
            else
                buttonAnimator.Play("Closed to Active");

            onClickEvents.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClickEvents.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Closed")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Open to Closed"))
                buttonAnimator.Play("Closed to Highlighted");

            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed to Open")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted to Open"))
                buttonAnimator.Play("Open to Highlighted");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Closed to Highlighted"))
                buttonAnimator.Play("Highlighted to Closed");
            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Open to Highlighted"))
                buttonAnimator.Play("Highlighted to Open");
        }
    }
}