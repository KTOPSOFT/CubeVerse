using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Creates a Vector2Int from two floats. The floats are rounded to int.
    /// </summary>
    [ScriptNode("Vector2Int (Floats)", "Vector2Int"), Serializable]
    public class Vector2IntFromFloats : ProcessorNode
    {
        [InPort("x", typeof(float)), SerializeReference]
        private InPort xIn = null;
        
        [InPort("y", typeof(float)), SerializeReference]
        private InPort yIn = null;
        
        
        [OutPort("Output", typeof(Vector2Int)), SerializeReference]
        private OutPort vector2Out = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var a = Mathf.FloorToInt(xIn.Get<float>());
            var b = Mathf.FloorToInt(yIn.Get<float>());
            
            vector2Out.Set(() => new Vector2Int(a, b));
        }
    }
}