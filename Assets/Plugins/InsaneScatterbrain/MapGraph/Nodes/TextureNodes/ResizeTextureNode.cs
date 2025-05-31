using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Resizes a texture.
    /// </summary>
    [ScriptNode("Resize Texture", "Textures"), Serializable]
    public class ResizeTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("New Size", typeof(Vector2Int), true), SerializeReference]
        private InPort newSizeIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;
        
        
        private TextureData textureData;
        public TextureData TextureData => textureData;

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            var newSize = newSizeIn.Get<Vector2Int>();
            
            textureData.Resize(newSize.x, newSize.y);
            
            textureOut.Set(() => textureData);
        }
    }
}