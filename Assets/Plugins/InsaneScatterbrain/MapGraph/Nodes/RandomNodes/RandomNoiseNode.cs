using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Outputs random boolean value.
    /// </summary>
    [ScriptNode("Random Noise", "Random"), Serializable]
    public class RandomNoiseNode : ProcessorNode
    {
        [InPort("Size", typeof(Vector2Int)), SerializeField]
        private InPort sizeIn = null;
        
        
        [OutPort("Random Noise", typeof(float[])), SerializeField]
        private OutPort randomNoiseOut = null;

        protected override void OnProcess()
        {
            var size = sizeIn.Get<Vector2Int>();
            
            var rng = Get<Rng>();
            
            var randomNoise = new float[size.x * size.y];
            for (var i = 0; i < randomNoise.Length; i++)
            {
                randomNoise[i] = rng.NextFloat(0f, 1f);
            }
            
            randomNoiseOut.Set(() => randomNoise);
        }
    }
}