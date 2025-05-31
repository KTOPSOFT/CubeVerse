using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Create a mask from the texture. Any non-transparent pixels will be considered to be masked.
    /// </summary>
    [ScriptNode("Texture To Mask", "Masks"), Serializable]
    public class TextureToMaskNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        /// <summary>
        /// If true, the mask is inversed, meaning that any transparent pixels will be masked instead of the
        /// non-transparent pixels.
        /// </summary>
        [InPort("Inverse?", typeof(bool)), SerializeReference] 
        private InPort inverseIn = null;
        
        
        [OutPort("Mask", typeof(Mask)), SerializeReference]
        private OutPort maskOut = null;
        
        private Mask mask;
        private TextureData texture;
        
#if UNITY_EDITOR
        public Mask Mask => mask;

        public TextureData TextureData => texture;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            texture = textureIn.Get<TextureData>();
            
            var inverse = inverseIn.Get<bool>();
            
            var unmaskedPoints = instanceProvider.Get<List<int>>();
            for (var i = 0; i < texture.ColorCount; ++i)
            {
                if (!inverse && texture[i].a == 0 || inverse && texture[i].a > 0)
                {
                    unmaskedPoints.Add(i);
                }
            }

            mask = instanceProvider.Get<Mask>();
            mask.Set(unmaskedPoints);

            maskOut.Set(() => mask);
        }
    }
}