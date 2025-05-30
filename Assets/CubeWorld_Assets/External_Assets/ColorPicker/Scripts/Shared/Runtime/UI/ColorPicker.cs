using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AdvancedColorPicker
{
    public class ColorPicker : MonoBehaviour
    {
        [Serializable]
        public class ColorChangedEvent : UnityEvent<Color> { }


        [SerializeField]
        private RGBColor rgbColor = new RGBColor(0, 0, 0);

        [SerializeField]
        private HSVColor hsvColor = new HSVColor(0, 0, 0);

        [SerializeField]
        private HSLColor hslColor = new HSLColor(0, 0, 0);

        [SerializeField]
        private byte _alpha = 255;

        public ColorChangedEvent OnColorChanged = new ColorChangedEvent();

        public Color CurrentColor
        {
            get
            {
                return rgbColor.ToColor(_alpha);
            }
            set
            {
                if (rgb.R == value.r && rgb.G == value.g && rgb.B == value.b && _alpha == (byte)(value.a * 255))
                    return;

#if UNITY_EDITOR
                MarkUndo("rgba");
#endif
                rgbColor = new RGBColor(value);
                _alpha = (byte)Math.Round(value.a * 255);
                RGBChanged();
            }
        }

        public RGBColor rgb
        {
            get
            {
                return rgbColor;
            }
            set
            {
                if (rgbColor == value)
                    return;
#if UNITY_EDITOR
                MarkUndo("rgb");
#endif
                rgbColor = value;
                RGBChanged();
            }
        }

        public HSVColor hsv
        {
            get
            {
                return hsvColor;
            }
            set
            {
                if (hsvColor == value)
                    return;

#if UNITY_EDITOR
                MarkUndo("hsv");
#endif

                hsvColor = value;
                HSVChanged();
            }
        }

        public HSLColor hsl
        {
            get
            {
                return hslColor;
            }
            set
            {
                if (hslColor == value)
                    return;

#if UNITY_EDITOR
                MarkUndo("hsl");
#endif

                hslColor = value;
                HSLChanged();
            }
        }

        public byte Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                if (_alpha == value)
                    return;

#if UNITY_EDITOR
                MarkUndo("alpha");
#endif

                _alpha = value;
                SendChangedEvent();
            }
        }

        public float AlphaNormalized
        {
            get
            {
                return (float)_alpha / 255f;
            }
            set
            {
                Alpha = (byte)Math.Round(Mathf.Clamp01(value) * 255);
            }
        }

        private void RGBChanged()
        {
            hsvColor = new HSVColor(rgbColor);
            hslColor = new HSLColor(rgbColor);

            SendChangedEvent();
        }

        private void HSVChanged()
        {
            rgbColor = new RGBColor(hsvColor);
            hslColor = new HSLColor(hsvColor);

            SendChangedEvent();
        }

        private void HSLChanged()
        {
            rgbColor = new RGBColor(hslColor);
            hsvColor = new HSVColor(hslColor);

            SendChangedEvent();
        }

        private void SendChangedEvent()
        {
            OnColorChanged.Invoke(CurrentColor);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public float GetValueNormalized(ColorType type)
        {
            return GetValueNormalized((ColorValueType)type);
        }

        public float GetValueNormalized(ColorValueType type)
        {
            switch (type)
            {
                case ColorValueType.Alpha:
                    return AlphaNormalized;
                case ColorValueType.RGB_R:
                    return (float)rgbColor.R;
                case ColorValueType.RGB_G:
                    return (float)rgbColor.G;
                case ColorValueType.RGB_B:
                    return (float)rgbColor.B;
                case ColorValueType.HSV_H:
                    return hsvColor.NormalizedH;
                case ColorValueType.HSV_S:
                    return hsvColor.NormalizedS;
                case ColorValueType.HSV_V:
                    return hsvColor.NormalizedV;
                case ColorValueType.HSL_H:
                    return hslColor.NormalizedH;
                case ColorValueType.HSL_S:
                    return hslColor.NormalizedS;
                case ColorValueType.HSL_L:
                    return hslColor.NormalizedL;
                default:
                    throw new System.NotImplementedException(type.ToString());
            }
        }

        public void SetValueNormalized(ColorType type, float value)
        {
            SetValueNormalized((ColorValueType)type, value);
        }

        public void SetValueNormalized(ColorValueType type, float value)
        {
            switch (type)
            {
                case ColorValueType.Alpha:
                    AlphaNormalized = value;
                    break;
                case ColorValueType.RGB_R:
                    rgb = new RGBColor(value, rgbColor.G, rgbColor.B);
                    break;
                case ColorValueType.RGB_G:
                    rgb = new RGBColor(rgbColor.R, value, rgbColor.B);
                    break;
                case ColorValueType.RGB_B:
                    rgb = new RGBColor(rgbColor.R, rgbColor.G, value);
                    break;
                case ColorValueType.HSV_H:
                    hsv = new HSVColor(value * 360, hsvColor.S, hsvColor.V);
                    break;
                case ColorValueType.HSV_S:
                    hsv = new HSVColor(hsvColor.H, value, hsvColor.V);
                    break;
                case ColorValueType.HSV_V:
                    hsv = new HSVColor(hsvColor.H, hsvColor.S, value);
                    break;
                case ColorValueType.HSL_H:
                    hsl = new HSLColor(value * 360, hslColor.S, hslColor.L);
                    break;
                case ColorValueType.HSL_S:
                    hsl = new HSLColor(hsl.H, value, hsl.L);
                    break;
                case ColorValueType.HSL_L:
                    hsl = new HSLColor(hsl.H, hsl.S, value);
                    break;
                default:
                    throw new System.NotImplementedException(type.ToString());
            }
        }

        public static Color32 GetValue(ColorType type1, float value1, ColorType type2, float value2, ColorType type3, float value3, byte alphaValue)
        {
            switch (type3)
            {
                case ColorType.RGB_R:
                    if (type1 == ColorType.RGB_G)
                        return new RGBColor(value3, value1, value2).ToColor(alphaValue);
                    else
                        return new RGBColor(value3, value2, value1).ToColor(alphaValue);
                case ColorType.RGB_G:
                    if (type1 == ColorType.RGB_R)
                        return new RGBColor(value1, value3, value2).ToColor(alphaValue);
                    else
                        return new RGBColor(value2, value3, value1).ToColor(alphaValue);
                case ColorType.RGB_B:
                    if (type1 == ColorType.RGB_R)
                        return new RGBColor(value1, value2, value3).ToColor(alphaValue);
                    else
                        return new RGBColor(value2, value1, value3).ToColor(alphaValue);
                case ColorType.HSV_H:
                    if (type1 == ColorType.HSV_S)
                        return new HSVColor(value3, value1, value2).ToColor(alphaValue);
                    else
                        return new HSVColor(value3, value2, value1).ToColor(alphaValue);
                case ColorType.HSV_S:
                    if (type1 == ColorType.HSV_H)
                        return new HSVColor(value1, value3, value2).ToColor(alphaValue);
                    else
                        return new HSVColor(value2, value3, value1).ToColor(alphaValue);
                case ColorType.HSV_V:
                    if (type1 == ColorType.HSV_H)
                        return new HSVColor(value1, value2, value3).ToColor(alphaValue);
                    else
                        return new HSVColor(value2, value1, value3).ToColor(alphaValue);
                case ColorType.HSL_H:
                    if (type1 == ColorType.HSL_S)
                        return new HSLColor(value3, value1, value2).ToColor(alphaValue);
                    else
                        return new HSLColor(value3, value2, value1).ToColor(alphaValue);
                case ColorType.HSL_S:
                    if (type1 == ColorType.HSL_H)
                        return new HSLColor(value1, value3, value2).ToColor(alphaValue);
                    else
                        return new HSLColor(value2, value3, value1).ToColor(alphaValue);
                case ColorType.HSL_L:
                    if (type1 == ColorType.HSL_H)
                        return new HSLColor(value1, value2, value3).ToColor(alphaValue);
                    else
                        return new HSLColor(value2, value1, value3).ToColor(alphaValue);
                default:
                    throw new System.NotImplementedException(type3.ToString());
            }
        }

#if UNITY_EDITOR
        private void MarkUndo(string name)
        {
            UnityEditor.Undo.RecordObject(this, name + " changed");
        }
#endif
    }
}