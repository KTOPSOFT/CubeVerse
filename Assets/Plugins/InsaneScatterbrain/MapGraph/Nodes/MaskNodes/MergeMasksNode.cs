using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Merges two masks together into a single mask.
    /// </summary>
    [ScriptNode("Merge Masks", "Masks"), Serializable]
    public class MergeMasksNode : ProcessorNode
    {
        [InPort("Mask A", typeof(Mask), true), SerializeReference] 
        private InPort maskAIn = null;
        
        [InPort("Mask B", typeof(Mask), true), SerializeReference] 
        private InPort maskBIn = null;
        
        
        [OutPort("Merged Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var maskA = maskAIn.Get<Mask>();
            var maskB = maskBIn.Get<Mask>();

            var mergedMask = instanceProvider.Get<Mask>(); 
            maskA.Merge(maskB, mergedMask);

            maskOut.Set(() => mergedMask);
        }
    }
}
