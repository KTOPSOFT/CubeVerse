using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    public class WindowResize : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        [Header("Resources")]
        public RectTransform resizeObject;

        [Header("Settings")]
        public Vector2 minSize;
        public Vector2 maxSize;

        private Vector2 currentPointerPosition;
        private Vector2 previousPointerPosition;
        Vector2 sizeDelta;
        Vector2 resizeValue;

        public void OnPointerDown(PointerEventData data)
        {
            resizeObject.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(resizeObject, data.position, data.pressEventCamera, out previousPointerPosition);
        }

        public void OnDrag(PointerEventData data)
        {
            if (resizeObject == null)
                return;

            sizeDelta = resizeObject.sizeDelta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(resizeObject, data.position, data.pressEventCamera, out currentPointerPosition);
            resizeValue = currentPointerPosition - previousPointerPosition;

            sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
            sizeDelta = new Vector2(
                Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
                );

            resizeObject.sizeDelta = sizeDelta;
            previousPointerPosition = currentPointerPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Transform parentObj = transform.parent;
            WindowDragger wd = parentObj.GetComponentInChildren<WindowDragger>();
            wd.ClampToArea();
        }
    }
}