using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Stamps one texture on top of another.
    /// </summary>
    [ScriptNode("Stamp", "Drawing"), Serializable]
    public class StampNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Stamp", typeof(TextureData), true), SerializeReference] 
        private InPort stampIn = null;

        [InPort("Offset", typeof(Vector2Int)), SerializeReference]
        private InPort offsetIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var stampData = stampIn.Get<TextureData>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var offset = offsetIn.Get<Vector2Int>();
            
            var stampWidth = stampData.Width;
            var width = textureData.Width;
            var height = textureData.Height;

            for (var i = 0; i < stampData.ColorCount; ++i)
            {
                var color = stampData[i];

                if (color.a == 0) continue;

                var x = i % stampWidth + offset.x;
                var y = i / stampWidth + offset.y;
                
                if (x < 0 || x >= width || y < 0 || y >= height) continue;

                var index = y * width + x;
                
                textureData[index] = color;
            }

            textureOut.Set(() => textureData);
        }
    }
}