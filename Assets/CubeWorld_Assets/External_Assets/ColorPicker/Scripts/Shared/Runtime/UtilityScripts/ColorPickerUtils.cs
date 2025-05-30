using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace AdvancedColorPicker
{
    public static class ColorPickerUtils
    {
        private static Texture2D _checkboard = null;
        private const string hexRegex = "^#?(?:[0-9a-fA-F]{3,4}){1,2}$";
        private const string PlayerPrefsSwatchKey = "ACP_Swatch_";

        /// <summary>
        /// The checkboard background texture used by all ColorPicker components
        /// </summary>
        public static Texture2D Checkboard
        {
            get
            {
                if (_checkboard == null)
                {
                    int checkboardsize = 8;

                    _checkboard = new Texture2D(checkboardsize, checkboardsize);
                    _checkboard.hideFlags = HideFlags.HideAndDontSave;

                    Color32[] pixels = new Color32[checkboardsize * checkboardsize];
                    for (int x = 0; x < checkboardsize; x++)
                    {
                        for (int y = 0; y < checkboardsize; y++)
                        {
                            byte value = ((x < checkboardsize / 2 && y < checkboardsize / 2) || (x >= checkboardsize / 2 && y >= checkboardsize / 2)) ? (byte)255 : (byte)100;
                            pixels[x + (y * checkboardsize)] = new Color32(value, value, value, 255);
                        }
                    }
                    _checkboard.SetPixels32(pixels);
                    _checkboard.Apply();
                }
                return _checkboard;
            }
        }

        /// <summary>
        /// Returns true when the given format is valid, false otherwise
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <remarks>
        /// Helper extension method that checks whether the given format is valid for the target.
        /// </remarks>
        public static bool IsFormatValid<T>(this T target, string Format) where T : IFormattable
        {
            try
            {
                target.ToString(Format, null);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static string ColorToHex(Color32 color, bool hashtag, bool alpha)
        {
            string result = null;

            if (hashtag) result = "#";

            result += string.Format("{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);

            if (alpha) result += string.Format("{0:X2}", color.a);

            return result;
        }

        public static bool HexToColor(string hex, HexfieldType acceptedInput, byte defaultAlpha, out Color32 color)
        {
            // Check if this could be a valid hex string (# is optional)
            if (System.Text.RegularExpressions.Regex.IsMatch(hex, hexRegex))
            {
                int startIndex = hex.StartsWith("#") ? 1 : 0;

                if ((acceptedInput & HexfieldType.RRGGBBAA) != 0 && hex.Length == startIndex + 8) //#RRGGBBAA
                {
                    color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 6, 2), NumberStyles.AllowHexSpecifier));
                }
                else if ((acceptedInput & HexfieldType.RRGGBB) != 0 && hex.Length == startIndex + 6)  //#RRGGBB
                {
                    color = new Color32(byte.Parse(hex.Substring(startIndex, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 2, 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(hex.Substring(startIndex + 4, 2), NumberStyles.AllowHexSpecifier),
                        defaultAlpha);
                }
                else if ((acceptedInput & HexfieldType.RGBA) != 0 && hex.Length == startIndex + 4) //#RGBA
                {
                    color = new Color32(byte.Parse(new string(hex[startIndex], 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(new string(hex[startIndex + 1], 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(new string(hex[startIndex + 2], 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(new string(hex[startIndex + 3], 2), NumberStyles.AllowHexSpecifier));
                }
                else if ((acceptedInput & HexfieldType.RGB) != 0 && hex.Length == startIndex + 3)  //#RGB
                {
                    color = new Color32(byte.Parse(new string(hex[startIndex], 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(new string(hex[startIndex + 1], 2), NumberStyles.AllowHexSpecifier),
                        byte.Parse(new string(hex[startIndex + 2], 2), NumberStyles.AllowHexSpecifier),
                        defaultAlpha);
                }
                else
                {
                    color = new Color32();
                    return false;
                }

                return true;
            }
            else
            {
                color = new Color32();
                return false;
            }
        }

        public static string GetSwatchKey(string name)
        {
            return PlayerPrefsSwatchKey + name;
        }

        public static Color32[] LoadSwatch(string name)
		{
            string swatches = PlayerPrefs.GetString(GetSwatchKey(name));

            int count = swatches.Length / 8;
            Color32[] result = new Color32[count];
            for (int index = 0; index < count; index++)
            {
                string hex = swatches.Substring(index * 8, 8);
                if (HexToColor(hex, HexfieldType.RRGGBBAA, 0, out Color32 color))
                {
                    result[index] = color;
                }
                else
				{
                    // This should never happen unless you manually wrote to this PlayerPrefs string
                    Debug.LogWarning($"Error while loading hex '{hex}' for swatch from '{name}'. Used default color instead");
                    result[index] = new Color32();
				}
            }
            return result;
        }

        public static void SaveSwatch(string name, params Color32[] colors)
        {
            StringBuilder builder = new StringBuilder(colors.Length * 8);

            for (int index = 0; index < colors.Length; index++)
            {
                builder.Append(ColorToHex(colors[index], false, true));
            }

            PlayerPrefs.SetString(GetSwatchKey(name), builder.ToString());
        }

        public static void DeleteSwatch(string name)
		{
            string key = GetSwatchKey(name);
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);
		}
    }
}