using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Merges two tilemaps together. Tiles in tilemap B overwrite tiles in tilemap A.
    /// </summary>
    [ScriptNode("Merge Tilemaps", "Tilemaps"), Serializable]
    public class MergeTilemapsNode : ProcessorNode
    {
        [InPort("Tilemap A", typeof(TilemapData), true), SerializeReference] 
        private InPort tilemapAIn = null;
        
        [InPort("Tilemap B", typeof(TilemapData), true), SerializeReference] 
        private InPort tilemapBIn = null;
        
        
        [OutPort("Merged Tilemap", typeof(TilemapData)), SerializeReference] 
        private OutPort tilemapOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var tilemapA = tilemapAIn.Get<TilemapData>();
            var tilemapB = tilemapBIn.Get<TilemapData>();

            var mergedTilemapData = instanceProvider.Get<TilemapData>();

            foreach (var tile in tilemapA.Tiles)
            {
                mergedTilemapData.SetTile(tile.Key, tile.Value);
            }
            
            foreach (var tile in tilemapB.Tiles)
            {
                mergedTilemapData.SetTile(tile.Key, tile.Value);
            }
            tilemapOut.Set(() => mergedTilemapData);
        }
    }
}