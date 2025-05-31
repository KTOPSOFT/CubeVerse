using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Converts texture to a texture data object.
    /// </summary>
    [ScriptNode("Texture To Data", "Textures"), Serializable]
    public class TextureToDataNode : ProcessorNode
    {
        [InPort("Texture", typeof(Texture2D), true), SerializeReference]
        private InPort textureIn = null;

        [OutPort("Data", typeof(TextureData)), SerializeReference] 
        private OutPort textureDataOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcessMainThread()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var texture = textureIn.Get<Texture2D>();

            var textureData = instanceProvider.Get<TextureData>();
            TextureData.CreateFromTexture(textureData, texture);
            textureDataOut.Set(() => textureData);
        }
    }
}