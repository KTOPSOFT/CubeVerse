using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Scales the texture by the given ratio.
    /// </summary>
    [ScriptNode("Scale Texture", "Textures"), Serializable]
    public class ScaleTextureNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField] 
        private InPort textureIn = null;
        
        [InPort("Scale", typeof(float), true), SerializeField] 
        private InPort scaleIn = null;
        

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
            var scale = scaleIn.Get<float>();

            var newWidth = Mathf.FloorToInt(originalTextureData.Width * scale);
            var newHeight = Mathf.FloorToInt(originalTextureData.Height * scale);

            textureData = instanceProvider.Get<TextureData>();

            originalTextureData.Scale(ref textureData, newWidth, newHeight);

            textureOut.Set(() => textureData);
        }
    }
}
