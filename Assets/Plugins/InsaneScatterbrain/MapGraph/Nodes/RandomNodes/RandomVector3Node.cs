using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs a random Vector3 value.
    /// </summary>
    [ScriptNode("Random Vector3", "Random"), Serializable]
    [ExplicitInPortTypes(typeof(Vector3)), ExplicitOutPortTypes(typeof(Vector3))]
    public class RandomVector3Node : RangeRandomNode<Vector3>
    {
        protected override Vector3 GetRandomValue(Vector3 minValue, Vector3 maxValue)
        {
            var rng = Get<Rng>();
            
            return new Vector3(
                rng.NextFloat(minValue.x, maxValue.x + 1), 
                rng.NextFloat(minValue.y, maxValue.y + 1),
                rng.NextFloat(minValue.z, maxValue.z + 1));
        }
    }
}