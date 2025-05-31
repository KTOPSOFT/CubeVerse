using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Square Root", "Math"), Serializable]
    public class SquareRootNode : ProcessorNode
    {
        [InPort("Value", typeof(float), true), SerializeReference]
        private InPort valueIn = null;


        [OutPort("Result", typeof(float)), SerializeReference]
        private OutPort valueOut = null;
        

        protected override void OnProcess()
        {
            var val = valueIn.Get<float>();

            var newVal = Mathf.Sqrt(val);

            valueOut.Set(() => newVal);
        }
    }
}