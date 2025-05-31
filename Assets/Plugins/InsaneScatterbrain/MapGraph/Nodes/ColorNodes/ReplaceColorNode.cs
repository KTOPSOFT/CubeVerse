using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Replaces all pixels of one color with another color.
    /// </summary>
    [ScriptNode("Replace Color", "Colors"), Serializable]
    public class ReplaceColorNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        [InPort("Find", typeof(Color32)), SerializeReference] 
        private InPort findColorIn = null;
        
        [InPort("Replace", typeof(Color32)), SerializeReference] 
        private InPort replaceColorIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;
        
        
        private TextureData textureData;

#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest generated texture data.
        /// </summary>
        public TextureData TextureData => textureData;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var findColor = findColorIn.Get<Color32>();
            var replaceColor = replaceColorIn.Get<Color32>();
            
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            if (findColor.IsEqualTo(replaceColor))
            {
                textureOut.Set(() => textureData);
                return;
            }

            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                if (!textureData[i].IsEqualTo(findColor)) continue;

                textureData[i] = replaceColor;
            }

            textureOut.Set(() => textureData);
        }
    }
}