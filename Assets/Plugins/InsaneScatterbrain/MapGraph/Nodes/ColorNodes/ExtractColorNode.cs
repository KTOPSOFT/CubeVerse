using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Extract Color", "Colors"), Serializable]
    public class ExtractColorNode : ProcessorNode
    {
        [InPort("Texture",typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Color To Extract",typeof(Color32)), SerializeReference] 
        private InPort colorToExtractIn = null;
        
        
        [OutPort("Texture",typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        [OutPort("Mask",typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var originalTextureData = textureIn.Get<TextureData>();
            var colorToExtract = colorToExtractIn.Get<Color32>();

            var instanceProvider = Get<IInstanceProvider>();
            
            Mask mask = null;

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(originalTextureData.Width, originalTextureData.Height);

            if (maskOut.IsConnected)
            {
                mask = instanceProvider.Get<Mask>();
            }

            for (var i = 0; i < originalTextureData.ColorCount; i++)
            {
                if (originalTextureData[i].IsEqualTo(colorToExtract))
                {
                    textureData[i] = colorToExtract;
                }

                if (maskOut.IsConnected && !originalTextureData[i].IsEqualTo(colorToExtract))
                {
                    mask.UnmaskPoint(i);
                }
            }
            
            textureOut.Set(() => textureData);
            maskOut.Set(() => mask);
        }
    }
}