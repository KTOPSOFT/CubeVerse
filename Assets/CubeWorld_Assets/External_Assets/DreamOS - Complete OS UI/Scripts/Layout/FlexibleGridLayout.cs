using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class FlexibleGridLayout : LayoutGroup
    {
        [Header("Layout")]
        public Vector2 spacing;
        public Vector2 cellSize = new Vector2(100, 100);
        public int rows;
        public int columns;

        [Header("Settings")]
        public FitType fitType;
        public bool fitX;
        public bool fitY;

        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRow,
            FixedColumn
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                fitX = true;
                fitY = true;

                float sqrRt = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrRt);
                columns = Mathf.CeilToInt(sqrRt);
            }

            if (fitType == FitType.Width || fitType == FitType.FixedColumn)
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            else if (fitType == FitType.Height || fitType == FitType.FixedRow)
                rows = Mathf.CeilToInt(transform.childCount / (float)rows);

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth / (float)columns) - ((spacing.x / (float)columns) * 2) - (padding.left / (float)columns) - (padding.right / (float)columns);
            float cellHeight = (parentHeight / (float)rows) - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
                var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        public override void CalculateLayoutInputVertical() { this.enabled = false; this.enabled = true; }

        public override void SetLayoutHorizontal() { }

        public override void SetLayoutVertical() { }
    }
}