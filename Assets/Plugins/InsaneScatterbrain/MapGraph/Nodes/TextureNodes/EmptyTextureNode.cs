using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Empty Texture", "Textures"), Serializable]
    public class EmptyTextureNode : ProcessorNode
    {
        [InPort("Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort sizeIn = null;

        [InPort("Default Color", typeof(Color32)), SerializeReference]
        private InPort defaultColorIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference] 
        private InPort maskIn = null;
        
        
        [OutPort("Empty Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureOut = null;

        protected InPort SizeIn => sizeIn;
        protected OutPort TextureOut => textureOut;

        private TextureData textureData;
        
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var size = sizeIn.Get<Vector2Int>();
            var defaultColor = defaultColorIn.Get<Color32>();
            var mask = maskIn.Get<Mask>();

            textureData = instanceProvider.Get<TextureData>();
            textureData.Set(size.x, size.y);
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                if (maskIn.IsConnected && mask.IsPointMasked(i)) continue;
                
                textureData[i] = defaultColor;
            }

            textureOut.Set(() => textureData);
        }
    }
}