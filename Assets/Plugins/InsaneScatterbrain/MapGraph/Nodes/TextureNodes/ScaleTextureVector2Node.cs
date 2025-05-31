using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Scales the texture by the given ratio.
    /// </summary>
    [ScriptNode("Scale Texture (Vector2)", "Textures"), Serializable]
    public class ScaleTextureVector2Node : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField] 
        private InPort textureIn = null;
        
        [InPort("Scale", typeof(Vector2), true), SerializeField] 
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
            var scale = scaleIn.Get<Vector2>();

            var newWidth = Mathf.FloorToInt(originalTextureData.Width * scale.x);
            var newHeight = Mathf.FloorToInt(originalTextureData.Height * scale.y);

            textureData = instanceProvider.Get<TextureData>();

            originalTextureData.Scale(ref textureData, newWidth, newHeight);

            textureOut.Set(() => textureData);
        }
    }
}
