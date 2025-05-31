using InsaneScatterbrain.Extensions;
using UnityEngine;

namespace InsaneScatterbrain.Services
{
    /// <summary>
    /// Class to help create Texture2Ds.
    /// </summary>
    public static class Texture2DFactory
    {
        /// <summary>
        /// Creates a new instance of Texture2D with a set of default specifications.
        /// </summary>
        /// <param name="width">The texture width.</param>
        /// <param name="height">The texture height.</param>
        /// <param name="fill">If true the texture will be automatically filled with transparent pixels.</param>
        /// <returns>The created texture.</returns>
        public static Texture2D CreateDefault(int width, int height, bool fill = true)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {filterMode = FilterMode.Point};
            if (fill)
            {
                texture.Fill(new Color(0,0,0,0));
            }
            return texture;
        }
    }
}