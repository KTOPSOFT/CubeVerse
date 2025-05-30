using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ColorEyedropperPreview : MaskableGraphic
    {
        public enum EyedropperPreviewType
        {
            PixelSize,
            PixelAmountHorizontal,
            PixelAmountVertical
        }

        [SerializeField]
        private EyedropperPreviewType type;

        [SerializeField]
        private float pixelSize = 8f;

        [SerializeField]
        private float borderSize = 1f;

        [SerializeField]
        private float horizontalPixels;

        [SerializeField]
        private float verticalPixels;

        [SerializeField]
        private Color32 selectionBoxColor = new Color32(200, 200, 200, 255);

        [SerializeField]
        private bool activated;
        private Coroutine coroutine;

        private Color32[] pixelsToDisplay = new Color32[0];

        private Color32 InactiveColor
        {
            get
            {
                return new Color32(154, 154, 154, 255);
            }
        }

        public EyedropperPreviewType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "eyedropperType");
#endif

                type = value;
                CalculateOtherValues();
            }
        }

        /// <summary>
        /// Gets or sets the size of each pixel in pixels. Could also be described as zoom level.
        /// If set, HorizontalPixes and VertcalPixels will be calculated based on this value and the Recttransform's dimensions.
        /// </summary>
        public float PixelSize
        {
            get
            {
                return pixelSize;
            }
            set
            {
                value = Mathf.Max(2, value);

                if (pixelSize == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "eyedropperPixelSize");
#endif

                type = EyedropperPreviewType.PixelSize;
                pixelSize = value;

                CalculateOtherValues();
            }
        }

        /// <summary>
        /// Gets or sets the size of the borders in pixels.
        /// </summary>
        public float BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                value = Mathf.Max(0, value);

                if (borderSize == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "eyedropperBorderSize");
