using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Converts a Vector2Int to a Vector2.
    /// </summary>
    [ScriptNode("Vector2Int To Vector2", "Vector2Int"), Serializable]
    public class Vector2IntToVector2 : ProcessorNode
    {
        [InPort("Input", typeof(Vector2Int)), SerializeReference]
        private InPort vector2IntIn = null;


        [OutPort("Output", typeof(Vector2)), SerializeReference]
        private OutPort vector2Out = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var vector2 = (Vector2) vector2IntIn.Get<Vector2Int>();
            vector2Out.Set(() => vector2);
        }
    }
}