using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    [DisallowMultipleComponent]
    public class ItemDragContainer : MonoBehaviour
    {
        [Header("Resources")]
        public RectTransform dragBorder;
        [HideInInspector] public GridLayoutGroup gridLayoutGroup;
        [HideInInspector] public HorizontalLayoutGroup horLayoutGroup;
        [HideInInspector] public VerticalLayoutGroup verLayoutGroup;

        [Header("Settings")]
        public PreferredLayout preferredLayout = PreferredLayout.Grid;
        public DragMode dragMode = DragMode.Free;
      
        public enum DragMode { Snapped, Free }
        public enum PreferredLayout { Grid, Horizontal, Vertical }

        public GameObject objectBeingDragged { get; set; }
        [HideInInspector] public bool allowUpdate = false;

        void Awake()
        {
            objectBeingDragged = null;

            if (preferredLayout == PreferredLayout.Grid)
                gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
            else if (preferredLayout == PreferredLayout.Horizontal)
                horLayoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>();
            else if (preferredLayout == PreferredLayout.Vertical)
                verLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>();

            if (dragBorder == null)
                dragBorder = gameObject.GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            StartCoroutine("LatencyHelper");
        }

        public void FreeDragMode() 
        {
            dragMode = DragMode.Free;

            if (allowUpdate == true)
                gridLayoutGroup.enabled = false;
        }

        public void SnappedDragMode() 
        { 
            dragMode = DragMode.Snapped;

            if (allowUpdate == true)
                gridLayoutGroup.enabled = true;
        }

        public void UpdateDragMode()
        {
            if (dragMode == DragMode.Free) { gridLayoutGroup.enabled = false; }
            else { gridLayoutGroup.enabled = true; }
        }

        IEnumerator LatencyHelper()
        {
            yield return new WaitForSeconds(0.5f);
            if (gridLayoutGroup != null) { UpdateDragMode(); allowUpdate = true; }
        }
    }
}