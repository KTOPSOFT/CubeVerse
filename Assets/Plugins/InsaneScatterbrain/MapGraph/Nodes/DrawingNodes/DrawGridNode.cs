using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Draws grid lines onto a texture.
    /// </summary>
    [ScriptNode("Draw Grid", "Drawing"), Serializable]
    public class DrawGridNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Cell Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort cellSizeIn = null;
        
        [InPort("Line Width", typeof(int)), SerializeReference] 
        private InPort lineWidthIn = null;
        
        [InPort("Color", typeof(Color32)), SerializeReference] 
        private InPort colorIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;


        private TextureData textureData;
        public TextureData TextureData => textureData;
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var cellSize = cellSizeIn.Get<Vector2Int>();
            var lineWidth = lineWidthIn.IsConnected ? lineWidthIn.Get<int>() : 1;
            var color = colorIn.Get<Color32>();

            var width = textureData.Width;
            var height = textureData.Height;
            
            var xStep = cellSize.x + lineWidth;
            var yStep = cellSize.y + lineWidth;

            for (var x = 0; x < width; x += xStep)
            for (var lineX = 0; lineX < lineWidth; ++lineX)
            for (var y = 0; y < height; ++y)
            {
                var index = y * width + x + lineX;
                textureData[index] = color;
            } 
            
            for (var y = 0; y < height; y += yStep)
            for (var lineY = 0; lineY < lineWidth; ++lineY)
            for (var x = 0; x < width; ++x)
            {
                var index = (y + lineY) * width + x;
                textureData[index] = color;
            }

            textureOut.Set(() => textureData);
        }
    }
}