using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Flips the texture on the X and/or Y axis.
    /// </summary>
    [ScriptNode("Flip Texture", "Textures"), Serializable]
    public class FlipTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Horizontal", typeof(bool)), SerializeReference] 
        private InPort horizontalIn = null;
        
        [InPort("Vertical", typeof(bool)), SerializeReference] 
        private InPort verticalIn = null;

        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;


        private TextureData textureData;
        
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var originalTextureData = textureIn.Get<TextureData>();
            var horizontal = horizontalIn.Get<bool>();
            var vertical = verticalIn.Get<bool>();

            if (!horizontal && !vertical)
            {
                textureData = originalTextureData;
                textureOut.Set(() => textureData);
                return;
            }

            var width = originalTextureData.Width;
            var height = originalTextureData.Height;

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);
            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var flippedX = horizontal ? width - x - 1 : x;
                var flippedY = vertical ? height - y - 1 : y;

                var index = y * width + x;
                var flippedIndex = flippedY * width + flippedX;
                textureData[flippedIndex] = originalTextureData[index];
            }
            
            textureOut.Set(() => textureData);
        }
    }
}