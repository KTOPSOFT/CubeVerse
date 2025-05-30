using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace AdvancedColorPicker
{
    public class WindowDragger : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public RectTransform windowToDrag;
        public bool clampInsideParentRect = true;

        private Vector2 currentPos;
        private Vector2 lastPos;

        private RectTransform rectTransform
        {
            get
            {
                return transform as RectTransform;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 result;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(windowToDrag.parent as RectTransform, eventData.position, eventData.pressEventCamera, out result))
            {
                currentPos += result - lastPos;
                lastPos = result;
                windowToDrag.anchoredPosition = ClampPosition(currentPos);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowToDrag.parent as RectTransform, eventData.position, eventData.pressEventCamera, out lastPos);
            currentPos = windowToDrag.anchoredPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        private Vector2 ClampPosition(Vector2 position)
        {
            if (!clampInsideParentRect)
                return position;

            Vector2 size = windowToDrag.rect.size;
            size = Vector2.Scale(size, windowToDrag.localScale);
            Rect area = (windowToDrag.parent as RectTransform).rect;

            // Offset center to accomidate to our anchor position
            Vector2 anchorCenter = new Vector2(0.5f, 0.5f) - ((windowToDrag.anchorMin + windowToDrag.anchorMax) * 0.5f);
            area.center = Vector2.Scale(area.size, anchorCenter);

            // Clamp the position to the area of the parent of the WindowTodrag
            return new Vector2(Mathf.Clamp(position.x, area.xMin + (size.x * windowToDrag.pivot.x), area.xMax - (size.x * (1f - windowToDrag.pivot.x))),
                Mathf.Clamp(position.y, area.yMin + (size.y * windowToDrag.pivot.y), area.yMax - (size.y * (1f - windowToDrag.pivot.y))));
        }
    }
}