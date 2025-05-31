using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains tilemap data. This is used to be able to manipulate tilemap data without requiring access to the
    /// Unity API, which is not possible on any thread that is not the main thread.
    /// </summary>
    public class TilemapData
    {
        private readonly SortedList<Vector2Int, TileBase> tiles = new SortedList<Vector2Int, TileBase>(new Vector2IntComparer());

        public HexagonalGridOffsetType HexagonalGridOffsetType { get; set; }

        /// <summary>
        /// Gets the tiles in the tilemap indexed by position.
        /// </summary>
        public IReadOnlyDictionary<Vector2Int, TileBase> Tiles => tiles;

        private RectInt totalBounds;
        
        /// <summary>
        /// Gets the tilemap's bounds.
        /// </summary>
        public RectInt Bounds => totalBounds;

        public void Reset()
        {
            tiles.Clear();
            HexagonalGridOffsetType = default;
            totalBounds = default;
        }

        /// <summary>
        /// Pastes the content of the given tilemap onto this one, starting at the given coordinates.
        /// </summary>
        /// <param name="coords">The coordinates to place the tilemap on.</param>
        /// <param name="tilemap">The tilemap to place on.</param>
        /// <param name="placements">The positions of tiles placed.</param>
        public void SetTilesBlock(Vector2Int coords, TilemapData tilemap, ref List<Vector2Int> placements)
        {
            SetTilesBlock(coords, tilemap.tiles, tilemap.HexagonalGridOffsetType, ref placements);
        }

        /// <summary>
        /// Set a block of tile map data.
        /// </summary>
        /// <param name="targetBounds">The bounds.</param>
        /// <param name="block">The block of tile to fill the bounds with.</param>
        public void SetTilesBlock(RectInt targetBounds, TileBase[] block)
        {
            var width = targetBounds.size.x;
            for (var x = targetBounds.xMin; x < targetBounds.xMax; ++x)
            {
                for (var y = targetBounds.yMin; y < targetBounds.yMax; ++y)
                {
                    var index = width * y + x;
                    var tile = block[index];
                    
                    if (tile == null) continue;

                    SetTile(new Vector2Int(x, y), tile);
                }
            }
        }

        /// <summary>
        /// Gets the bounds based on where it would be placed on a grid. The bounds will differ when it concerns
        /// a hexagonal grid for which the shape must be maintained, as some tiles might need to be shifted over.
        ///
        /// This method assumes that target coordinates refer to those on a grid with an odd row offset.
        /// </summary>
        /// <param name="targetCoords">The target coordinates.</param>
        /// <returns>The adjusted bounds.</returns>
        public RectInt GetOffsetAdjustedBounds(Vector2Int targetCoords)
        {
            var originalGridOffset = HexagonalGridOffsetType;
            var size = Bounds.size;

            var targetBounds = new RectInt(targetCoords.x, targetCoords.y, size.x, size.y);
            var adjustedBounds = targetBounds;

            if (size.y < 2) 
            {
                // It's a single row tilemap, so it has no relative offsets to maintain.
                return adjustedBounds;
            }

            var rowsToOffset = GetOffsetAdjustment(targetCoords.y, originalGridOffset);

            if (rowsToOffset == HexagonalGridOffsetType.None) return adjustedBounds;
            
            adjustedBounds.xMax += 1;

            var shiftXMin = true;
            // Check if the non-offset rows still have tiles on x = 0, if not we can move the x min with the offset.
            for (var y = adjustedBounds.yMin; y < adjustedBounds.yMax; y++)
            {
                var localY = y - targetCoords.y;
                if (rowsToOffset == HexagonalGridOffsetType.OddRows && localY.IsOdd() ||
                    rowsToOffset == HexagonalGridOffsetType.EvenRows && localY.IsEven()) continue;
                    
                var position = new Vector2Int(adjustedBounds.x, y);
                position -= adjustedBounds.min;
                    
                if (!tiles.ContainsKey(position)) continue;

                shiftXMin = false;
                break;
            }

            if (shiftXMin) adjustedBounds.xMin++;

            return adjustedBounds;
        }

        private static HexagonalGridOffsetType GetOffsetAdjustment(int yStart, HexagonalGridOffsetType originalOffsetType)
        {            
            var targetStartsOnOddRow = yStart.IsOdd();
            var targetOffset = targetStartsOnOddRow ? HexagonalGridOffsetType.EvenRows : HexagonalGridOffsetType.OddRows;

            if (originalOffsetType == HexagonalGridOffsetType.EvenRows && targetOffset == HexagonalGridOffsetType.OddRows)
            {
                return HexagonalGridOffsetType.EvenRows;
            }
            
            if (originalOffsetType == HexagonalGridOffsetType.OddRows && targetOffset == HexagonalGridOffsetType.EvenRows)
            {
                return HexagonalGridOffsetType.OddRows;
            }

            return HexagonalGridOffsetType.None;
        }

        /// <summary>
        /// Set a block of tile map data.
        /// </summary>
        /// <param name="position">The start position.</param>
        /// <param name="blockTiles">The block of tile to fill the bounds with.</param>
        /// <param name="originalOffsetType">The hexagonal grid mode (if any).</param>
        /// <param name="placements">The positions of tiles placed.</param>
        private void SetTilesBlock(Vector2Int position, IDictionary<Vector2Int, TileBase> blockTiles, HexagonalGridOffsetType originalOffsetType, ref List<Vector2Int> placements)
        {
            var targetOffset = GetOffsetAdjustment(position.y, originalOffsetType);
            placements.Clear();
            placements.EnsureCapacity(blockTiles.Count);

            foreach (var blockTilePair in blockTiles)
            {
                var tilePosition = blockTilePair.Key;
                var tile = blockTilePair.Value;

                var tileWorldPosition = position + tilePosition;
                
                var adjustedX = tileWorldPosition.x;
                if (targetOffset == HexagonalGridOffsetType.EvenRows && tilePosition.y.IsEven() || 
                    targetOffset == HexagonalGridOffsetType.OddRows && tilePosition.y.IsOdd())
                {
                    adjustedX++;
                }
                
                var coords = new Vector2Int(adjustedX, tileWorldPosition.y);
                placements.Add(coords);

                SetTile(coords, tile);
            }
        }
        
        /// <summary>
        /// Set tile at given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="tile">The tile.</param>
        public void SetTile(Vector2Int position, TileBase tile)
        {
            if (position.x < totalBounds.xMin) totalBounds.xMin = position.x;
            if (position.y < totalBounds.yMin) totalBounds.yMin = position.y;
            if (position.x + 1 > totalBounds.xMax) totalBounds.xMax = position.x + 1;
            if (position.y + 1 > totalBounds.yMax) totalBounds.yMax = position.y + 1;
            
            tiles[position] = tile;
        }

        public static TilemapData CreateFromTilemap(Tilemap tilemap)
        {
            var grid = tilemap.GetComponent<Grid>();
            var tilemapPrefab = tilemap.GetComponent<MapGraphTilemapPrefab>();
            
            var offsetType = HexagonalGridOffsetType.None;
            if (grid != null && tilemapPrefab != null && 
                tilemapPrefab.LockShape && 
                grid.cellLayout == GridLayout.CellLayout.Hexagon)
            {
                var startsOnOddRow = tilemap.cellBounds.yMin.IsOdd();
                offsetType = startsOnOddRow ? HexagonalGridOffsetType.EvenRows : HexagonalGridOffsetType.OddRows;
            }
            
            var bounds = tilemap.cellBounds;
            var tiles = tilemap.GetTilesBlock(bounds);
            
            var tilemapData = new TilemapData();

            bounds.x = 0;
            bounds.y = 0;

            var boundsRect = new RectInt(bounds.x, bounds.y, bounds.size.x, bounds.size.y);
            tilemapData.SetTilesBlock(boundsRect, tiles);
            tilemapData.HexagonalGridOffsetType = offsetType;

            return tilemapData;
        }
    }
}