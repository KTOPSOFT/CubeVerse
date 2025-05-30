using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class GradientBackground : GraphicalColorComponent
    {
        private bool acceptEvents = true;

        public enum GradientPartType
        {
            Custom,

            Color,

            RGB_R,
            RGB_G,
            RGB_B,

            HSV_H,
            HSV_S,
            HSV_V,

            HSL_H,
            HSL_S,
            HSL_L
        }

        /// <summary>
        /// Indicates on which value the gradient should center. Custom allows for a value between 0 and 1. All other settings use the ColorPicker's value of that type instead.
        /// </summary>
        public enum GradientCenterType
        {
            /// <summary>
            /// We ourselves control where the CenterPosition will be
            /// </summary>
            Custom,

            Alpha,

            RGB_R,
            RGB_G,
            RGB_B,

            HSV_H,
            HSV_S,
            HSV_V,

            HSL_H,
            HSL_S,
            HSL_L
        }


        [SerializeField]
        private GradientPart[] colors = new GradientPart[2] { new GradientPart(), new GradientPart() };

        [SerializeField]
        private bool displayCheckboard;

        [SerializeField]
        private bool gradient = true;

        [SerializeField]
        private Slider.Direction direction;

        [SerializeField, Range(0, 1)]
        private float centerPos = 0.5f;

        [SerializeField]
        private GradientCenterType centerType;

        [SerializeField]
        private float checkBoardSize = 1f;

        [SerializeField]
        private float borderSize = 1f;

        public bool DisplayCheckboard
        {
            get
            {
                return displayCheckboard;
            }
            set
            {
                if (displayCheckboard == value)
                    return;

                displayCheckboard = value;
                DisplayNewColor();
            }
        }

        public float CheckboardSize
        {
            get
            {
                return checkBoardSize;
            }
            set
            {
                if (checkBoardSize == value)
                    return;

                checkBoardSize = value;
                DisplayNewColor();
            }
        }

        public bool Gradient
        {
            get
            {
                return gradient;
            }
            set
            {
                if (gradient == value)
                    return;

                gradient = value;
                DisplayNewColor();
            }
        }

        /// <summary>
        /// Gets or sets the CenterType.
        /// This indicates what controls the CenterPosition.
        /// </summary>
        public GradientCenterType CenterType
        {
            get
            {
                return centerType;
            }
            set
            {
                if (centerType == value)
                    return;

                centerType = value;
                DisplayNewColor();
            }
        }

        /// <summary>
        /// Gets or sets the Center position.
        /// Setting this value forces CenterType to 'Custom'.
        /// </summary>
        /// <remarks>
        /// This position indicates a position between 0 (left side of the Texture) and 1 (right side of the Texture). 0.5 being the center of the texture. (This example is when Direction is set to LeftToRight)
        /// 
        /// If Gradient is set to false, the left and right color will split on this position.
        /// 
        /// If Gradient is true, when lerping te 2 colors, the 0.5 of the lerp will be at this position.
        /// If this value is 0, we will instantly lerp from 0 to 0.5, and the rest of the bar will be used to lerp from 0.5 to 1.
        /// 
        /// </remarks>
        public float CenterPosition
        {
            get
            {
                return centerPos;
            }
            set
            {
                value = Mathf.Clamp01(value);

                if (centerPos == value)
                    return;

                centerPos = value;
                centerType = GradientCenterType.Custom;
                DisplayNewColor();
            }
        }

        public float BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                value = Mathf.Max(value, 0);

                if (borderSize == value)
                    return;

                borderSize = value;
                DisplayNewColor();
            }
        }

        public Slider.Direction Direction
        {
            get
            {
                return direction;
            }
            set
            {
                if (direction == value)
                    return;

                direction = value;

                DisplayNewColor();
            }
        }

        public int Count
        {
            get
            {
                return colors.Length;
            }
            set
            {
                value = Math.Max(value, 2);

                if (colors.Length == value)
                    return;

                GradientPart[] newcolors = new GradientPart[value];

                // copy existing
                for (int i = 0; i < value; i++)
                {
                    if (i < colors.Length)
                        newcolors[i] = colors[i];
                    else
                        newcolors[i] = new GradientPart(colors[colors.Length - 1]);
                }

                colors = newcolors;
                DisplayNewColor();
            }
        }

        public GradientPart this[int index]
        {
            get
            {
                colors[index].callback = DisplayNewColor;
                return colors[index];
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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            checkBoardSize = Mathf.Max(0.001f, checkBoardSize);
            borderSize = Mathf.Max(0, borderSize);
            base.OnValidate();
        }

        protected override void Reset()
        {
            color = new Color32(50, 50, 50, 255);
            base.Reset();
        }
#endif

        public void SetToDefaultType(ColorType type)
        {
            this.centerType = GradientCenterType.Custom;
            this.centerPos = 0.5f;

            switch (type)
            {
                case ColorType.RGB_R:
                    this[0].Type = GradientPartType.RGB_R;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.RGB_R;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.RGB_G:
                    this[0].Type = GradientPartType.RGB_G;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.RGB_G;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.RGB_B:
                    this[0].Type = GradientPartType.RGB_B;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.RGB_B;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.HSV_H:
                    Count = 2;
                    this[0].Type = GradientPartType.HSV_H;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = true;
                    this[0].FixedValue2 = this[0].FixedValue3 = 1;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSV_H;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = true;
                    this[1].FixedValue2 = this[1].FixedValue3 = 1;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 7;
                    break;
                case ColorType.HSV_S:
                    this[0].Type = GradientPartType.HSV_S;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSV_S;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.HSV_V:
                    this[0].Type = GradientPartType.HSV_V;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSV_V;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.HSL_H:
                    Count = 2;
                    this[0].Type = GradientPartType.HSL_H;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = true;
                    this[0].FixedValue2 = 1;
                    this[0].FixedValue3 = 0.5f;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSL_H;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = true;
                    this[1].FixedValue2 = 1;
                    this[1].FixedValue3 = 0.5f;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 7;
                    break;
                case ColorType.HSL_S:
                    this[0].Type = GradientPartType.HSL_S;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSL_S;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 2;
                    break;
                case ColorType.HSL_L:
                    Count = 2;
                    this[0].Type = GradientPartType.HSL_L;
                    this[0].Value2IsFixed = this[0].Value3IsFixed = false;
                    this[0].AlphaIsFixed = true;
                    this[0].FixedAlphaValue = 255;

                    this[1].Type = GradientPartType.HSL_L;
                    this[1].Value2IsFixed = this[1].Value3IsFixed = false;
                    this[1].AlphaIsFixed = true;
                    this[1].FixedAlphaValue = 255;
                    Count = 3;
                    break;
                default:
                    break;
            }
        }

        protected override void DisplayNewColor()
        {
            if (!acceptEvents)
                return;
            acceptEvents = false;

            for (int i = 0; i < colors.Length; i++)
            {
                float normalizedI = (float)i / (colors.Length - 1);
                switch (colors[i].Type)
                {
                    case GradientPartType.Custom:
                        break;
                    case GradientPartType.Color:
                        colors[i].Color = Picker == null ? new Color32(0, 0, 0, 255) : Picker.rgb.ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.RGB_R:
                        colors[i].Color = Picker == null ? new Color(normalizedI, colors[i].FixedValue2, colors[i].FixedValue3, colors[i].FixedAlphaNormalized) :
                            new Color(normalizedI, colors[i].GetValue2((float)Picker.rgb.G), colors[i].GetValue3((float)Picker.rgb.B), colors[i].GetAlpha(Picker.AlphaNormalized));
                        break;
                    case GradientPartType.RGB_G:
                        colors[i].Color = Picker == null ? new Color(colors[i].FixedValue2, normalizedI, colors[i].FixedValue3, colors[i].FixedAlphaNormalized) :
                            new Color(colors[i].GetValue2((float)Picker.rgb.R), normalizedI, colors[i].GetValue3((float)Picker.rgb.B), colors[i].GetAlpha(Picker.AlphaNormalized));
                        break;
                    case GradientPartType.RGB_B:
                        colors[i].Color = Picker == null ? new Color(colors[i].FixedValue2, colors[i].FixedValue3, normalizedI, colors[i].FixedAlphaNormalized) :
                            new Color(colors[i].GetValue2((float)Picker.rgb.R), colors[i].GetValue3((float)Picker.rgb.G), normalizedI, colors[i].GetAlpha(Picker.AlphaNormalized));
                        break;
                    case GradientPartType.HSV_H:
                        colors[i].Color = Picker == null ? new HSVColor(normalizedI, colors[i].FixedValue2, colors[i].FixedValue3).ToColor(colors[i].FixedAlphaValue) :
                            new HSVColor(normalizedI, colors[i].GetValue2(Picker.hsv.NormalizedS), colors[i].GetValue3(Picker.hsv.NormalizedV)).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.HSV_S:
                        colors[i].Color = Picker == null ? new HSVColor(colors[i].FixedValue2, normalizedI, colors[i].FixedValue3).ToColor(colors[i].FixedAlphaValue) :
                            new HSVColor(colors[i].GetValue2(Picker.hsv.NormalizedH), normalizedI, colors[i].GetValue3(Picker.hsv.NormalizedV)).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.HSV_V:
                        colors[i].Color = Picker == null ? new HSVColor(colors[i].FixedValue2, colors[i].FixedValue3, normalizedI).ToColor(colors[i].FixedAlphaValue) :
                            new HSVColor(colors[i].GetValue2(Picker.hsv.NormalizedH), colors[i].GetValue3(Picker.hsv.NormalizedS), normalizedI).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.HSL_H:
                        colors[i].Color = Picker == null ? new HSLColor(normalizedI, colors[i].FixedValue2, colors[i].FixedValue3).ToColor(colors[i].FixedAlphaValue) :
                            new HSLColor(normalizedI, colors[i].GetValue2(Picker.hsl.NormalizedS), colors[i].GetValue3(Picker.hsl.NormalizedL)).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.HSL_S:
                        colors[i].Color = Picker == null ? new HSLColor(colors[i].FixedValue2, normalizedI, colors[i].FixedValue3).ToColor(colors[i].FixedAlphaValue) :
                            new HSLColor(colors[i].GetValue2(Picker.hsl.NormalizedH), normalizedI, colors[i].GetValue3(Picker.hsl.NormalizedL)).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    case GradientPartType.HSL_L:
                        colors[i].Color = Picker == null ? new HSLColor(colors[i].FixedValue2, colors[i].FixedValue3, normalizedI).ToColor(colors[i].FixedAlphaValue) :
                            new HSLColor(colors[i].GetValue2(Picker.hsl.NormalizedH), colors[i].GetValue3(Picker.hsl.NormalizedS), normalizedI).ToColor(colors[i].GetAlpha(Picker.Alpha));
                        break;
                    default:
                        break;
                }
            }

            switch (centerType)
            {
                case GradientCenterType.Custom:
                    break;
                case GradientCenterType.Alpha:
                    centerPos = Picker == null ? 0.5f : Picker.AlphaNormalized;
                    break;
                case GradientCenterType.RGB_R:
                    centerPos = Picker == null ? 0.5f : (float)Picker.rgb.R;
                    break;
                case GradientCenterType.RGB_G:
                    centerPos = Picker == null ? 0.5f : (float)Picker.rgb.G;
                    break;
                case GradientCenterType.RGB_B:
                    centerPos = Picker == null ? 0.5f : (float)Picker.rgb.B;
                    break;
                case GradientCenterType.HSV_H:
                    centerPos = Picker == null ? 0.5f : Picker.hsv.NormalizedH;
                    break;
                case GradientCenterType.HSV_S:
                    centerPos = Picker == null ? 0.5f : Picker.hsv.NormalizedS;
                    break;
                case GradientCenterType.HSV_V:
                    centerPos = Picker == null ? 0.5f : Picker.hsv.NormalizedV;
                    break;
                case GradientCenterType.HSL_H:
                    centerPos = Picker == null ? 0.5f : Picker.hsl.NormalizedH;
                    break;
                case GradientCenterType.HSL_S:
                    centerPos = Picker == null ? 0.5f : Picker.hsl.NormalizedS;
                    break;
                case GradientCenterType.HSL_L:
                    centerPos = Picker == null ? 0.5f : Picker.hsl.NormalizedL;
                    break;
                default:
                    break;
            }

            acceptEvents = true;

            SetVerticesDirty();
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
                Rect uvRect = new Rect(0, 0, Mathf.Ceil(Mathf.Max(1, rectTransform.rect.width)), Mathf.Ceil(Mathf.Max(1, rectTransform.rect.height)));

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
                    Vector2 whiteUV = new Vector2(0.25f, 0.25f);

                    Vector3 lb;
                    Vector3 lt;
                    Vector3 rt;
                    Vector3 rb;

                    int vert = 0;

                    if (borderSize > 0)
                    {
                        vh.AddVert(new Vector3(v.x, v.y), color, whiteUV);
                        vh.AddVert(new Vector3(v.x, v.w), color, whiteUV);
                        vh.AddVert(new Vector3(v.z, v.w), color, whiteUV);
                        vh.AddVert(new Vector3(v.z, v.y), color, whiteUV);
                        vh.AddTriangle(vert, vert + 1, vert + 2);
                        vh.AddTriangle(vert + 2, vert + 3, vert);
                        vert += 4;

                        v.x += borderSize;
                        v.y += borderSize;
                        v.w -= borderSize;
                        v.z -= borderSize;
                    }

                    switch (direction)
                    {
                        case Slider.Direction.BottomToTop:
                            lb = new Vector3(v.z, v.y);
                            lt = new Vector3(v.x, v.y);
                            rt = new Vector3(v.x, v.w);
                            rb = new Vector3(v.z, v.w);
                            break;
                        case Slider.Direction.LeftToRight:
                            lb = new Vector3(v.x, v.y);
                            lt = new Vector3(v.x, v.w);
                            rt = new Vector3(v.z, v.w);
                            rb = new Vector3(v.z, v.y);
                            break;
                        case Slider.Direction.RightToLeft:
                            lb = new Vector3(v.z, v.y);
                            lt = new Vector3(v.z, v.w);
                            rt = new Vector3(v.x, v.w);
                            rb = new Vector3(v.x, v.y);
                            break;
                        case Slider.Direction.TopToBottom:
                            lb = new Vector3(v.x, v.w);
                            lt = new Vector3(v.z, v.w);
                            rt = new Vector3(v.z, v.y);
                            rb = new Vector3(v.x, v.y);
                            break;
                        default:
                            throw new System.NotImplementedException(direction.ToString());
                    }

                    // Draw checkboard background if required
                    if (displayCheckboard)
                    {
                        switch (direction)
                        {
                            case Slider.Direction.BottomToTop:
                            case Slider.Direction.TopToBottom:
                                Vector2 tempSize = rectTransform.rect.size / 8;
                                uvRect = new Rect(Vector2.zero, new Vector2(tempSize.y, tempSize.x));
                                break;
                            case Slider.Direction.LeftToRight:
                            case Slider.Direction.RightToLeft:
                                uvRect = new Rect(Vector2.zero, rectTransform.rect.size / 8);
                                break;
                            default:
                                throw new System.NotImplementedException(direction.ToString());
                        }
                        uvRect.max = uvRect.max / checkBoardSize;

                        vh.AddVert(lb, new Color32(255, 255, 255, 255), new Vector2(uvRect.xMin, uvRect.yMin));
                        vh.AddVert(lt, new Color32(255, 255, 255, 255), new Vector2(uvRect.xMin, uvRect.yMax));
                        vh.AddVert(rt, new Color32(255, 255, 255, 255), new Vector2(uvRect.xMax, uvRect.yMax));
                        vh.AddVert(rb, new Color32(255, 255, 255, 255), new Vector2(uvRect.xMax, uvRect.yMin));
                        vh.AddTriangle(vert, vert + 1, vert + 2);
                        vh.AddTriangle(vert + 2, vert + 3, vert);
                        vert += 4;
                    }

                    for (int i = 0; i < colors.Length - 1; i++)
                    {
                        // custom left bot and top
                        Vector3 clb = lb + ((rb - lb) * ((float)i / (colors.Length - 1)));
                        Vector3 clt = lt + ((rt - lt) * ((float)i / (colors.Length - 1)));

                        // custom right bot and top
                        Vector3 crb = lb + ((rb - lb) * ((float)(i + 1) / (colors.Length - 1)));
                        Vector3 crt = lt + ((rt - lt) * ((float)(i + 1) / (colors.Length - 1)));

                        // custom center bot and top
                        Vector3 ccb = clb + ((crb - clb) * centerPos);
                        Vector3 cct = clt + ((crt - clt) * centerPos);

                        if (gradient)
                        {
                            Color32 cColor = Color32.Lerp(colors[i].Color, colors[i + 1].Color, 0.5f);

                            vh.AddVert(clb, colors[i].Color, whiteUV);
                            vh.AddVert(clt, colors[i].Color, whiteUV);

                            vh.AddVert(ccb, cColor, whiteUV);
                            vh.AddVert(cct, cColor, whiteUV);

                            vh.AddVert(crb, colors[i + 1].Color, whiteUV);
                            vh.AddVert(crt, colors[i + 1].Color, whiteUV);

                            vh.AddTriangle(vert, vert + 1, vert + 3);
                            vh.AddTriangle(vert + 3, vert + 2, vert);
                            vh.AddTriangle(vert + 2, vert + 3, vert + 5);
                            vh.AddTriangle(vert + 5, vert + 4, vert + 2);
                            vert += 6;
                        }
                        else
                        {
                            vh.AddVert(clb, colors[i].Color, whiteUV);
                            vh.AddVert(clt, colors[i].Color, whiteUV);

                            vh.AddVert(ccb, colors[i].Color, whiteUV);
                            vh.AddVert(cct, colors[i].Color, whiteUV);

                            vh.AddVert(ccb, colors[i + 1].Color, whiteUV);
                            vh.AddVert(cct, colors[i + 1].Color, whiteUV);

                            vh.AddVert(crb, colors[i + 1].Color, whiteUV);
                            vh.AddVert(crt, colors[i + 1].Color, whiteUV);

                            vh.AddTriangle(vert, vert + 1, vert + 3);
                            vh.AddTriangle(vert + 3, vert + 2, vert);
                            vh.AddTriangle(vert + 4, vert + 5, vert + 7);
                            vh.AddTriangle(vert + 7, vert + 6, vert + 4);
                            vert += 8;
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

                Vector2 whiteUV = new Vector2(0.25f, 0.25f);

                Vector3 lb;
                Vector3 lt;
                Vector3 rt;
                Vector3 rb;

                int vert = 0;

                if (borderSize > 0)
                {
                    vh.AddVert(new Vector3(v.x, v.y), color, whiteUV);
                    vh.AddVert(new Vector3(v.x, v.w), color, whiteUV);
                    vh.AddVert(new Vector3(v.z, v.w), color, whiteUV);
                    vh.AddVert(new Vector3(v.z, v.y), color, whiteUV);
                    vh.AddTriangle(vert, vert + 1, vert + 2);
                    vh.AddTriangle(vert + 2, vert + 3, vert);
                    vert += 4;

                    v.x += borderSize;
                    v.y += borderSize;
                    v.w -= borderSize;
                    v.z -= borderSize;
                }

                switch (direction)
                {
                    case Slider.Direction.BottomToTop:
                        lb = new Vector3(v.z, v.y);
                        lt = new Vector3(v.x, v.y);
                        rt = new Vector3(v.x, v.w);
                        rb = new Vector3(v.z, v.w);
                        break;
                    case Slider.Direction.LeftToRight:
                        lb = new Vector3(v.x, v.y);
                        lt = new Vector3(v.x, v.w);
                        rt = new Vector3(v.z, v.w);
                        rb = new Vector3(v.z, v.y);
                        break;
                    case Slider.Direction.RightToLeft:
                        lb = new Vector3(v.z, v.y);
                        lt = new Vector3(v.z, v.w);
                        rt = new Vector3(v.x, v.w);
                        rb = new Vector3(v.x, v.y);
                        break;
                    case Slider.Direction.TopToBottom:
                        lb = new Vector3(v.x, v.w);
                        lt = new Vector3(v.z, v.w);
                        rt = new Vector3(v.z, v.y);
                        rb = new Vector3(v.x, v.y);
                        break;
                    default:
                        throw new System.NotImplementedException(direction.ToString());
                }

                // Draw checkboard background if required
                if (displayCheckboard)
                {
                    switch (direction)
                    {
                        case Slider.Direction.BottomToTop:
                        case Slider.Direction.TopToBottom:
                            Vector2 tempSize = rectTransform.rect.size / 8;
                            r = new Rect(Vector2.zero, new Vector2(tempSize.y, tempSize.x));
                            break;
                        case Slider.Direction.LeftToRight:
                        case Slider.Direction.RightToLeft:
                            r = new Rect(Vector2.zero, rectTransform.rect.size / 8);
                            break;
                        default:
                            throw new System.NotImplementedException(direction.ToString());
                    }
                    r.max = r.max / checkBoardSize;

                    vh.AddVert(lb, new Color32(255, 255, 255, 255), new Vector2(r.xMin, r.yMin));
                    vh.AddVert(lt, new Color32(255, 255, 255, 255), new Vector2(r.xMin, r.yMax));
                    vh.AddVert(rt, new Color32(255, 255, 255, 255), new Vector2(r.xMax, r.yMax));
                    vh.AddVert(rb, new Color32(255, 255, 255, 255), new Vector2(r.xMax, r.yMin));
                    vh.AddTriangle(vert, vert + 1, vert + 2);
                    vh.AddTriangle(vert + 2, vert + 3, vert);
                    vert += 4;
                }

                for (int i = 0; i < colors.Length - 1; i++)
                {
                    // custom left bot and top
                    Vector3 clb = lb + ((rb - lb) * ((float)i / (colors.Length - 1)));
                    Vector3 clt = lt + ((rt - lt) * ((float)i / (colors.Length - 1)));

                    // custom right bot and top
                    Vector3 crb = lb + ((rb - lb) * ((float)(i + 1) / (colors.Length - 1)));
                    Vector3 crt = lt + ((rt - lt) * ((float)(i + 1) / (colors.Length - 1)));

                    // custom center bot and top
                    Vector3 ccb = clb + ((crb - clb) * centerPos);
                    Vector3 cct = clt + ((crt - clt) * centerPos);

                    if (gradient)
                    {
                        Color32 cColor = Color32.Lerp(colors[i].Color, colors[i + 1].Color, 0.5f);

                        vh.AddVert(clb, colors[i].Color, whiteUV);
                        vh.AddVert(clt, colors[i].Color, whiteUV);

                        vh.AddVert(ccb, cColor, whiteUV);
                        vh.AddVert(cct, cColor, whiteUV);

                        vh.AddVert(crb, colors[i + 1].Color, whiteUV);
                        vh.AddVert(crt, colors[i + 1].Color, whiteUV);

                        vh.AddTriangle(vert, vert + 1, vert + 3);
                        vh.AddTriangle(vert + 3, vert + 2, vert);
                        vh.AddTriangle(vert + 2, vert + 3, vert + 5);
                        vh.AddTriangle(vert + 5, vert + 4, vert + 2);
                        vert += 6;
                    }
                    else
                    {
                        vh.AddVert(clb, colors[i].Color, whiteUV);
                        vh.AddVert(clt, colors[i].Color, whiteUV);

                        vh.AddVert(ccb, colors[i].Color, whiteUV);
                        vh.AddVert(cct, colors[i].Color, whiteUV);

                        vh.AddVert(ccb, colors[i + 1].Color, whiteUV);
                        vh.AddVert(cct, colors[i + 1].Color, whiteUV);

                        vh.AddVert(crb, colors[i + 1].Color, whiteUV);
                        vh.AddVert(crt, colors[i + 1].Color, whiteUV);

                        vh.AddTriangle(vert, vert + 1, vert + 3);
                        vh.AddTriangle(vert + 3, vert + 2, vert);
                        vh.AddTriangle(vert + 4, vert + 5, vert + 7);
                        vh.AddTriangle(vert + 7, vert + 6, vert + 4);
                        vert += 8;
                    }

                }
            }
        }
#endif

        [Serializable]
        public class GradientPart
        {
            [SerializeField]
            private GradientPartType type;

            [SerializeField]
            private Color32 color;

            /// <summary>
            /// This is used internally as a callback for when you change any of these properties. I know this is public, but please don't touch it.
            /// </summary>
            public UnityAction callback;

            [SerializeField]
            private bool v2Fixed;
            [SerializeField]
            private bool v3Fixed;
            [SerializeField]
            private bool alphaFixed = true;

            [SerializeField, Range(0, 1)]
            private float fixedV2;
            [SerializeField, Range(0, 1)]
            private float fixedV3;
            [SerializeField, Range(0, 255)]
            private byte fixedAlpha = 255;

            public GradientPartType Type
            {
                get
                {
                    return type;
                }
                set
                {
                    if (type == value)
                        return;

                    type = value;
                    SendChanged();
                }
            }

            public Color32 Color
            {
                get
                {
                    return color;
                }
                set
                {
                    if (color.a == value.a && color.r == value.r && color.g == value.g && color.b == value.b)
                        return;

                    color = value;
                    SendChanged();
                }
            }

            public bool Value2IsFixed
            {
                get
                {
                    return v2Fixed;
                }
                set
                {
                    if (v2Fixed == value)
                        return;
                    v2Fixed = value;
                    SendChanged();
                }
            }

            public bool Value3IsFixed
            {
                get
                {
                    return v3Fixed;
                }
                set
                {
                    if (v3Fixed == value)
                        return;
                    v3Fixed = value;
                    SendChanged();
                }
            }

            public bool AlphaIsFixed
            {
                get
                {
                    return alphaFixed;
                }
                set
                {
                    if (alphaFixed == value)
                        return;
                    alphaFixed = value;
                    SendChanged();
                }
            }

            public float FixedValue2
            {
                get
                {
                    return fixedV2;
                }
                set
                {
                    value = Mathf.Clamp01(value);

                    if (fixedV2 == value)
                        return;
                    fixedV2 = value;
                    SendChanged();
                }
            }

            public float FixedValue3
            {
                get
                {
                    return fixedV3;
                }
                set
                {
                    value = Mathf.Clamp01(value);

                    if (fixedV3 == value)
                        return;
                    fixedV3 = value;
                    SendChanged();
                }
            }

            public byte FixedAlphaValue
            {
                get
                {
                    return fixedAlpha;
                }
                set
                {
                    if (fixedAlpha == value)
                        return;
                    fixedAlpha = value;
                    SendChanged();
                }
            }

            public float FixedAlphaNormalized
            {
                get
                {
                    return (float)fixedAlpha / 255;
                }
            }

            public GradientPart()
            {
                this.type = GradientPartType.RGB_R;
            }

            public GradientPart(GradientPart original)
            {
                this.type = original.type;
                this.color = original.color;
                this.v2Fixed = original.v2Fixed;
                this.v3Fixed = original.v3Fixed;
                this.alphaFixed = original.alphaFixed;
                this.fixedV2 = original.fixedV2;
                this.fixedV3 = original.fixedV3;
                this.fixedAlpha = original.fixedAlpha;
            }

            private void SendChanged()
            {
                if (callback != null)
                    callback();
            }

            public float GetValue2(float requested)
            {
                return v2Fixed ? fixedV2 : requested;
            }

            public float GetValue3(float requested)
            {
                return v3Fixed ? fixedV3 : requested;
            }

            public float GetAlpha(float requested)
            {
                return alphaFixed ? (float)fixedAlpha / 255 : requested;
            }

            public byte GetAlpha(byte requested)
            {
                return alphaFixed ? fixedAlpha : requested;
            }
        }
    }
}