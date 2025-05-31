using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains tile types by name. The names can be used to associate them with a named color.
    /// </summary>
    public class Tileset : ObjectTypeSetScriptableObject<TileType, TileTypeEntry, TileBase>, ISerializationCallbackReceiver
    {
        private Dictionary<TileBase, TileType> tilesByTileType = new Dictionary<TileBase, TileType>();

        private void OnEnable()
        {
            OnEntrySet += (typeId, entryIndex, oldTile, newTile) =>
            {
                if (oldTile == null || !tilesByTileType.ContainsKey(oldTile)) return;
                
                tilesByTileType.Remove(oldTile);
                var type = Get(typeId);

                if (newTile == null) return;
                
                tilesByTileType.Add(newTile, type);
            };
            
            OnEntryRemoved += (typeId, entryIndex, tile) =>
            {
                if (tile == null || !tilesByTileType.ContainsKey(tile)) return;
                
                tilesByTileType.Remove(tile);
            };
        }

        /// <summary>
        /// Get the type the given tile belongs to.
        /// </summary>
        /// <param name="tile">The tile to find the type for.</param>
        /// <returns>The tile type, tile is a part of.</returns>
        public string GetTypeName(TileBase tile)
        {
            return tilesByTileType[tile].Name;
        }

        /// <summary>
        /// Adds a tile type.
        /// </summary>
        /// <param name="type">The tile type to add.</param>
        public override void Add(TileType type)
        {
            foreach (var tileEntry in type.Entries)
            {
                tilesByTileType.Add(tileEntry.Value, type);
            }
            
            base.Add(type);
        }

        /// <summary>
        /// Removes a tile type by name.
        /// </summary>
        /// <param name="id">The ID of the tile type to remove.</param>
        public override void Remove(string id)
        {
            var type = Get(id);
            foreach (var tileEntry in type.Entries)
            {
                if (tileEntry.Value == null) continue;
                
                tilesByTileType.Remove(tileEntry.Value);
            }
            
            base.Remove(id);
        }
        
        #region DataSet

        [Serializable] private class OpenTileset : OpenObjectTypeSet<TileType, TileTypeEntry, TileBase> { }
        
        [SerializeField] private OpenTileset openTileset = new OpenTileset();
        
        protected override OpenObjectTypeSet<TileType, TileTypeEntry, TileBase> OpenSet => openTileset;

        #endregion

        #region Serialization

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            tilesByTileType = new Dictionary<TileBase, TileType>();

            foreach (var tileTypeId in OrderedIds)
            {
                var tileType = Get(tileTypeId);
                foreach (var tileEntry in tileType.Entries)
                {
                    if (tileEntry.Value == null) continue;

                    if (tilesByTileType.ContainsKey(tileEntry.Value)) continue; // Avoid exception by not adding duplicates.
                    
                    tilesByTileType.Add(tileEntry.Value, tileType);
                }
            }
        }
        
        [Obsolete("Tile type data is now stored in a data set. Will be removed in version 2.0.")]
        [SerializeField] private List<TileType> tileTypes = new List<TileType>();
        
        #endregion
    }
}