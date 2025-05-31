using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random Vector3Int value.
    /// </summary>
    [ScriptNode("Random Vector3Int", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3Int)), ExplicitOutPortTypes(typeof(Vector3Int))]
    public class RandomVector3IntNode : RangeRandomNode<Vector3Int>
    {
        protected override Vector3Int GetRandomValue(Vector3Int minValue, Vector3Int maxValue)
        {
            var rng = Get<Rng>();
            
            return new Vector3Int(
                rng.Next(minValue.x, maxValue.x + 1), 
                rng.Next(minValue.y, maxValue.y + 1),
                rng.Next(minValue.z, maxValue.z + 1));
        }
    }
}