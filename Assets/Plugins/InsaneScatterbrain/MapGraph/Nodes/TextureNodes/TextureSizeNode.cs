using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs the texture's width and height.
    /// </summary>
    [ScriptNode("Texture Size", "Textures"), Serializable]
    public class TextureSizeNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField]
        private InPort textureIn = null;
        
        [OutPort("Size", typeof(Vector2Int)), SerializeField] 
        private OutPort sizeOut = null;

        protected override void OnProcess()
        {
            var textureData = textureIn.Get<TextureData>();
            var size = new Vector2Int(textureData.Width, textureData.Height);
            sizeOut.Set(() => size);
        }
    }
}