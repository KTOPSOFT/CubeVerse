using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace AdvancedColorPicker
{
    public class WindowOrderer : UIBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }
    }
}