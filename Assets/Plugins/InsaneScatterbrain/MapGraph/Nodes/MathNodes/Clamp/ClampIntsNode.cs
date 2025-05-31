using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Clamp (Int)", "Math"), Serializable]
    public class ClampIntsNode : ProcessorNode
    {
        [InPort("Value", typeof(int), true), SerializeReference]
        private InPort valueIn = null;
        
        [InPort("Min", typeof(int)), SerializeReference]
        private InPort minIn = null;
        
        [InPort("Max", typeof(int)), SerializeReference]
        private InPort maxIn = null;
        
        
        [OutPort("Result", typeof(int)), SerializeReference]
        private OutPort valueOut = null;
        
            
        protected override void OnProcess()
        {
            var val = valueIn.Get<int>();
            var min = minIn.Get<int>();
            var max = maxIn.Get<int>();

            var clampedVal = Mathf.Clamp(val, min, max);

            valueOut.Set(() => clampedVal);
        }
    }
}