using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random color.
    /// </summary>
    [ScriptNode("Random Color", "Random"), Serializable]
    [ExplicitOutPortTypes(typeof(Color32))]
    public class RandomColorNode : RandomNode<Color32>
    {
        protected override Color32 GetRandomValue() => Get<Rng>().NextColor();
    }
}