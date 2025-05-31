using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Applies a mask to a texture, meaning that it replaces colors with transparency if their position is masked.
    /// </summary>
    [ScriptNode("Apply Mask", "Masks"), Serializable]
    public class ApplyMaskNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Mask", typeof(Mask), true), SerializeReference] 
        private InPort maskIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        

        private TextureData textureData;
#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data. Only available in the editor.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();

            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var mask = maskIn.Get<Mask>();

            for (var i = 0; i < textureData.ColorCount; i++)
            {
                if (!mask.IsPointMasked(i)) continue;
                
                textureData[i] = default;
            }
            
            textureOut.Set(() => textureData);
        }
    }
}