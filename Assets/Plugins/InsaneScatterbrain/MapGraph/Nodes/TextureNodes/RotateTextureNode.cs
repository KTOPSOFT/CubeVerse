using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Rotates a texture around its center.
    /// </summary>
    [ScriptNode("Rotate Texture", "Textures"), Serializable]
    public class RotateTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Angle", typeof(float), true), SerializeReference]
        private InPort angleIn = null;

        [InPort("Rotate Around", typeof(Vector2)), SerializeReference]
        private InPort rotateAroundIn = null;
        
        
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
            var angle = angleIn.Get<float>();

            var width = originalTextureData.Width;
            var height = originalTextureData.Height;
            
            var rotateAround = rotateAroundIn.IsConnected 
                ? rotateAroundIn.Get<Vector2>() 
                : new Vector2((width - 1) / 2f, (height - 1) / 2f);

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);

            for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var v = new Vector2Int(x, y).RotateAround(rotateAround, -angle);
                
                if (v.x < 0 || v.x >= width || v.y < 0 || v.y >= height) continue;

                var index = y * width + x;
                var rotatedIndex = v.y * width + v.x;
                
                textureData[index] = originalTextureData[rotatedIndex];
            }
            
            textureOut.Set(() => textureData);
        }
    }
}