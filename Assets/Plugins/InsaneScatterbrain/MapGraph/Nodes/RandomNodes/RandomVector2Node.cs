using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random Vector2 value.
    /// </summary>
    [ScriptNode("Random Vector2", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2)), ExplicitOutPortTypes(typeof(Vector2))]
    public class RandomVector2Node : RangeRandomNode<Vector2>
    {
        protected override Vector2 GetRandomValue(Vector2 minValue, Vector2 maxValue)
        {
            var rng = Get<Rng>();
            
            return new Vector2(
                rng.NextFloat(minValue.x, maxValue.x), 
                rng.NextFloat(minValue.y, maxValue.y));
        }
    }
}