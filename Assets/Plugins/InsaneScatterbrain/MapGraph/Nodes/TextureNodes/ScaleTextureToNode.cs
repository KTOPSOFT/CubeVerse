using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Scales the texture to the given size.
    /// </summary>
    [ScriptNode("Scale Texture To", "Textures"), Serializable]
    public class ScaleTextureToNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField] 
        private InPort textureIn = null;
        
        [InPort("Target Size", typeof(Vector2Int), true), SerializeField] 
        private InPort targetSizeIn = null;
        

        [OutPort("Texture", typeof(TextureData)), SerializeField] 
        private OutPort textureOut = null;
        
        private TextureData textureData;
        
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var originalTextureData = textureIn.Get<TextureData>();
            var targetSize = targetSizeIn.Get<Vector2Int>();

            textureData = instanceProvider.Get<TextureData>();

            originalTextureData.Scale(ref textureData, targetSize.x, targetSize.y);

            textureOut.Set(() => textureData);
        }
    }
}