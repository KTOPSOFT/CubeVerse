using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Fills the texture completely with the given color.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="fillColor">The fill color.</param>
        public static void Fill(this Texture2D texture, Color fillColor)
        {
            var colors = texture.GetRawTextureData<Color32>();

            for (var i = 0; i < colors.Length; ++i)
            {
                colors[i] = fillColor;
            }
        }

        /// <summary>
        /// Creates a clone of the texture instance.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <returns>The clone texture.</returns>
        public static Texture2D Clone(this Texture2D texture)
        {
            var copy = Texture2DFactory.CreateDefault(texture.width, texture.height);
            Graphics.CopyTexture(texture, copy);
            return copy;
        }
    }
}