#endif

                borderSize = value;
                CalculateOtherValues();
            }
        }

        /// <summary>
        /// Gets or sets the amount of pixels that are both left and right of the center pixel. Meaning that the total amount of horizontal pixels is: "1 + (HorizontalPixels * 2)"
        /// If set, the PixelSize and VerticalPixels are calculated depending on this value and the Recttransform's dimensions.
        /// </summary>
        public float HorizontalPixels
        {
            get
            {
                return horizontalPixels;
            }
            set
            {
                value = Mathf.Max(0, value);

                if (horizontalPixels == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "eyedropperHorizontal");
#endif

                type = EyedropperPreviewType.PixelAmountHorizontal;
                horizontalPixels = value;

                CalculateOtherValues();
            }
        }

        /// <summary>
        /// Gets or sets the amount of pixels that are above and under the center pixel. Meaning that the total amount of vertical pixels is: "1 + (VerticalPixels * 2)"
        /// If set, the PixelSize and HorizontalPixels are calculated depending on this value and the Recttransform's dimensions.
        /// </summary>
        public float VerticalPixels
        {
            get
            {
                return verticalPixels;
            }
            set
            {
                value = Mathf.Max(0, value);
                if (verticalPixels == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "eyedropperVertical");
#endif

                type = EyedropperPreviewType.PixelAmountVertical;
                verticalPixels = value;

                CalculateOtherValues();
            }
        }

        /// <summary>
        /// Gets the amount of vertices this component expects to use.
        /// </summary>
        public int ExpectedVertices
        {
            get
            {
                int xSize = (Mathf.CeilToInt(horizontalPixels) * 2) + 1;
                int ySize = (Mathf.CeilToInt(verticalPixels) * 2) + 1;

                // 8 for background and selection box
                return 8 + ((xSize * ySize) * 4);

            }
        }

        public Color32 SelectionBoxColor
        {
            get
            {
                return selectionBoxColor;
            }
            set
            {
                if (selectionBoxColor.r == value.r && selectionBoxColor.g == value.b && selectionBoxColor.b == value.b && selectionBoxColor.a == value.a)
                    return;

                selectionBoxColor = value;
                SetVerticesDirty();
            }
        }

        public bool Activated
        {
            get
            {
                return activated;
            }
            set
            {
                if (activated == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "EyedropperPreviewActive");
#endif
                activated = value;
                SetVerticesDirty();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateOtherValues();
#if UNITY_EDITOR
            if (Application.isPlaying)
                coroutine = StartCoroutine(ReadScreenColor());
#else
        coroutine = StartCoroutine(ReadScreenColor());
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        private void CalculateOtherValues()
        {
            switch (type)
            {
                case EyedropperPreviewType.PixelSize:
                    horizontalPixels = ((rectTransform.rect.width / (pixelSize + borderSize)) - 1) * 0.5f;
                    verticalPixels = ((rectTransform.rect.height / (pixelSize + borderSize)) - 1) * 0.5f;
                    break;
                case EyedropperPreviewType.PixelAmountHorizontal:
                    pixelSize = (rectTransform.rect.width / ((horizontalPixels * 2) + 1)) - borderSize;
                    verticalPixels = ((rectTransform.rect.height / (pixelSize + borderSize)) - 1) * 0.5f;
                    break;
                case EyedropperPreviewType.PixelAmountVertical:
                    pixelSize = (rectTransform.rect.height / ((verticalPixels * 2) + 1)) - borderSize;
                    horizontalPixels = ((rectTransform.rect.width / (pixelSize + borderSize)) - 1) * 0.5f;
                    break;
                default:
                    break;

            }
            SetVerticesDirty();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Adjust the scale of the Graphic to make it pixel-perfect.
        /// </summary>
        public override void SetNativeSize()
        {
            Texture tex = mainTexture;
            if (tex != null)
            {
                Rect uvRect = new Rect(Vector2.zero, rectTransform.rect.size);
                int w = Mathf.RoundToInt(tex.width * uvRect.width);
                int h = Mathf.RoundToInt(tex.height * uvRect.height);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            CalculateOtherValues();
        }

        private IEnumerator ReadScreenColor()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (isActiveAndEnabled && activated)
                {
                    int width = Mathf.CeilToInt(horizontalPixels);
                    int height = Mathf.CeilToInt(verticalPixels);

                    int xmin = Mathf.FloorToInt(Input.mousePosition.x) - width;
                    int ymin = Mathf.FloorToInt(Input.mousePosition.y) - height;
                    int totalWidth = 1 + (width * 2);
                    int totalHeight = 1 + (height * 2);

                    int xOffset = Mathf.Max(0, -xmin);
                    int yOffset = Mathf.Max(0, -ymin);

                    int excessX = Mathf.Max(0, -(Screen.width - (xmin + totalWidth)));
                    int excessY = Mathf.Max(0, -(Screen.height - (ymin + totalHeight)));

                    Texture2D texture = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);
                    texture.hideFlags = HideFlags.HideAndDontSave;

                    Rect rect = new Rect(xmin + xOffset, ymin + yOffset, (totalWidth - xOffset) - excessX, (totalHeight - yOffset) - excessY);

                    if (rect.width > 0 && rect.height > 0)
                    {
                        texture.ReadPixels(rect, xOffset, yOffset);
                        texture.Apply();
                    }
                    pixelsToDisplay = texture.GetPixels32();
                    DestroyImmediate(texture);

                    // Set vertices dirty each frame while activated. To make sure moving objects are dislayed correctly at any time
                    SetVerticesDirty();
                }
            }
        }
        
#if UNITY_5_2_0 || UNITY_5_2_1
        protected override void OnPopulateMesh(Mesh toFill)
        {
            Texture tex = mainTexture;


            if (tex != null)
            {
                Vector4 v = Vector4.zero;
                Rect uvRect = new Rect(Vector2.zero, rectTransform.rect.size);
                uvRect.width = Mathf.Ceil(uvRect.width);
                uvRect.height = Mathf.Ceil(uvRect.height);

                int w = Mathf.RoundToInt(tex.width * uvRect.width);
                int h = Mathf.RoundToInt(tex.height * uvRect.height);

                float paddedW = ((w & 1) == 0) ? w : w + 1;
                float paddedH = ((h & 1) == 0) ? h : h + 1;

                v.x = 0f;
                v.y = 0f;
                v.z = w / paddedW;
                v.w = h / paddedH;

                v.x -= rectTransform.pivot.x;
                v.y -= rectTransform.pivot.y;
                v.z -= rectTransform.pivot.x;
                v.w -= rectTransform.pivot.y;

                v.x *= rectTransform.rect.width;
                v.y *= rectTransform.rect.height;
                v.z *= rectTransform.rect.width;
                v.w *= rectTransform.rect.height;

                using (var vh = new VertexHelper())
                {
                    Vector2 whiteUV = Vector2.zero;

                    Vector3 lb = new Vector3(v.x, v.y);
                    Vector3 lt = new Vector3(v.x, v.w);
                    Vector3 rt = new Vector3(v.z, v.w);
                    Vector3 rb = new Vector3(v.z, v.y);

                    int vert = 0;

                    vh.AddVert(lb, color, whiteUV);
                    vh.AddVert(lt, color, whiteUV);
                    vh.AddVert(rt, color, whiteUV);
                    vh.AddVert(rb, color, whiteUV);

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                    vert += 4;

                    float width = rb.x - lb.x;
                    float height = lt.y - lb.y;

                    Vector3 center = Vector3.Lerp(lb, rt, 0.5f);
                    Rect rect = new Rect(lb, new Vector2(width, height));
                    bool sizeIsCorrect = pixelsToDisplay.Length == (1 + (Mathf.CeilToInt(horizontalPixels) * 2)) * (1 + (Mathf.CeilToInt(verticalPixels) * 2));

                    UIVertex[] vertices = new UIVertex[4];
                    vertices[0].uv0 = whiteUV;
                    vertices[1].uv0 = whiteUV;
                    vertices[2].uv0 = whiteUV;
                    vertices[3].uv0 = whiteUV;

                    if (ExpectedVertices >= 65000)
                    {
                        Debug.LogWarning("to many vertices, currently " + ExpectedVertices + "/64999. make sure the ColorEyedropperPreview displays less pixels. You can achiee this by either editing it's settings in the inspector or resizing the component", this);
                    }
                    else
                    {
                        for (int radius = 1; radius <= Mathf.Ceil(horizontalPixels) || radius <= Mathf.Ceil(verticalPixels); radius++)
                        {
                            float offset = radius * (pixelSize + borderSize);

                            // Top side
                            for (int top = 0; top < radius * 2; top++)
                            {
                                float extraOffset = top * (pixelSize + borderSize);
                                Vector3 pos = center + new Vector3(-offset, offset) + new Vector3(extraOffset, 0);

                                if (SetQuadPositions(vertices, pos, rect))
                                {
                                    Color32 c;
                                    if (sizeIsCorrect && activated)
                                        c = pixelsToDisplay[(pixelsToDisplay.Length / 2) + (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * radius) - (radius - top)];
                                    else
                                        c = InactiveColor;
                                    SetQuadColors(vertices, c);
                                    vh.AddUIVertexQuad(vertices);
                                }
                            }

                            // Right side
                            for (int right = 0; right < radius * 2; right++)
                            {
                                float extraOffset = right * (pixelSize + borderSize);
                                Vector3 pos = center + new Vector3(offset, offset) + new Vector3(0, -extraOffset);

                                if (SetQuadPositions(vertices, pos, rect))
                                {
                                    Color32 c;
                                    if (sizeIsCorrect && activated)
                                        c = pixelsToDisplay[(pixelsToDisplay.Length / 2) + radius + (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * (radius - right))];
                                    else
                                        c = InactiveColor;
                                    SetQuadColors(vertices, c);
                                    vh.AddUIVertexQuad(vertices);
                                }
                            }

                            // Bot side
                            for (int bot = 0; bot < radius * 2; bot++)
                            {
                                float extraOffset = bot * (pixelSize + borderSize);
                                Vector3 pos = center + new Vector3(offset, -offset) + new Vector3(-extraOffset, 0);

                                if (SetQuadPositions(vertices, pos, rect))
                                {
                                    Color32 c;
                                    if (sizeIsCorrect && activated)
                                        c = pixelsToDisplay[(pixelsToDisplay.Length / 2) - (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * radius) + (radius - bot)];
                                    else
                                        c = InactiveColor;
                                    SetQuadColors(vertices, c);
                                    vh.AddUIVertexQuad(vertices);
                                }
                            }

                            // Left side
                            for (int left = 0; left < radius * 2; left++)
                            {
                                float extraOffset = left * (pixelSize + borderSize);
                                Vector3 pos = center + new Vector3(-offset, -offset) + new Vector3(0, extraOffset);

                                if (SetQuadPositions(vertices, pos, rect))
                                {
                                    Color32 c;
                                    if (sizeIsCorrect && activated)
                                        c = pixelsToDisplay[(pixelsToDisplay.Length / 2) - radius - (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * (radius - left))];
                                    else
                                        c = InactiveColor;
                                    SetQuadColors(vertices, c);
                                    vh.AddUIVertexQuad(vertices);
                                }
                            }
                        }
                    }

                    // Selectionbox
                    if (SetQuadPositions(vertices, center, rect, borderSize + 1f))
                    {
                        SetQuadColors(vertices, selectionBoxColor);
                        vh.AddUIVertexQuad(vertices);
                    }

                    // Center piece
                    if (SetQuadPositions(vertices, center, rect))
                    {
                        Color32 co;
                        if (sizeIsCorrect && activated)
                            co = pixelsToDisplay[pixelsToDisplay.Length / 2];
                        else
                            co = InactiveColor;
                        SetQuadColors(vertices, co);
                        vh.AddUIVertexQuad(vertices);
                    }
                    vh.FillMesh(toFill);
                }
            }
        }
#else // Newest version of UNITY
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Texture tex = mainTexture;

            vh.Clear();
            if (tex != null)
            {
                Rect r = GetPixelAdjustedRect();
                Vector4 v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

                Vector2 whiteUV = Vector2.zero;

                Vector3 lb = new Vector3(v.x, v.y);
                Vector3 lt = new Vector3(v.x, v.w);
                Vector3 rt = new Vector3(v.z, v.w);
                Vector3 rb = new Vector3(v.z, v.y);

                int vert = 0;

                vh.AddVert(lb, color, whiteUV);
                vh.AddVert(lt, color, whiteUV);
                vh.AddVert(rt, color, whiteUV);
                vh.AddVert(rb, color, whiteUV);

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vert += 4;

                float width = rb.x - lb.x;
                float height = lt.y - lb.y;

                Vector3 center = Vector3.Lerp(lb, rt, 0.5f);
                Rect rect = new Rect(lb, new Vector2(width, height));
                bool sizeIsCorrect = pixelsToDisplay.Length == (1 + (Mathf.CeilToInt(horizontalPixels) * 2)) * (1 + (Mathf.CeilToInt(verticalPixels) * 2));

                UIVertex[] vertices = new UIVertex[4];
                vertices[0].uv0 = whiteUV;
                vertices[1].uv0 = whiteUV;
                vertices[2].uv0 = whiteUV;
                vertices[3].uv0 = whiteUV;

                if (ExpectedVertices >= 65000)
                {
                    Debug.LogWarning("to many vertices, currently " + ExpectedVertices + "/64999. make sure the ColorEyedropperPreview displays less pixels. You can achiee this by either editing it's settings in the inspector or resizing the component", this);
                }
                else
                {
                    for (int radius = 1; radius <= Mathf.Ceil(horizontalPixels) || radius <= Mathf.Ceil(verticalPixels); radius++)
                    {
                        float offset = radius * (pixelSize + borderSize);

                        // Top side
                        for (int top = 0; top < radius * 2; top++)
                        {
                            float extraOffset = top * (pixelSize + borderSize);
                            Vector3 pos = center + new Vector3(-offset, offset) + new Vector3(extraOffset, 0);

                            if (SetQuadPositions(vertices, pos, rect))
                            {
                                Color32 c;
                                if (sizeIsCorrect && activated)
                                    c = pixelsToDisplay[(pixelsToDisplay.Length / 2) + (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * radius) - (radius - top)];
                                else
                                    c = InactiveColor;
                                SetQuadColors(vertices, c);
                                vh.AddUIVertexQuad(vertices);
                            }
                        }

                        // Right side
                        for (int right = 0; right < radius * 2; right++)
                        {
                            float extraOffset = right * (pixelSize + borderSize);
                            Vector3 pos = center + new Vector3(offset, offset) + new Vector3(0, -extraOffset);

                            if (SetQuadPositions(vertices, pos, rect))
                            {
                                Color32 c;
                                if (sizeIsCorrect && activated)
                                    c = pixelsToDisplay[(pixelsToDisplay.Length / 2) + radius + (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * (radius - right))];
                                else
                                    c = InactiveColor;
                                SetQuadColors(vertices, c);
                                vh.AddUIVertexQuad(vertices);
                            }
                        }

                        // Bot side
                        for (int bot = 0; bot < radius * 2; bot++)
                        {
                            float extraOffset = bot * (pixelSize + borderSize);
                            Vector3 pos = center + new Vector3(offset, -offset) + new Vector3(-extraOffset, 0);

                            if (SetQuadPositions(vertices, pos, rect))
                            {
                                Color32 c;
                                if (sizeIsCorrect && activated)
                                    c = pixelsToDisplay[(pixelsToDisplay.Length / 2) - (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * radius) + (radius - bot)];
                                else
                                    c = InactiveColor;
                                SetQuadColors(vertices, c);
                                vh.AddUIVertexQuad(vertices);
                            }
                        }

                        // Left side
                        for (int left = 0; left < radius * 2; left++)
                        {
                            float extraOffset = left * (pixelSize + borderSize);
                            Vector3 pos = center + new Vector3(-offset, -offset) + new Vector3(0, extraOffset);

                            if (SetQuadPositions(vertices, pos, rect))
                            {
                                Color32 c;
                                if (sizeIsCorrect && activated)
                                    c = pixelsToDisplay[(pixelsToDisplay.Length / 2) - radius - (((Mathf.CeilToInt(horizontalPixels) * 2) + 1) * (radius - left))];
                                else
                                    c = InactiveColor;
                                SetQuadColors(vertices, c);
                                vh.AddUIVertexQuad(vertices);
                            }
                        }
                    }
                }

                // Selectionbox
                if (SetQuadPositions(vertices, center, rect, borderSize + 1f))
                {
                    SetQuadColors(vertices, selectionBoxColor);
                    vh.AddUIVertexQuad(vertices);
                }

                // Center piece
                if (SetQuadPositions(vertices, center, rect))
                {
                    Color32 co;
                    if (sizeIsCorrect && activated)
                        co = pixelsToDisplay[pixelsToDisplay.Length / 2];
                    else
                        co = InactiveColor;
                    SetQuadColors(vertices, co);
                    vh.AddUIVertexQuad(vertices);
                }
            }
        }
