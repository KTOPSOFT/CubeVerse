using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Base class for nodes that return a random value, between a given min. and max. value.
    /// </summary>
    /// <typeparam name="T">Type of random value.</typeparam>
    public abstract class RangeRandomNode<T> : RandomNode<T>
    {
        [SerializeReference] private InPort minValueIn = null;
        [SerializeReference] private InPort maxValueIn = null;
        
        private T minValue;
        private T maxValue;

        public override void OnLoadInputPorts()
        {
            minValueIn = AddIn<T>("Min.");
            maxValueIn = AddIn<T>("Max.");
            base.OnLoadInputPorts();
        }

        protected override void OnProcess()
        {
            minValue = minValueIn.Get<T>();
            maxValue = maxValueIn.Get<T>();
            base.OnProcess();
        }

        protected override T GetRandomValue() => GetRandomValue(minValue, maxValue);

        protected abstract T GetRandomValue(T minValue, T maxValue);
        
        [Obsolete("Nodes aren't directly dependant on a graph anymore. You can use the parameterless constructor instead. This constructor will be removed with version 2.0.")]
        protected RangeRandomNode(ScriptGraphGraph graph) : base(graph)
        {
        }

        protected RangeRandomNode()
        {
        }
    }
}