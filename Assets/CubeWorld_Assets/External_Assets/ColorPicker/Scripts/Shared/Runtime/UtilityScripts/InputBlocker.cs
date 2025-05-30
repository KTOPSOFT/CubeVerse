using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace AdvancedColorPicker
{
    /// <summary>
    /// A helper class to create input blockers. 
    /// </summary>
    public static class InputBlocker
    {
        /// <summary>
        /// Creates a blocker that covers the entire screen.
        /// </summary>
        /// <param name="onClosed">The callback which is called whenever the blocker is clicked and closed</param>
        public static void CreateBlocker(UnityAction onClosed)
        {
            CreateBlocker(onClosed, 32767);
        }

        /// <summary>
        /// Creates a blocker that covers the entire screen.
        /// </summary>
        /// <param name="onClosed">The callback which is called whenever the blocker is clicked and closed</param>
        /// <param name="sortingOrder">The SortingOrder for the blocker canvas, this value should be higher then any canvas that shouldn't receive input and lower then any canvas that should receive input</param>
        public static void CreateBlocker(UnityAction onClosed, int sortingOrder)
        {
            CreateBlocker(onClosed, sortingOrder, 0);
        }

        /// <summary>
        /// Creates a blocker that covers the entire screen.
        /// </summary>
        /// <param name="onClosed">The callback which is called whenever the blocker is clicked and closed</param>
        /// <param name="sortingOrder">The SortingOrder for the blocker canvas, this value should be higher then any canvas that shouldn't receive input and lower then any canvas that should receive input</param>
        /// <param name="sortingLayerID">The layerID for the blocker canvas</param>
        public static void CreateBlocker(UnityAction onClosed, int sortingOrder, int sortingLayerID)
        {
            GameObject go = new GameObject("BlockerCanvas");
            Canvas canvas = go.AddComponent<Canvas>();
            go.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingLayerID = sortingLayerID;
            canvas.sortingOrder = sortingOrder;

            GameObject blocker = new GameObject("Blocker");

            RectTransform transform = blocker.AddComponent<RectTransform>();
            transform.SetParent(go.transform, false);
            transform.anchorMin = Vector3.zero;
            transform.anchorMax = Vector3.one;
            transform.offsetMin = transform.offsetMax = Vector2.zero;

            Image image = blocker.AddComponent<Image>();
            image.color = Color.clear;

            Button button = blocker.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                UnityEngine.Object.Destroy(go);
                onClosed();
            });
        }

        /// <summary>
        /// Creates a blocker which will cover the entirety of parent and any other childs of parent will be in front of the blocker, still receiving input.
        /// </summary>
        /// <param name="parent">The parent of which this blocker will be a child</param>
        /// <param name="onClosed">The callback which is called whenever the blocker is clicked and closed</param>
        /// <returns></returns>
        public static GameObject CreateBlocker(RectTransform parent, UnityAction onClosed)
        {
            GameObject blocker = new GameObject("blocker");
            RectTransform rectTransform = blocker.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
            rectTransform.SetAsFirstSibling();

            Image image = blocker.AddComponent<Image>();
            image.color = Color.clear;

            Button button = blocker.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                UnityEngine.Object.Destroy(blocker);
                onClosed();
            });

            return blocker;
        }
    }
}