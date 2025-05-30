//This asset was uploaded by https://unityassetcollection.com

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    public class GameHubInstallationButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Resources")]
        public Animator buttonAnimator;

        [Header("Events")]
        public UnityEvent onClick;
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        void Start()
        {
            if (buttonAnimator == null)
                this.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hover to Normal")
                || buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Out"))
                buttonAnimator.Play("Normal to Hover");

            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("In"))
                buttonAnimator.Play("In to Hover");

            onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Normal to Hover"))
                buttonAnimator.Play("Hover to Normal");

            else if (buttonAnimator.GetCurrentAnimatorStateInfo(0).IsName("In to Hover"))
                buttonAnimator.Play("In to Normal");

            onPointerExit.Invoke();
        }
    }
}