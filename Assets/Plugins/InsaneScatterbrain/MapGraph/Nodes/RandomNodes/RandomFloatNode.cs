using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random float value.
    /// </summary>
    [ScriptNode("Random Float", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(float)), ExplicitOutPortTypes(typeof(float))]
    public class RandomFloatNode : RangeRandomNode<float>
    {
        protected override float GetRandomValue(float minValue, float maxValue) => Get<Rng>().NextFloat(minValue, maxValue);
    }
}