using System;
using System.Collections;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a tilemap from a texture.
    /// </summary>
    [ScriptNode("Texture To Tilemap", "Tilemaps"), Serializable]
    public class ConvertTextureToTilemapNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
         
        [InPort("Tileset", typeof(Tileset), true), SerializeReference]
        private InPort tilesetIn = null;
        
        [InPort("Tilemap", typeof(Tilemap), true), SerializeReference]
        private InPort tilemapIn = null;

        [InPort("Offset", typeof(Vector2Int)), SerializeReference]
        private InPort offsetIn = null;

        [InPort("Prevent Clear?", typeof(bool)), SerializeReference]
        private InPort preventClearIn = null;

        private Rng random;
        private NamedColorSet namedColorSet;
        private TextureData textureData;
        private Tileset tileset;
        private int width;
        private int height;
        private Tilemap tilemap;
        private Vector2Int offset;
        private bool preventClear;

        private void ReadInPorts()
        {
            random = Get<Rng>();
            namedColorSet = Get<NamedColorSet>();
            
            textureData = textureIn.Get<TextureData>();
            tileset = tilesetIn.Get<Tileset>();
            
            width = textureData.Width;
            height = textureData.Height;
            tilemap = tilemapIn.Get<Tilemap>();

            offset = offsetIn.Get<Vector2Int>();
            preventClear = preventClearIn.Get<bool>();
        }

        private TileBase GetTile(int tileIndex)
        {
            var color = textureData[tileIndex];

            if (color.IsEqualTo(default))
            {
                if (!preventClear)
                    return null;
                
                var x = tileIndex % width;
                var y = tileIndex / width;

                var tileCoords = new Vector3Int(x, y, 0) + (Vector3Int) offset;

                if (!tilemap.HasTile(tileCoords))
                    return null;
                    
                // In case the tilemap hasn't been cleared, we need to make sure that the existing tile is kept.
                return tilemap.GetTile(tileCoords);
            }

            if (color.IsEqualTo(default))
                return null;
            
            var tileType = namedColorSet.GetName(color);
            
            if (tileType != null) 
                return tileset.GetRandomObject(tileType, random);
            
            Debug.LogError($"Unknown color: {color}");
            return null;
        }

        /// <inheritdoc cref="ProcessorNode.OnProcessMainThread"/>
        protected override void OnProcessMainThread()
        {
            ReadInPorts();
            
            if (!preventClear) tilemap.ClearAllTiles();

            var tiles = new TileBase[textureData.ColorCount];

            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                var tile = GetTile(i);
                if (tile == null)
                    continue;

                tiles[i] = tile;
            }
            
            tilemap.SetTilesBlock(new BoundsInt(offset.x,offset.y,0,width, textureData.Height, 1), tiles);

#if UNITY_EDITOR
            EditorUtility.SetDirty(tilemap);
#endif
        }
        
        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            ReadInPorts();

            var cellBounds = tilemap.cellBounds;
            var existingWidth = cellBounds.size.x;
            var existingHeight = cellBounds.size.y;
            
            var loopingWidth = Mathf.Max(width, existingWidth);
            var loopingHeight = Mathf.Max(height, existingHeight);

            for (var y = 0; y < loopingHeight; ++y)
            for (var x = 0; x < loopingWidth; ++x)
            {
                var tileCoords = new Vector3Int(x, y, 0) + (Vector3Int) offset;
                
                // If the tile is outside the bounds of the texture, clear it, except if we're preventing clearing.
                if (x >= width || y >= height)
                {
                    if (!preventClear)
                        tilemap.SetTile(tileCoords, null);

                    yield return null;
                    continue;
                }

                var i = x + y * width;

                var tile = GetTile(i);
                if (tile == null)
                {
                    if (!preventClear)
                        tilemap.SetTile(tileCoords, null);
                    
                    yield return null;
                    continue;
                }

                tilemap.SetTile(tileCoords, tile);
                
                // Reset the matrix transform to identity, so that the tile is not rotated or flipped, in the way that the previous tile might have been.
                tilemap.SetTransformMatrix(tileCoords, Matrix4x4.identity);
                
                yield return null;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(tilemap);
#endif
        }
    }
}