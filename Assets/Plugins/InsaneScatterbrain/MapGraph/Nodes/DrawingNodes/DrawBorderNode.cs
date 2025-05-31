using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws a border of the given color on the other edge of the texture.
    /// </summary>
    [ScriptNode("Draw Border", "Drawing"), Serializable]
    public class DrawBorderNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Width", typeof(int)), SerializeReference] 
        private InPort widthIn = null;

        [InPort("Border Color", typeof(Color32)), SerializeReference] 
        private InPort borderColorIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        [OutPort("Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var borderColor = borderColorIn.Get<Color32>();
            var borderWidth = widthIn.Get<int>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);

            if (!widthIn.IsConnected) borderWidth = 1;
            
            var width = textureData.Width;
            var height = textureData.Height;

            var mask = maskOut.IsConnected ? instanceProvider.Get<Mask>() : null;

            mask?.Set(width * height);

            for (var x = 0; x < width; ++x)
            {
                for (var i = 0; i < borderWidth; ++i)
                {
                    var row = i * width;

                    var topIndex = x + row;
                    var bottomIndex = width * (height - 1) + x - row;

                    if (topIndex > -1 && topIndex < textureData.ColorCount)
                    {
                        textureData[topIndex] = borderColor;        // Top
                        mask?.MaskPoint(topIndex);
                    }

                    if (bottomIndex > -1 && bottomIndex < textureData.ColorCount)
                    {
                        textureData[bottomIndex] = borderColor;     // Bottom
                        mask?.MaskPoint(bottomIndex);
                    }
                }
            }

            for (var y = 0; y < height; ++y)
            {
                for (var i = 0; i < borderWidth; ++i)
                {
                    var leftIndex = y * width + i;
                    var rightIndex = y * width + (width - 1) - i;

                    if (leftIndex > -1 && leftIndex < textureData.ColorCount)
                    {
                        textureData[leftIndex] = borderColor;       // Left
                        mask?.MaskPoint(leftIndex);
                    }

                    if (rightIndex > -1 && rightIndex < textureData.ColorCount)
                    {
                        textureData[rightIndex] = borderColor;      // Right
                        mask?.MaskPoint(rightIndex);
                    }
                }
            }
            
            textureOut.Set(() => textureData);

            if (mask != null)
            {
                maskOut.Set(() => mask);
            }
        }
    }
}