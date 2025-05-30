using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace AdvancedColorPicker
{
    public class PopupWindow : MonoBehaviour
    {
        public bool createBlocker;
        private GameObject blocker;

        private RectTransform rectTransform
        {
            get
            {
                return transform as RectTransform;
            }
        }

        public void Close()
        {
            Destroy(gameObject);
            if (blocker != null)
                Destroy(blocker);
        }

        public static T Open<T>(RectTransform container, RectTransform around, T prefab) where T : PopupWindow
        {
            // temporary around position
            RectTransform aroundPos = new GameObject("Around").AddComponent<RectTransform>();
            aroundPos.SetParent(around, false);
            aroundPos.anchorMin = Vector2.zero;
            aroundPos.anchorMax = Vector2.one;
            aroundPos.offsetMin = aroundPos.offsetMax = Vector2.zero;
            aroundPos.SetParent(container, true);

            Vector2 offset = new Vector2(container.rect.size.x * container.pivot.x, container.rect.size.y * container.pivot.y);

            Vector2 arPosition = aroundPos.anchoredPosition + offset;

            // Calculate corner positions (local)
            Vector2 size;
            size = -aroundPos.rect.size;
            Vector2 leftBot = arPosition + (Vector2)(aroundPos.localRotation * (Vector3)Vector2.Scale(Vector2.Scale(size, aroundPos.pivot), aroundPos.localScale));
            size = new Vector2(-aroundPos.rect.size.x, aroundPos.rect.size.y);
            Vector2 leftTop = arPosition + (Vector2)(aroundPos.localRotation * (Vector3)Vector2.Scale(Vector2.Scale(size, aroundPos.pivot), aroundPos.localScale));
            size = aroundPos.rect.size;
            Vector2 rightTop = arPosition + (Vector2)(aroundPos.localRotation * (Vector3)Vector2.Scale(Vector2.Scale(size, Vector2.one - aroundPos.pivot), aroundPos.localScale));
            size = new Vector2(aroundPos.rect.size.x, -aroundPos.rect.size.y);
            Vector2 rightBot = arPosition + (Vector2)(aroundPos.localRotation * (Vector3)Vector2.Scale(Vector2.Scale(size, aroundPos.pivot), aroundPos.localScale));

            // Destroy temporary RectTransform
            Destroy(aroundPos.gameObject);

            // Calculate min/max
            Vector2 min = new Vector2(Mathf.Min(leftBot.x, leftTop.x, rightTop.x, rightBot.x), Mathf.Min(leftBot.y, leftTop.y, rightTop.y, rightBot.y));
            Vector2 max = new Vector2(Mathf.Max(leftBot.x, leftTop.x, rightTop.x, rightBot.x), Mathf.Max(leftBot.y, leftTop.y, rightTop.y, rightBot.y));

            // Calculte amount of pixels left on each side
            float left = min.x;
            float top = container.rect.height - max.y;
            float right = container.rect.width - max.x;
            float bot = min.y;

            // Calculate best side
            List<OptimalSide> sides = new List<OptimalSide>();
            sides.Add(new OptimalSide(Side.Left, new Vector2(left, container.rect.height)));
            sides.Add(new OptimalSide(Side.Above, new Vector2(container.rect.width, top)));
            sides.Add(new OptimalSide(Side.Right, new Vector2(right, container.rect.height)));
            sides.Add(new OptimalSide(Side.Below, new Vector2(container.rect.width, bot)));
            sides.Sort(new SideSorter(prefab.rectTransform.rect.size));

            // Instantiate Popup
            T instance = Instantiate(prefab);
            instance.transform.SetParent(container, false);
            RectTransform instanceTransform = instance.rectTransform;
            instanceTransform.anchorMin = instanceTransform.anchorMax = Vector2.zero;
            instanceTransform.pivot = new Vector2(0.5f, 0.5f);
            instanceTransform.anchoredPosition = Vector2.Lerp(min, max, 0.5f);
            Vector2 halfRange = (max - min) * 0.5f;
            Vector2 halfSize = prefab.rectTransform.rect.size * 0.5f;
            switch (sides[0].side)
            {
                case Side.Left:
                    instanceTransform.anchoredPosition -= new Vector2(halfRange.x, 0);
                    instanceTransform.anchoredPosition -= new Vector2(halfSize.x, 0);
                    break;
                case Side.Above:
                    instanceTransform.anchoredPosition += new Vector2(0, halfRange.y);
                    instanceTransform.anchoredPosition += new Vector2(0, halfSize.y);
                    break;
                case Side.Right:
                    instanceTransform.anchoredPosition += new Vector2(halfRange.x, 0);
                    instanceTransform.anchoredPosition += new Vector2(halfSize.x, 0);
                    break;
                case Side.Below:
                    instanceTransform.anchoredPosition -= new Vector2(0, halfRange.y);
                    instanceTransform.anchoredPosition -= new Vector2(0, halfSize.y);
                    break;
                default:
                    break;
            }


            // Clamp to parent
            instanceTransform.anchoredPosition = new Vector2(Mathf.Clamp(instanceTransform.anchoredPosition.x, halfSize.x, container.rect.size.x - halfSize.x),
                Mathf.Clamp(instanceTransform.anchoredPosition.y, halfSize.y, container.rect.size.y - halfSize.y));

            if (instance.createBlocker)
            {
                instance.blocker = InputBlocker.CreateBlocker(container, instance.Close);
            }

            return instance;
        }


        /// <summary>
        /// Sorts OptimalSides to make sure the window opens on the largest side
        /// </summary>
        private class SideSorter : IComparer<OptimalSide>
        {
            private Vector2 requiredSize;

            public SideSorter(Vector2 requiredSize)
            {
                this.requiredSize = requiredSize;
            }

            public int Compare(OptimalSide one, OptimalSide two)
            {
                Vector2 oneLeft = one.size - requiredSize;
                Vector2 twoLeft = two.size - requiredSize;

                Vector2 difference = twoLeft - oneLeft;

                bool xFits = oneLeft.x > 0 && oneLeft.y > 0;
                bool yFits = twoLeft.x > 0 && twoLeft.y > 0;

                if (xFits && !yFits)
                {
                    return -1;
                }
                else if (yFits && !xFits)
                {
                    return 1;
                }
                else if (xFits && yFits)
                {
                    float diff = difference.x + difference.y;
                    return Mathf.Approximately(diff, 0) ? 0 : diff > 0 ? 1 : -1;
                }
                else
                {
                    Vector2 shortagex = new Vector2(Mathf.Min(0, oneLeft.x), Mathf.Min(0, oneLeft.y));
                    Vector2 shortagey = new Vector2(Mathf.Min(0, twoLeft.x), Mathf.Min(0, twoLeft.y));
                    float shortageOne = shortagex.x + shortagex.y;
                    float shortageTwo = shortagey.x + shortagey.y;
                    float shortage = shortageTwo - shortageOne;
                    return Mathf.Approximately(shortage, 0) ? 0 : shortage > 0 ? 1 : -1;
                }
            }
        }

        private class OptimalSide
        {
            public Side side;
            public Vector2 size;

            public OptimalSide(Side side, Vector2 size)
            {
                this.side = side;
                this.size = size;
            }

            public override string ToString()
            {
                return side + " : " + size;
            }
        }

        public enum Side
        {
            Left,
            Above,
            Right,
            Below
        }
    }
}