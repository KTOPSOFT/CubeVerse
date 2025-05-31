using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Converts tilemap to a tilemap data object.
    /// </summary>
    [ScriptNode("Tilemap To Data", "Tilemaps"), Serializable]
    public class TilemapToDataNode : ProcessorNode
    {
        [InPort("Tilemap", typeof(Tilemap), true), SerializeReference]
        private InPort tilemapIn = null;

        [OutPort("Data", typeof(TilemapData)), SerializeReference] 
        private OutPort tilemapDataOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var tilemap = tilemapIn.Get<Tilemap>();
            var tilemapData = TilemapData.CreateFromTilemap(tilemap);
            tilemapDataOut.Set(() => tilemapData);
        }
    }
}