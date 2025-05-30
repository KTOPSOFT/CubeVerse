using System;
using UnityEngine;

namespace AdvancedColorPicker
{
    /// <summary>
    /// Describes a color in RGB values
    /// </summary>
    [Serializable]
    public struct RGBColor : IEquatable<RGBColor>
    {
        public const ColorType Flag = (ColorType)(1 << 1 | 1 << 2 | 1 << 3);

        /// <summary>
        /// Red, ranges between 0 and 1
        /// </summary>
        public double R;

        /// <summary>
        /// Green, ranges between 0 and 1
        /// </summary>
        public double G;

        /// <summary>
        /// Blue, ranges between 0 and 1
        /// </summary>
        public double B;

        /// <summary>
        /// Red as a byte, ranges between 0 and 255
        /// </summary>
        public byte BR
        {
            get
            {
                return (byte)Math.Round(R * 255d);
            }
            set
            {
                R = (double)value / 255d;
            }
        }

        /// <summary>
        /// Green as a byte, ranges between 0 and 255
        /// </summary>
        public byte BG
        {
            get
            {
                return (byte)Math.Round(G * 255d);
            }
            set
            {
                G = (double)value / 255d;
            }
        }

        /// <summary>
        /// Blue as a byte, ranges between 0 and 255
        /// </summary>
        public byte BB
        {
            get
            {
                return (byte)Math.Round(B * 255d);
            }
            set
            {
                B = (double)value / 255d;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">Range between 0 and 255</param>
        /// <param name="g">Range between 0 and 255</param>
        /// <param name="b">Range between 0 and 255</param>
        public RGBColor(byte r, byte g, byte b)
        {
            R = (double)r / 255d;
            G = (double)g / 255d;
            B = (double)b / 255d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r">Range between 0 and 1</param>
        /// <param name="g">Range between 0 and 1</param>
        /// <param name="b">Range between 0 and 1</param>
        public RGBColor(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }

        public RGBColor(HSVColor hsv)
        {
            double calc = hsv.H / 60;
            int hi = Convert.ToInt32(Math.Floor(calc)) % 6;
            double f = calc - Math.Floor(calc);

            double v = hsv.V;
            double p = (hsv.V * (1 - hsv.S));
            double q = (hsv.V * (1 - f * hsv.S));
            double t = (hsv.V * (1 - (1 - f) * hsv.S));

            switch (hi)
            {
                case 0:
                    R = v;
                    G = t;
                    B = p;
                    break;
                case 1:
                    R = q;
                    G = v;
                    B = p;
                    break;
                case 2:
                    R = p;
                    G = v;
                    B = t;
                    break;
                case 3:
                    R = p;
                    G = q;
                    B = v;
                    break;
                case 4:
                    R = t;
                    G = p;
                    B = v;
                    break;
                default:
                    R = v;
                    G = p;
                    B = q;
                    break;
            }
        }

        public RGBColor(HSLColor hsl)
        {
            if (hsl.S == 0)
            {
                R = G = B = hsl.L;
            }
            else
            {
                double t1, t2;
                double th = hsl.H / 360d;

                if (hsl.L < 0.5d)
                {
                    t2 = hsl.L * (1d + hsl.S);
                }
                else
                {
                    t2 = (hsl.L + hsl.S) - (hsl.L * hsl.S);
                }
                t1 = 2d * hsl.L - t2;

                double tr, tg, tb;
                tr = th + (1.0d / 3.0d);
                tg = th;
                tb = th - (1.0d / 3.0d);

                R = ColorCalc(tr, t1, t2);
                G = ColorCalc(tg, t1, t2);
                B = ColorCalc(tb, t1, t2);
            }
        }

        private static double ColorCalc(double c, double t1, double t2)
        {

            if (c < 0) c += 1d;
            if (c > 1) c -= 1d;
            if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
            if (2.0d * c < 1.0d) return t2;
            if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
            return t1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public RGBColor(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
        }

        public Color32 ToColor(byte alpha)
        {
            return new Color32(this.BR, this.BG, this.BB, alpha);
        }

        public override string ToString()
        {
            return BR + " : " + BG + " : " + BB;
        }

        public bool Equals(RGBColor other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public static bool operator ==(RGBColor one, RGBColor two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(RGBColor one, RGBColor two)
        {
            return !one.Equals(two);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is RGBColor && Equals((RGBColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (int)BR;
                hash = hash * 23 + (int)BG;
                hash = hash * 23 + (int)BB;
                return hash;
            }
        }
    }
}
