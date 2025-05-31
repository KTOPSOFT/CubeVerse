using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Inverts the given masks, meaning it masks the unmasked points and vice versa.
    /// </summary>
    [ScriptNode("Invert Mask", "Masks"), Serializable]
    public class InvertMaskNode : ProcessorNode
    {
        [InPort("Mask", typeof(Mask), true), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference] 
        private InPort boundsIn = null;
        
        
        [OutPort("Inverted Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;

        private Vector2Int bounds;
        private Mask invertedMask;
        
    #if UNITY_EDITOR
        public Vector2Int Bounds => bounds;
        public Mask InvertedMask => invertedMask;
    #endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var mask = instanceProvider.Get<Mask>();
            
            maskIn.Get<Mask>().Clone(mask);
            bounds = boundsIn.Get<Vector2Int>();

            var unmaskedPoints = instanceProvider.Get<List<int>>();
            for (var x = 0; x < bounds.x; ++x)
            {
                for (var y = 0; y < bounds.y; ++y)
                {
                    var i = y * bounds.x + x;

                    if (!mask.IsPointMasked(i)) continue;
                    
                    unmaskedPoints.Add(i);
                }
            }

            invertedMask = instanceProvider.Get<Mask>();
            invertedMask.Set(unmaskedPoints);

            maskOut.Set(() => invertedMask);
        }
    }
}