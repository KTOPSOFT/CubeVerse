using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Subtracts one mask from another.
    /// </summary>
    [ScriptNode("Subtract Mask", "Masks"), Serializable]
    public class SubtractMaskNode : ProcessorNode
    {
        [InPort("Mask", typeof(Mask), true), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Subtract", typeof(Mask), true), SerializeReference] 
        private InPort subtractIn = null;
        
        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference] 
        private InPort boundsIn = null; 
        
         
        [OutPort("Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var mask = maskIn.Get<Mask>();
            var subtract = subtractIn.Get<Mask>();
            
            var bounds = boundsIn.Get<Vector2Int>();
            
            var resultMask = instanceProvider.Get<Mask>();
            resultMask.Set(bounds.x * bounds.y);
            
            for (var x = 0; x < bounds.x; x++)
            for (var y = 0; y < bounds.y; y++)
            {
                var i = y * bounds.x + x;
                
                if (!mask.IsPointMasked(i) || subtract.IsPointMasked(i))
                    continue;
                
                resultMask.MaskPoint(i);
            }

            maskOut.Set(() => resultMask);
        }
    }
}