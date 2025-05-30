using UnityEngine;
using UnityEngine.UI;
using System;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(SliderCircle2D), typeof(CanvasRenderer)), ExecuteInEditMode]
    public class GradientCircle : GraphicalColorTypeComponent
    {
        private SliderCircle2D slider;
        private bool dontListenToSlider;

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

        protected override void Awake()
        {
            base.Awake();
            slider = GetComponent<SliderCircle2D>();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            slider.onValueChanged.AddListener(UpdatePickerValues);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            slider.onValueChanged.RemoveListener(UpdatePickerValues);
        }

        private void UpdatePickerValues(float xValue, float yValue)
        {
            if (dontListenToSlider)
                return;

            if (Picker != null)
            {
                Picker.SetValueNormalized(ValueType1, xValue / 360f);
                Picker.SetValueNormalized(ValueType2, yValue);
            }

            DisplayNewColor();
        }

        private void UpdateSliderValues()
        {
            dontListenToSlider = true;
            slider.angle = Picker != null ? Picker.GetValueNormalized(ValueType1) * 360 : 0;
            slider.distance = Picker != null ? Picker.GetValueNormalized(ValueType2) : 0;
            dontListenToSlider = false;
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
                    Vector2 radius = new Vector2((v.z - v.x), (v.w - v.y)) * 0.5f;
                    Vector3 center = new Vector3(v.x + radius.x, v.y + radius.y);

                    // Place center vertices
                    float distance = 0;
                    for (int sideIndex = 0; sideIndex < slider.Corners; sideIndex++)
                    {
                        float angle = ((float)sideIndex / slider.Corners);
                        angle = (angle + ((float)(sideIndex + 1) / slider.Corners)) / 2f; // get center
                        vh.AddVert(center, GetColor(angle, distance), new Vector2(0.25f, 0.25f));
                    }

                    int previousCorner = 0;
                    int currentCorner = slider.Corners;
                    for (int depthIndex = 1; depthIndex < GetMinimum(ValueType2); depthIndex++)
                    {
                        // Place Top vert (Triangle: 3, 7, 14, etc)
                        Vector2 rad = (radius / (GetMinimum(ValueType2) - 1)) * depthIndex;
                        distance = (float)depthIndex / (GetMinimum(ValueType2) - 1);
                        Vector3 t = new Vector3(0, 1, 0);
                        vh.AddVert(center + new Vector3(t.x * rad.x, t.y * rad.y), GetColor(0, distance), new Vector2(0.25f, 0.25f));

                        for (int sideIndex = 1; sideIndex <= slider.Corners; sideIndex++)
                        {
                            float prevAngle = ((float)(sideIndex - 1) / slider.Corners);
                            float angle = ((float)sideIndex / slider.Corners);
                            Vector3 t2 = (Quaternion.AngleAxis(prevAngle * 360f, Vector3.forward) * new Vector3(0, 1, 0));
                            Vector3 temp = (Quaternion.AngleAxis(angle * 360f, Vector3.forward) * new Vector3(0, 1, 0));
                            Vector3 prevVert = center + new Vector3(t2.x * rad.x, t2.y * rad.y);
                            Vector3 nextVert = center + new Vector3(temp.x * rad.x, temp.y * rad.y);

                            for (int vertIndex = 1; vertIndex < depthIndex; vertIndex++)
                            {
                                // Places one vert and 2 trianlges
                                float a = Mathf.Lerp(prevAngle, angle, (float)vertIndex / depthIndex);
                                vh.AddVert(Vector3.Lerp(prevVert, nextVert, (float)vertIndex / depthIndex), GetColor(a, distance), new Vector2(0.25f, 0.25f));

                                vh.AddTriangle(currentCorner + (depthIndex * (sideIndex - 1)) + (vertIndex - 1),
                                    currentCorner + (depthIndex * (sideIndex - 1)) + vertIndex,
                                    previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex - 1);
                                vh.AddTriangle(currentCorner + (depthIndex * (sideIndex - 1)) + vertIndex,
                                    previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex,
                                    previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex - 1);
                            }

                            // Place corner vert
                            vh.AddVert(nextVert, GetColor(angle, distance), new Vector2(0.25f, 0.25f));
                            vh.AddTriangle(currentCorner + (depthIndex * sideIndex) - 1, currentCorner + (depthIndex * sideIndex), previousCorner + ((depthIndex - 1) * sideIndex));
                            if (previousCorner < slider.Corners)
                                previousCorner++;
                        }
                        previousCorner = currentCorner;
                        currentCorner += (slider.Corners * depthIndex) + 1;
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

                Vector2 radius = new Vector2((v.z - v.x), (v.w - v.y)) * 0.5f;
                Vector3 center = new Vector3(v.x + radius.x, v.y + radius.y);

                // Place center vertices
                float distance = 0;
                for (int sideIndex = 0; sideIndex < slider.Corners; sideIndex++)
                {
                    float angle = ((float)sideIndex / slider.Corners);
                    angle = (angle + ((float)(sideIndex + 1) / slider.Corners)) / 2f; // get center
                    vh.AddVert(center, GetColor(angle, distance), new Vector2(0.25f, 0.25f));
                }

                int previousCorner = 0;
                int currentCorner = slider.Corners;
                for (int depthIndex = 1; depthIndex < GetMinimum(ValueType2); depthIndex++)
                {
                    // Place Top vert (Triangle: 3, 7, 14, etc)
                    Vector2 rad = (radius / (GetMinimum(ValueType2) - 1)) * depthIndex;
                    distance = (float)depthIndex / (GetMinimum(ValueType2) - 1);
                    Vector3 t = new Vector3(0, 1, 0);
                    vh.AddVert(center + new Vector3(t.x * rad.x, t.y * rad.y), GetColor(0, distance), new Vector2(0.25f, 0.25f));

                    for (int sideIndex = 1; sideIndex <= slider.Corners; sideIndex++)
                    {
                        float prevAngle = ((float)(sideIndex - 1) / slider.Corners);
                        float angle = ((float)sideIndex / slider.Corners);
                        Vector3 t2 = (Quaternion.AngleAxis(prevAngle * 360f, Vector3.forward) * new Vector3(0, 1, 0));
                        Vector3 temp = (Quaternion.AngleAxis(angle * 360f, Vector3.forward) * new Vector3(0, 1, 0));
                        Vector3 prevVert = center + new Vector3(t2.x * rad.x, t2.y * rad.y);
                        Vector3 nextVert = center + new Vector3(temp.x * rad.x, temp.y * rad.y);

                        for (int vertIndex = 1; vertIndex < depthIndex; vertIndex++)
                        {
                            // Places one vert and 2 trianlges
                            float a = Mathf.Lerp(prevAngle, angle, (float)vertIndex / depthIndex);
                            vh.AddVert(Vector3.Lerp(prevVert, nextVert, (float)vertIndex / depthIndex), GetColor(a, distance), new Vector2(0.25f, 0.25f));

                            vh.AddTriangle(currentCorner + (depthIndex * (sideIndex - 1)) + (vertIndex - 1),
                                currentCorner + (depthIndex * (sideIndex - 1)) + vertIndex,
                                previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex - 1);
                            vh.AddTriangle(currentCorner + (depthIndex * (sideIndex - 1)) + vertIndex,
                                previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex,
                                previousCorner + ((depthIndex - 1) * (sideIndex - 1)) + vertIndex - 1);
                        }

                        // Place corner vert
                        vh.AddVert(nextVert, GetColor(angle, distance), new Vector2(0.25f, 0.25f));
                        vh.AddTriangle(currentCorner + (depthIndex * sideIndex) - 1, currentCorner + (depthIndex * sideIndex), previousCorner + ((depthIndex - 1) * sideIndex));
                        if (previousCorner < slider.Corners)
                            previousCorner++;
                    }
                    previousCorner = currentCorner;
                    currentCorner += (slider.Corners * depthIndex) + 1;
                }
            }
        }
#endif

        private Color32 GetColor(float angle, float distance)
        {
            return ColorPicker.GetValue(ValueType1, slider.InverseAngle ? 1f - angle : angle, ValueType2, distance, ValueType3, GetValue3(), 255);
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled)
                return;

            UpdateSliderValues();

            SetVerticesDirty();
        }

        private int GetMinimum(ColorType type)
        {
            switch (type)
            {
                case ColorType.RGB_R:
                case ColorType.RGB_G:
                case ColorType.RGB_B:
                case ColorType.HSV_V:
                case ColorType.HSL_S:
                    return 2;
                case ColorType.HSV_S:
                    return 2;
                case ColorType.HSV_H:
                case ColorType.HSL_H:
                    return 7;
                case ColorType.HSL_L:
                    return 3;
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}
