using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random Vector2Int value.
    /// </summary>
    [ScriptNode("Random Vector2Int", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(Vector2Int)), ExplicitOutPortTypes(typeof(Vector2Int))]
    public class RandomVector2IntNode : RangeRandomNode<Vector2Int>
    {
        protected override Vector2Int GetRandomValue(Vector2Int minValue, Vector2Int maxValue)
        {
            var rng = Get<Rng>();
            
            return new Vector2Int(
                rng.Next(minValue.x, maxValue.x + 1), 
                rng.Next(minValue.y, maxValue.y + 1));
        }
    }
}