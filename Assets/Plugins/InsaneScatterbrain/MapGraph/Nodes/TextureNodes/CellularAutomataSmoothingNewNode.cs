using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Applies cellular automata smoothing on the provided texture.
    /// </summary>
    [ScriptNode("Cellular Automata Smoothing", "Textures"), Serializable]
    public class CellularAutomataSmoothingNewNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;

        [InPort("Number of Passes", typeof(int), true), SerializeReference]
        private InPort passesIn = null;

        [InPort("Fill Color", typeof(Color32)), SerializeReference]
        private InPort fillColorIn = null;

        [InPort("Empty Color", typeof(Color32)), SerializeReference]
        private InPort emptyColorIn = null;

        [InPort("Neighbor Threshold", typeof(int)), SerializeReference]
        private InPort neighborThresholdIn = null;

        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;

        private Color32 fillColor;
        private Color32 emptyColor;
        private int neighborThreshold;
        private int width;
        private int height;
        private TextureData readBuffer;
        private TextureData writeBuffer;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => readBuffer;
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
            for (var neighbourY = y - 1; neighbourY <= y + 1; ++neighbourY)
            {
                if (!IsInTextureRange(neighbourX, neighbourY))
                { 
                    // Pixels out of bounds also count as filled.
                    filledCount++;
                    continue;
                }

                var index = neighbourY * width + neighbourX;
                filledCount += readBuffer[index].IsEqualTo(fillColor) ? 1 : 0;
            }
            
            return filledCount;
        }

        /// <summary>
        /// Applies a single pass of automata smoothing on the texture.
        /// </summary>
        private void ApplySmoothing()
        {
            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var neighbourFilledTiles = GetSurroundingFilledCount(x, y);

                var index = y * width + x;
                writeBuffer[index] = neighbourFilledTiles > neighborThreshold ? fillColor : emptyColor;
            }

            // Swap buffers, so we read from the buffer with the latest smoothing results, in any following passes.
            (readBuffer, writeBuffer) = (writeBuffer, readBuffer);
        }

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();

            var passes = passesIn.Get<int>();
            if (passes < 0)
            {
                throw new ArgumentException("Number of passes cannot be negative.");
            }

            readBuffer = instanceProvider.Get<TextureData>();
            writeBuffer = instanceProvider.Get<TextureData>();
            
            // Clone the input texture data to avoid modifying the original data, while applying the smoothing.
            textureIn.Get<TextureData>().Clone(readBuffer);
            textureIn.Get<TextureData>().Clone(writeBuffer);

            fillColor = fillColorIn.Get<Color32>();
            emptyColor = emptyColorIn.Get<Color32>();
            neighborThreshold = neighborThresholdIn.IsConnected ? neighborThresholdIn.Get<int>() : 4;

            width = readBuffer.Width;
            height = readBuffer.Height;

            for (var i = 0; i < passes; ++i)
            {
                ApplySmoothing();
            }

            // After the last smoothing pass, the read buffer contains the final result.
            textureOut.Set(() => readBuffer);
        }
    }
}