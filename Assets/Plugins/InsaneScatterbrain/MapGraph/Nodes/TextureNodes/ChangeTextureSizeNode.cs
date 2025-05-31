using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Changes the texture's size by the specified amount.
    /// </summary>
    [ScriptNode("Change Texture Size", "Textures"), Serializable]
    public class ChangeTextureSizeNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Size Change", typeof(Vector2Int), true), SerializeReference]
        private InPort sizeChangeIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;
        
        
        private TextureData textureData;
        public TextureData TextureData => textureData;

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            var sizeChange = sizeChangeIn.Get<Vector2Int>();
            var newWidth = textureData.Width + sizeChange.x;
            var newHeight = textureData.Height + sizeChange.y;
            
            textureData.Resize(newWidth, newHeight);
            
            textureOut.Set(() => textureData);
        }
    }
}