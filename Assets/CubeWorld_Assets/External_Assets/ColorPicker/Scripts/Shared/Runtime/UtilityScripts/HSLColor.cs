using System;
using UnityEngine;

namespace AdvancedColorPicker
{
    /// <summary>
    /// Describes a color in HSL values
    /// </summary>
    [Serializable]
    public struct HSLColor : IEquatable<HSLColor>
    {
        public const ColorType Flag = (ColorType)(1 << 7 | 1 << 8 | 1 << 9);

        /// <summary>
        /// Hue, ranges between 0 and 360
        /// </summary>
        public double H;

        /// <summary>
        /// Saturation, ranges between 0 and 1
        /// </summary>
        public double S;

        /// <summary>
        /// Light, ranges between 0 and 1
        /// </summary>
        public double L;

        /// <summary>
        /// Normalized Hue, ranges between 0 and 1
        /// </summary>
        public float NormalizedH
        {
            get
            {
                return (float)H / 360;
            }
            set
            {
                H = value * 360;
            }
        }

        public float NormalizedS
        {
            get
            {
                return (float)S;
            }
        }

        public float NormalizedL
        {
            get
            {
                return (float)L;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Range between 0 and 360</param>
        /// <param name="s">Range between 0 and 1</param>
        /// <param name="l">Range between 0 and 1</param>
        public HSLColor(double h, double s, double l)
        {
            this.H = h;
            this.S = s;
            this.L = l;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Range between 0 and 1</param>
        /// <param name="s">Range between 0 and 1</param>
        /// <param name="l">Range between 0 and 1</param>
        public HSLColor(float h, float s, float l)
        {
            this.H = h * 360;
            this.S = s;
            this.L = l;
        }

        /// <summary>
        /// Creates HSL values based upon HSV values.
        /// </summary>
        /// <param name="hsv"></param>
        public HSLColor(HSVColor hsv)
        {
            H = hsv.H;
            L = (2 - hsv.S) * hsv.V;
            if (L == 0 || L == 2)
                S = hsv.S;
            else
                S = hsv.S * hsv.V / (L <= 1d ? L : 2 - L);
            L *= 0.5d;
        }

        /// <summary>
        /// Creates HSL values based upon RGB values.
        /// </summary>
        /// <param name="rgb"></param>
        public HSLColor(RGBColor rgb)
        {
            double _Min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            double _Max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            double _Delta = _Max - _Min;

            H = 0;
            S = 0;
            L = (_Max + _Min) * 0.5d;

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (_Delta / (_Max + _Min));
                }
                else
                {
                    S = (_Delta / (2.0d - _Max - _Min));
                }


                if (rgb.R == _Max)
                {
                    H = (rgb.G - rgb.B) / _Delta;
                }
                else if (rgb.G == _Max)
                {
                    H = 2d + (rgb.B - rgb.R) / _Delta;
                }
                else if (rgb.B == _Max)
                {
                    H = 4d + (rgb.R - rgb.G) / _Delta;
                }
                H *= 60;
                if (H < 0)
                    H += 360;
            }
        }

        public Color32 ToColor(byte alpha)
        {
            return new RGBColor(this).ToColor(alpha);
        }

        public override string ToString()
        {
            return H + " : " + S + " : " + L;
        }

        public bool Equals(HSLColor other)
        {
            return H == other.H && S == other.S && L == other.L;
        }

        public static bool operator ==(HSLColor one, HSLColor two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(HSLColor one, HSLColor two)
        {
            return !one.Equals(two);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is HSLColor && Equals((HSLColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)H;
                hash = hash * 23 + (int)S;
                hash = hash * 23 + (int)L;
                return hash;
            }
        }
    }
}