#endif


        private bool SetQuadPositions(UIVertex[] verts, Vector3 center, Rect inside)
        {
            return SetQuadPositions(verts, center, inside, 0f);
        }

        private bool SetQuadPositions(UIVertex[] verts, Vector3 center, Rect inside, float offset)
        {
            // Set position
            float half = (pixelSize / 2) + offset;

            // Get corner positions, and clamp them inside the rect
            float minX = Mathf.Clamp(center.x - half, inside.xMin, inside.xMax);
            float minY = Mathf.Clamp(center.y - half, inside.yMin, inside.yMax);
            float maxX = Mathf.Clamp(center.x + half, inside.xMin, inside.xMax);
            float maxY = Mathf.Clamp(center.y + half, inside.yMin, inside.yMax);

            // Check if this quad has either a width or height of 0, if so, return false
            if (Mathf.Approximately(minX, maxX) || Mathf.Approximately(minY, maxY))
                return false;

            // Set the positions of the quad
            verts[0].position = new Vector3(minX, minY);
            verts[1].position = new Vector3(minX, maxY);
            verts[2].position = new Vector3(maxX, maxY);
            verts[3].position = new Vector3(maxX, minY);

            return true;
        }

        private void SetQuadColors(UIVertex[] verts, Color32 color)
        {
            verts[0].color = color;
            verts[1].color = color;
            verts[2].color = color;
            verts[3].color = color;
        }
    }
}