using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Animator))]
    public class QuickCenterButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Resources")]
        public Animator buttonAnimator;

        [Header("Settings")]
        public float smoothness = 0.2f;

        void OnEnable()
        {
            buttonAnimator.CrossFade("Out", smoothness);
            StartCoroutine("DisableAnimator");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StopCoroutine("DisableAnimator");
            buttonAnimator.enabled = true;
            buttonAnimator.CrossFade("In", smoothness);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonAnimator.CrossFade("Out", smoothness);
            StartCoroutine("DisableAnimator");
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(1);
            buttonAnimator.enabled = false;
        }
    }
}