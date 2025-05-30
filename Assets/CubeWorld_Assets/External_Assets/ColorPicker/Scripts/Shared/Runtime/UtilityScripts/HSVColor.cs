using System;
using UnityEngine;

namespace AdvancedColorPicker
{
    /// <summary>
    /// Describes a color in HSV values
    /// </summary>
    [Serializable]
    public struct HSVColor : IEquatable<HSVColor>
    {
        public const ColorType Flag = (ColorType)(1 << 4 | 1 << 5 | 1 << 6);

        /// <summary>
        /// Hue, ranges between 0 and 360
        /// </summary>
        public double H;

        /// <summary>
        /// Saturation, ranges between 0 and 1
        /// </summary>
        public double S;

        /// <summary>
        /// Value (Brightness), ranges between 0 and 1
        /// </summary>
        public double V;

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
            set
            {
                S = value;
            }
        }

        public float NormalizedV
        {
            get
            {
                return (float)V;
            }
            set
            {
                V = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Range between 0 and 360</param>
        /// <param name="s">Range between 0 and 1</param>
        /// <param name="v">Range between 0 and 1</param>
        public HSVColor(double h, double s, double v)
        {
            this.H = h;
            this.S = s;
            this.V = v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h">Range between 0 and 1</param>
        /// <param name="s">Range between 0 and 1</param>
        /// <param name="v">Range between 0 and 1</param>
        public HSVColor(float h, float s, float v)
        {
            this.H = h * 360f;
            this.S = s;
            this.V = v;
        }

        /// <summary>
        /// Creates HSV values based upon RGB values
        /// </summary>
        /// <param name="rgb"></param>
        public HSVColor(RGBColor rgb)
        {
            double delta, min, max;
            H = 0;

            min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            max = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            delta = max - min;
            V = max;

            if (max == 0.0)
                S = 0;
            else
                S = delta / max;

            if (S != 0)
            {
                if (rgb.R == max)
                    H = (rgb.G - rgb.B) / delta;
                else if (rgb.G == max)
                    H = 2d + (rgb.B - rgb.R) / delta;
                else if (rgb.B == max)
                    H = 4d + (rgb.R - rgb.G) / delta;

                H *= 60;
                if (H < 0.0)
                    H += 360;
            }
        }

        /// <summary>
        /// Creates HSV values based upon hsl values
        /// </summary>
        /// <param name="hsl"></param>
        public HSVColor(HSLColor hsl)
        {
            H = hsl.H;

            double s = hsl.S;
            double l = hsl.L * 2;

            if (l <= 1)
                s *= l;
            else
                s *= (2 - l);

            V = l + s;
            if (V == 0)
                S = hsl.S;
            else
                S = (2 * s) / V;
            V *= 0.5d;
        }

        public Color32 ToColor(byte alpha)
        {
            return new RGBColor(this).ToColor(alpha);
        }

        public override string ToString()
        {
            return H + " : " + S + " : " + V;
        }

        public bool Equals(HSVColor other)
        {
            return H == other.H && S == other.S && V == other.V;
        }

        public static bool operator ==(HSVColor one, HSVColor two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(HSVColor one, HSVColor two)
        {
            return !one.Equals(two);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is HSVColor && Equals((HSVColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)H;
                hash = hash * 23 + (int)S;
                hash = hash * 23 + (int)V;
                return hash;
            }
        }
    }
}
