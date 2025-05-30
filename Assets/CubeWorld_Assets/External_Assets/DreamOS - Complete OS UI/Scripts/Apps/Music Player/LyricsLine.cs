using System.Collections;
using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class LyricsLine : MonoBehaviour
    {
        [Header("Resources")]
        public TextMeshProUGUI textObject;
        public CanvasGroup itemCG;
        public RectTransform itemRT;

        [Header("Settings")]
        [Range(0, 1)] public float upcomingAlpha = 0.15f;
        public float transitionMultiplier = 5;
        public float smoothness = 15;
        private float tempY;
        private bool isCurrent;
        private bool isIn;

        public void SetIn()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            tempY = itemRT.sizeDelta.y;
            itemRT.sizeDelta = new Vector2(itemRT.sizeDelta.x, 0);
            itemCG.alpha = 0;
            if (isIn == false) { StartCoroutine("ItemIn"); isIn = true; }
        }

        public void SetCurrent()
        {
            if (isCurrent == true || gameObject.activeInHierarchy == false)
                return;

            StartCoroutine("ItemCurrent");
            isCurrent = true;
        }

        public void SetOut()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            textObject.enableAutoSizing = false;
            StopCoroutine("ItemIn");
            StopCoroutine("ItemCurrent");
            StartCoroutine("ItemOut");
        }

        IEnumerator ItemIn()
        {
            while (itemRT.sizeDelta.y < tempY - 0.1f)
            {
                if (itemCG.alpha < upcomingAlpha) { itemCG.alpha += Time.deltaTime * transitionMultiplier; }
                itemRT.sizeDelta = Vector2.Lerp(itemRT.sizeDelta, new Vector2(itemRT.sizeDelta.x, tempY), Time.deltaTime * smoothness);
                yield return null;
            }

            StopCoroutine("ItemIn");
        }

        IEnumerator ItemCurrent()
        {
            while (itemCG.alpha < 1)
            {
                itemCG.alpha += Time.deltaTime * transitionMultiplier;
                yield return null;
            }

            StopCoroutine("ItemCurrent");
        }

        IEnumerator ItemOut()
        {
            while (itemCG.alpha > 0)
            {
                itemCG.alpha -= Time.deltaTime * transitionMultiplier;
                itemRT.sizeDelta = Vector2.Lerp(itemRT.sizeDelta, new Vector2(itemRT.sizeDelta.x, 0), Time.deltaTime * smoothness);
                yield return null;
            }

            Destroy(gameObject);
            StopCoroutine("ItemOut");
        }
    }
}