using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs random boolean value.
    /// </summary>
    [ScriptNode("Random Bool", "Random"), Serializable]
    [ExplicitOutPortTypes(typeof(bool))]
    public class RandomBoolNode : RandomNode<bool>
    {
        protected override bool GetRandomValue() => Get<Rng>().Next(0, 2) == 1;
    }
}