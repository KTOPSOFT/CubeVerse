using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace AdvancedColorPicker
{
    [ExecuteInEditMode, RequireComponent(typeof(CanvasRenderer))]
    public class GradientBackground2D : GraphicalColorTypeComponent
    {
        private int xSize = 0; // Amount of verices in the width
        private int ySize = 0; // Amount of vertices in the height
        [SerializeField,HideInInspector]
        private Color32[][] colors = new Color32[0][];

        protected virtual bool InverseX
        {
            get
            {
                return false;
            }
        }

        protected virtual bool InverseY
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the texture used to draw this Graphic.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                return ColorPickerUtils.Checkboard;
            }
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
                    // Add vertices
                    float xStep = (v.z - v.x) / (xSize - 1);
                    float yStep = (v.w - v.y) / (ySize - 1);
                    Vector3 pos = new Vector3(v.x, v.y);
                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            for (int index = 0; index < colors.Length; index++)
                            {
                                vh.AddVert(pos, colors[index][y + (x * ySize)], new Vector2(0.25f, 0.25f));
                            }
                            pos.y += yStep;
                        }
                        pos.x += xStep;
                        pos.y = v.y;
                    }

                    // Add triangles
                    for (int y = 0; y < ySize - 1; y++)
                    {
                        for (int x = 0; x < xSize - 1; x++)
                        {
                            for (int index = 0; index < colors.Length; index++)
                            {
                                int pos1 = index + ((y + (x * ySize)) * colors.Length);
                                int pos2 = index + (((y + 1) + (x * ySize)) * colors.Length);
                                int pos3 = index + (((y + 1) + ((x + 1) * ySize)) * colors.Length);
                                int pos4 = index + ((y + ((x + 1) * ySize)) * colors.Length);

                                vh.AddTriangle(pos1, pos2, pos3);
                                vh.AddTriangle(pos3, pos4, pos1);
                            }
                        }
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
                if (InverseX)
                {
                    float x = v.x;
                    v.x = v.z;
                    v.z = x;
                }
                if (InverseY)
                {
                    float y = v.y;
                    v.y = v.w;
                    v.w = y;
                }

                // Add vertices
                float xStep = (v.z - v.x) / (xSize - 1);
                float yStep = (v.w - v.y) / (ySize - 1);
                Vector3 pos = new Vector3(v.x, v.y);
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        for (int index = 0; index < colors.Length; index++)
                        {
                            vh.AddVert(pos, colors[index][y + (x * ySize)], new Vector2(0.25f, 0.25f));
                        }
                        pos.y += yStep;
                    }
                    pos.x += xStep;
                    pos.y = v.y;
                }

                // Add triangles
                for (int y = 0; y < ySize - 1; y++)
                {
                    for (int x = 0; x < xSize - 1; x++)
                    {
                        for (int index = 0; index < colors.Length; index++)
                        {
                            int pos1 = index + ((y + (x * ySize)) * colors.Length);
                            int pos2 = index + (((y + 1) + (x * ySize)) * colors.Length);
                            int pos3 = index + (((y + 1) + ((x + 1) * ySize)) * colors.Length);
                            int pos4 = index + ((y + ((x + 1) * ySize)) * colors.Length);

                            vh.AddTriangle(pos1, pos2, pos3);
                            vh.AddTriangle(pos3, pos4, pos1);
                        }
                    }
                }
            }
        }
