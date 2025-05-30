using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    [DisallowMultipleComponent]
    public class ItemDragger : UIBehaviour, IPointerEnterHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Resources")]
        public ItemDragContainer dragContainer;
        private RectTransform dragObject;

        [Header("Settings")]
        public bool rememberPosition = false;
        public string saveKey = "Desktop";

        private Vector2 originalLocalPointerPosition;
        private Vector3 originalPanelLocalPosition;

        private RectTransform dragObjectInternal
        {
            get
            {
                if (dragObject == null) { return (transform as RectTransform); }
                else { return dragObject; }
            }
        }

        private RectTransform dragAreaInternal
        {
            get
            {
                if (dragContainer.dragBorder != null)
                {
                    RectTransform newArea = transform.parent as RectTransform;
                    return newArea;
                }

                else { return dragContainer.dragBorder; }
            }
        }

        public new void Start()
        {
            if (dragContainer == null)
            {
                if (gameObject.GetComponentInParent<ItemDragContainer>() == null)
                    transform.parent.gameObject.AddComponent<ItemDragContainer>();

                dragContainer = gameObject.GetComponentInParent<ItemDragContainer>();
            }

            if (rememberPosition == true && dragContainer.dragMode == ItemDragContainer.DragMode.Snapped
                && PlayerPrefs.HasKey(gameObject.name + saveKey + "DraggerIndex"))
            {
                transform.SetSiblingIndex(PlayerPrefs.GetInt(gameObject.name + saveKey + "DraggerIndex"));
                return;
            }

            else if (rememberPosition == true && dragContainer.dragMode == ItemDragContainer.DragMode.Free
                && !PlayerPrefs.HasKey(gameObject.name + saveKey + "XPos"))
            {
                PlayerPrefs.SetFloat(gameObject.name + saveKey + "XPos", transform.position.x);
                PlayerPrefs.SetFloat(gameObject.name + saveKey + "YPos", transform.position.y);
            }

            if (dragContainer.gridLayoutGroup != null) { dragContainer.gridLayoutGroup.enabled = true; }
            if (rememberPosition == true) { UpdateObject(); }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Right)
                return;

            if (dragContainer.dragMode == ItemDragContainer.DragMode.Snapped) { dragContainer.objectBeingDragged = this.gameObject; }
            else
            {
                this.transform.SetAsLastSibling();
                originalPanelLocalPosition = dragObjectInternal.localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            }
        }

        public void OnDrag(PointerEventData data) 
        {
            if (dragContainer.dragMode == ItemDragContainer.DragMode.Free)
            {
                Vector2 localPointerPosition;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaInternal, data.position, data.pressEventCamera, out localPointerPosition))
                {
                    Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                    dragObjectInternal.localPosition = originalPanelLocalPosition + offsetToOriginal;
                }

                ClampToArea();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragContainer.dragMode == ItemDragContainer.DragMode.Snapped)
            {
                if (dragContainer.objectBeingDragged == this.gameObject)
                    dragContainer.objectBeingDragged = null;

                if (rememberPosition == true)
                    PlayerPrefs.SetInt(gameObject.name + saveKey + "DraggerIndex", transform.GetSiblingIndex());
            }

            else if (dragContainer.dragMode == ItemDragContainer.DragMode.Free && rememberPosition == true)
            {
                PlayerPrefs.SetFloat(gameObject.name + saveKey + "XPos", transform.position.x);
                PlayerPrefs.SetFloat(gameObject.name + saveKey + "YPos", transform.position.y);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (dragContainer.dragMode != ItemDragContainer.DragMode.Snapped)
                return;

            GameObject objectBeingDragged = dragContainer.objectBeingDragged;
            
            if (objectBeingDragged != null && objectBeingDragged != this.gameObject)
                objectBeingDragged.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
        }

        public void ClampToArea()
        {
            Vector3 pos = dragObjectInternal.localPosition;
            Vector3 minPosition = dragAreaInternal.rect.min - dragObjectInternal.rect.min;
            Vector3 maxPosition = dragAreaInternal.rect.max - dragObjectInternal.rect.max;
            pos.x = Mathf.Clamp(dragObjectInternal.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(dragObjectInternal.localPosition.y, minPosition.y, maxPosition.y);
            dragObjectInternal.localPosition = pos;
        }

        public void UpdateObject()
        {
            if (dragContainer.gridLayoutGroup != null && dragContainer.gridLayoutGroup.enabled == true)
                dragContainer.gridLayoutGroup.enabled = false;

            if (!PlayerPrefs.HasKey(gameObject.name + saveKey + "XPos"))
                UpdateData();

            float x = PlayerPrefs.GetFloat(gameObject.name + saveKey + "XPos");
            float y = PlayerPrefs.GetFloat(gameObject.name + saveKey + "YPos");
            Vector3 tempPos = new Vector3(x, y, 0);
            transform.position = tempPos;
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(dragContainer.gridLayoutGroup.cellSize.x, dragContainer.gridLayoutGroup.cellSize.y);
        }

        public void UpdateData()
        {
            PlayerPrefs.SetFloat(gameObject.name + saveKey + "XPos", transform.position.x);
            PlayerPrefs.SetFloat(gameObject.name + saveKey + "YPos", transform.position.y);
        }

        public void RemoveData()
        {
            PlayerPrefs.DeleteKey(gameObject.name + saveKey + "XPos");
            PlayerPrefs.DeleteKey(gameObject.name + saveKey + "YPos");
            PlayerPrefs.DeleteKey(gameObject.name + saveKey + "DraggerIndex");
        }
    }
}