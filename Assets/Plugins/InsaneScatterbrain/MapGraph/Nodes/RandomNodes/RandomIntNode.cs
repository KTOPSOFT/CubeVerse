using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random integer value.
    /// </summary>
    [ScriptNode("Random Int", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(int)), ExplicitOutPortTypes(typeof(int))]
    public class RandomIntNode : RangeRandomNode<int>
    {
        protected override int GetRandomValue(int minValue, int maxValue) => Get<Rng>().Next(minValue, maxValue + 1);
    }
}