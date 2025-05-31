using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Creates a Vector2Int from to integers.
    /// </summary>
    [ScriptNode("Vector2Int (Ints)", "Vector2Int"), Serializable]
    public class Vector2IntFromInts : ProcessorNode
    {
        [InPort("x", typeof(int)), SerializeReference]
        private InPort xIn = null;
        
        [InPort("y", typeof(int)), SerializeReference]
        private InPort yIn = null;
        
        
        [OutPort("Output", typeof(Vector2Int)), SerializeReference] 
        private OutPort vector2Out = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var a = xIn.Get<int>();
            var b = yIn.Get<int>();
            
            vector2Out.Set(() => new Vector2Int(a, b));
        }
    }
}