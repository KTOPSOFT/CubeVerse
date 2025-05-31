using System;
using System.Collections;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Copies the tilemap data from the TilemapData object over to the Tilemap instance.
    /// </summary>
    [ScriptNode("Copy Tilemap Data", "Tilemaps", false), Serializable]
    public class CopyTilemapDataNode : ProcessorNode
    {
        [InPort("Data Source", typeof(TilemapData), true), SerializeReference] 
        private InPort dataSourceIn = null;
        
        [InPort("Copy To Target", typeof(Tilemap), true), SerializeReference] 
        private InPort copyToTargetIn = null;

        [InPort("Offset", typeof(Vector2Int)), SerializeReference]
        private InPort offsetIn = null;

        [InPort("Prevent Clear?", typeof(bool)), SerializeReference]
        private InPort preventClearIn = null;

        
        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            var source = dataSourceIn.Get<TilemapData>();
            var target = copyToTargetIn.Get<Tilemap>();
            var offset = offsetIn.Get<Vector2Int>();
            var preventClear = preventClearIn.Get<bool>();

            if (!preventClear) target.ClearAllTiles();

            foreach (var tileBlock in source.Tiles)
            {
                var position2d = tileBlock.Key;
                var tile = tileBlock.Value;
                    
                var position = new Vector3Int(position2d.x + offset.x, position2d.y + offset.y, 0);
                target.SetTile(position, tile);

                yield return null;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }
    }
}