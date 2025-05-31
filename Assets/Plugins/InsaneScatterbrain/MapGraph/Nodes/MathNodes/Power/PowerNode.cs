using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Power", "Math"), Serializable]
    public class PowerNode : ProcessorNode
    {
        [InPort("Value", typeof(float), true), SerializeReference]
        private InPort valueIn = null;
        
        [InPort("Power", typeof(float)), SerializeReference]
        private InPort powerIn = null;
        
        
        [OutPort("Result", typeof(float)), SerializeReference]
        private OutPort valueOut = null;
        

        protected override void OnProcess()
        {
            var val = valueIn.Get<float>();
            var power = powerIn.Get<float>();

            var newVal = Mathf.Pow(val, power);

            valueOut.Set(() => newVal);
        }
    }
}