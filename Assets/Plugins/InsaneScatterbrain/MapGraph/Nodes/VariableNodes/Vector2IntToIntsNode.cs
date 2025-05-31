using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Splits a Vector2Int up into its components.
    /// </summary>
    [ScriptNode("Split Vector2Int", "Vector2Int"), Serializable]
    public class Vector2IntToInts : ProcessorNode
    {
        [InPort("Vector2Int", typeof(Vector2Int), true), SerializeReference] 
        private InPort vIn = null;

        
        [OutPort("x", typeof(int)), SerializeReference]
        private OutPort xOut = null;
        
        [OutPort("y", typeof(int)), SerializeReference]
        private OutPort yOut = null;


        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var v = vIn.Get<Vector2Int>();
            
            xOut.Set(() => v.x);
            yOut.Set(() => v.y);
        }
    }
}