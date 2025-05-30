/* Unity why u do dis :( */

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    public class EventTriggerFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private EventTrigger tempTrigger;
        private bool onEnter;

        void Awake()
        {
#if UNITY_2021_1_OR_NEWER
            // Temp fix due to an engine issue
            tempTrigger = gameObject.GetComponent<EventTrigger>();
            tempTrigger.enabled = false;
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
#if UNITY_2021_1_OR_NEWER
            onEnter = true;
            tempTrigger.OnPointerEnter(eventData);
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if UNITY_2021_1_OR_NEWER
            onEnter = false;

            StopCoroutine("WaitForPointerExit");
            StartCoroutine("WaitForPointerExit", eventData);
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_2021_1_OR_NEWER
            tempTrigger.OnPointerClick(eventData);
#endif
        }

        IEnumerator WaitForPointerExit(PointerEventData eventData)
        {
            yield return new WaitForSeconds(0.1f);
            if (onEnter == false) { tempTrigger.OnPointerExit(eventData); }
        }
    }
}