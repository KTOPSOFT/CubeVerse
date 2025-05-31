using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Creates a color array of the given length, each element is filled with the given color.
        /// </summary>
        /// <param name="color">The color to fill the array with.</param>
        /// <param name="length">The length of the array.</param>
        /// <returns>An array of colors of given length filled with given color.</returns>
        public static Color32[] CreateArray(this Color32 color, int length)
        {
            var colors = new Color32[length];
            for (var i = 0; i < length; ++i)
            {
                colors[i] = color;
            }

            return colors;
        }
        
        /// <summary>
        /// Creates a color array of the given length, each element is filled with the given color.
        /// </summary>
        /// <param name="color">The color to fill the array with.</param>
        /// <param name="length">The length of the array.</param>
        /// <returns>An array of colors of given length filled with given color.</returns>
        public static Color[] CreateArray(this Color color, int length)
        {
            var colors = new Color[length];
            for (var i = 0; i < length; ++i)
            {
                colors[i] = color;
            }

            return colors;
        }

        /// <summary>
        /// Checks if another color is equal to the color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="otherColor">The other color.</param>
        /// <returns>True if the color and the other color are equal.</returns>
        public static bool IsEqualTo(this Color32 color, Color32 otherColor)
        {
            return
                color.r == otherColor.r &&
                color.g == otherColor.g &&
                color.b == otherColor.b &&
                color.a == otherColor.a;
        }
    }
}