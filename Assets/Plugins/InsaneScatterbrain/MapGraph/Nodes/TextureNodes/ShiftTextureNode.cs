using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Moves all the pixels inside the texture by the given amount inside the texture. Pixels that would "fall off"
    /// will be placed back onto the other side of the texture.
    /// </summary>
    [ScriptNode("Shift Texture", "Textures"), Serializable]
    public class ShiftTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;

        [InPort("Shift By", typeof(Vector2Int), true), SerializeReference]
        private InPort shiftByIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeField] 
        private OutPort textureOut = null;

        
        private TextureData textureData;
        
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var originalTextureData = textureIn.Get<TextureData>();
            var shiftBy = shiftByIn.Get<Vector2Int>();
            var width = originalTextureData.Width;
            var height = originalTextureData.Height;

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);

            for (var oldX = 0; oldX < width; ++oldX)
            for (var oldY = 0; oldY < height; ++oldY)
            {
                var newX = Calc.Mod(oldX + shiftBy.x, width);
                var newY = Calc.Mod(oldY + shiftBy.y, height);

                var oldIndex = oldY * width + oldX;
                var newIndex = newY * width + newX;
                textureData[newIndex] = originalTextureData[oldIndex];
            }
            
            textureOut.Set(() => textureData);
        }
    }
}