#endif

        protected override void DisplayNewColor()
        {
            switch (ValueType1)
            {
                case ColorType.RGB_R:
                case ColorType.RGB_G:
                case ColorType.RGB_B:
                case ColorType.HSV_S:
                case ColorType.HSV_V:
                case ColorType.HSL_S:
                    xSize = 2;
                    break;
                case ColorType.HSV_H:
                case ColorType.HSL_H:
                    xSize = 7;
                    break;
                case ColorType.HSL_L:
                    xSize = 3;
                    break;
                default:
                    xSize = 0;
                    break;
            }
            switch (ValueType2)
            {
                case ColorType.RGB_R:
                case ColorType.RGB_G:
                case ColorType.RGB_B:
                case ColorType.HSV_S:
                case ColorType.HSV_V:
                case ColorType.HSL_S:
                    ySize = 2;
                    break;
                case ColorType.HSV_H:
                case ColorType.HSL_H:
                    ySize = 7;
                    break;
                case ColorType.HSL_L:
                    ySize = 3;
                    break;
                default:
                    ySize = 0;
                    break;
            }

            switch (RangeType)
            {
                case RGBColor.Flag:
                    ResizeColors(1);
                    break;
                case HSVColor.Flag:
                case HSLColor.Flag:
                    ResizeColors(3);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Length != xSize * ySize)
                    colors[i] = new Color32[xSize * ySize];
            }

            int index = 0;
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float xpos = (float)x / (xSize - 1);
                    float ypos = (float)y / (ySize - 1);

                    switch (ValueType1)
                    {
                        case ColorType.RGB_R:
                            if (ValueType2 == ColorType.RGB_G)
                                colors[0][index] = new Color(xpos, ypos, GetValue3(), 1f);
                            else
                                colors[0][index] = new Color(xpos, GetValue3(), ypos, 1f);
                            break;
                        case ColorType.RGB_G:
                            if (ValueType2 == ColorType.RGB_R)
                                colors[0][index] = new Color(ypos, xpos, GetValue3(), 1f);
                            else
                                colors[0][index] = new Color(GetValue3(), xpos, ypos, 1f);
                            break;
                        case ColorType.RGB_B:
                            if (ValueType2 == ColorType.RGB_R)
                                colors[0][index] = new Color(ypos, GetValue3(), xpos, 1f);
                            else
                                colors[0][index] = new Color(GetValue3(), ypos, xpos, 1f);
                            break;
                        case ColorType.HSV_H:
                            if (ValueType2 == ColorType.HSV_S)
                                SetColorsHSV(index, xpos, ypos, GetValue3());
                            else
                                SetColorsHSV(index, xpos, GetValue3(), ypos);
                            break;
                        case ColorType.HSV_S:
                            if (ValueType2 == ColorType.HSV_H)
                                SetColorsHSV(index, ypos, xpos, GetValue3());
                            else
                                SetColorsHSV(index, GetValue3(), xpos, ypos);
                            break;
                        case ColorType.HSV_V:
                            if (ValueType2 == ColorType.HSV_H)
                                SetColorsHSV(index, ypos, GetValue3(), xpos);
                            else
                                SetColorsHSV(index, GetValue3(), ypos, xpos);
                            break;
                        case ColorType.HSL_H:
                            if (ValueType2 == ColorType.HSL_S)
                                SetColorsHSL(index, xpos, ypos, GetValue3());
                            else
                                SetColorsHSL(index, xpos, GetValue3(), ypos);
                            break;
                        case ColorType.HSL_S:
                            if (ValueType2 == ColorType.HSL_H)
                                SetColorsHSL(index, ypos, xpos, GetValue3());
                            else
                                SetColorsHSL(index, GetValue3(), xpos, ypos);
                            break;
                        case ColorType.HSL_L:
                            if (ValueType2 == ColorType.HSL_S)
                                SetColorsHSL(index, GetValue3(), ypos, xpos);
                            else
                                SetColorsHSL(index, ypos, GetValue3(), xpos);
                            break;
                        default:
                            break;
                    }
                    index++;
                }
            }

            SetVerticesDirty();
        }

        private void SetColorsHSV(int index, float h, float s, float v)
        {
            colors[0][index] = new HSVColor(h, 1f, 1f).ToColor(255);
            colors[1][index] = new Color32(255, 255, 255, (byte)((1 - s) * 255));
            colors[2][index] = new Color32(0, 0, 0, (byte)((1 - v) * 255));
        }

        private void SetColorsHSL(int index, float h, float s, float l)
        {
            colors[0][index] = new HSLColor(h, 1f, 0.5f).ToColor(255);
            colors[1][index] = new Color(l, l, l, 1f - s);
            colors[2][index] = new Color(l, l, l, Mathf.Abs(l - 0.5f) * 2);
        }


        public void SetToDefaultType(ColorType t1, ColorType t2)
        {
            ValueType1 = t1;
            ValueType2 = t2;

            switch (ValueType3)
            {
                case ColorType.HSV_S:
                    FixedValue3 = 1f;
                    break;
                case ColorType.HSV_V:
                    FixedValue3 = 1f;
                    break;
                case ColorType.HSL_S:
                    FixedValue3 = 1f;
                    break;
                case ColorType.HSL_L:
                    FixedValue3 = 0.5f;
                    break;
                default:
                    FixedValue3 = -1f;
                    break;
            }
        }

        private void ResizeColors(int size)
        {
            if (colors.Length != size)
                colors = new Color32[size][];

            for (int i = 0; i < size; i++)
            {
                colors[i] = new Color32[xSize * ySize];
            }
        }
    }
}