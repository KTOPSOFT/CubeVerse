using System;
using InsaneScatterbrain.RandomNumberGeneration;
using InsaneScatterbrain.ScriptGraph;
using Rng = InsaneScatterbrain.Services.Rng;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a randomized RNG state.
    /// </summary>
    [ScriptNode("Random RNG State", "Random"), Serializable]
    [ExplicitOutPortTypes(typeof(RngState))]
    public class RandomRngStateNode : RandomNode<RngState>
    {
        protected override RngState GetRandomValue() => Get<Rng>().State();
    }
}