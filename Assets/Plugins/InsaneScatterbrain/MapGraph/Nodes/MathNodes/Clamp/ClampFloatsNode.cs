using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Clamp (Float)", "Math"), Serializable]
    public class ClampFloatsNode : ProcessorNode
    {
        [InPort("Value", typeof(float), true), SerializeReference]
        private InPort valueIn = null;
        
        [InPort("Min", typeof(float)), SerializeReference]
        private InPort minIn = null;
        
        [InPort("Max", typeof(float)), SerializeReference]
        private InPort maxIn = null;
        
        
        [OutPort("Result", typeof(float)), SerializeReference]
        private OutPort valueOut = null;
        
            
        protected override void OnProcess()
        {
            var val = valueIn.Get<float>();
            var min = minIn.Get<float>();
            var max = maxIn.Get<float>();

            var clampedVal = Mathf.Clamp(val, min, max);

            valueOut.Set(() => clampedVal);
        }
    }
}