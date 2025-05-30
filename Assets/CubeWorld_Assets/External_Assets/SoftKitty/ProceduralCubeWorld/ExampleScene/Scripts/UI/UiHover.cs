using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SoftKitty.PCW.Demo
{
    public class UiHover : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        [HideInInspector]
        public bool isHover = false;
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
        }

        private void OnDisable()
        {
            isHover = false;
        }

    }
}
