using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Applies cellular automata smoothing on the provided texture.
    /// This implementation is here for legacy purposes so the new one wouldn't break any existing graphs
    /// depending on it. The implementation is less accurate than the new one and you'll probably want
    /// to use that one instead. 
    /// </summary>
    [ScriptNode("Cellular Automata Smoothing (Legacy)", "Textures"), Serializable]
    public class CellularAutomataSmoothingNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Number of Passes", typeof(int), true), SerializeReference]
        private InPort passesIn = null;
        
        [InPort("Fill Color", typeof(Color32)), SerializeReference]
        private InPort fillColorIn = null;
        
        [InPort("Empty Color", typeof(Color32)), SerializeReference]
        private InPort emptyColorIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;

        
        private Color32 fillColor;
        private Color32 emptyColor;
        
        private int width;
        private int height;
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        /// <summary>
        /// Gets whether a pixel coordinate is in a valid range for this texture.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>True if it's a valid coordinate, false otherwise.</returns>
        private bool IsInTextureRange(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
        /// <summary>
        /// Gets the number of neighbouring pixels that are filled.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The number of filled neighbouring pixels.</returns>
        private int GetSurroundingFilledCount(int x, int y)
        {
            var filledCount = 0;
            
            for (var neighbourX = x - 1; neighbourX <= x + 1; ++neighbourX)
            {
                for (var neighbourY = y - 1; neighbourY <= y + 1; ++neighbourY)
                {
                    if (!IsInTextureRange(neighbourX, neighbourY))
                    { 
                        // Pixels out of bounds also count as filled.
                        filledCount++;
                        continue;
                    }

                    var index = neighbourY * width + neighbourX;
                    filledCount += textureData[index].IsEqualTo(fillColor) ? 1 : 0;
                }
            }
            
            return filledCount;
        }

        /// <summary>
        /// Applies a single pass of automata smoothing on the texture.
        /// </summary>
        private void ApplySmoothing()
        {
            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    var neighbourFilledTiles = GetSurroundingFilledCount(x, y);

                    var index = y * width + x;
                    textureData[index] = neighbourFilledTiles > 4 ? fillColor : emptyColor;
                }
            }
        }

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var passes = passesIn.Get<int>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            fillColor = fillColorIn.Get<Color32>();
            emptyColor = emptyColorIn.Get<Color32>();

            width = textureData.Width;
            height = textureData.Height;

            for (var i = 0; i < passes; ++i)
            {
                ApplySmoothing();
            }

            textureOut.Set(() => textureData);
        }
    }
}