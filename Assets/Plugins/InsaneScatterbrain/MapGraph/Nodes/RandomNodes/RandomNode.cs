using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Base class for nodes that return a random value.
    /// </summary>
    /// <typeparam name="T">Type of random value.</typeparam>
    public abstract class RandomNode<T> : ProcessorNode
    {
        [InPort("Constant?", typeof(bool)), SerializeReference] 
        private InPort constantOutputIn = null;
        
        
        [SerializeReference] private OutPort valueOut = null;

        public override void OnLoadOutputPorts()
        {
            valueOut = AddOut<T>("Value");
        }

        protected override void OnProcess()
        {
            var constantOutput = constantOutputIn.Get<bool>();
            
            if (constantOutput)
            {
                var value = GetRandomValue();
                valueOut.Set(() => value);
            }
            else
            {
                valueOut.Set(GetRandomValue);
            }
        }

        protected abstract T GetRandomValue();
        
        [Obsolete("Nodes aren't directly dependant on a graph anymore. You can use the parameterless constructor instead. This constructor will be removed with version 2.0.")]
        protected RandomNode(ScriptGraphGraph graph) : base(graph)
        {
        }
        
        protected RandomNode()
        {
        }
    }
}