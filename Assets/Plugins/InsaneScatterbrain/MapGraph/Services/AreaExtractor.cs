using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public class AreaExtractor
    {
        private int width;
        private int height;

        private readonly List<Area> areasList = new List<Area>();
        private readonly List<Area> outerAreasList = new List<Area>();
        private readonly List<Area> innerAreasList = new List<Area>();
        private readonly HashSet<int> donePixels = new HashSet<int>();

        private readonly Func<Area> newArea;
        
        public Area[] Areas { get; private set; }
        public Area[] OuterAreas { get; private set; }
        public Area[] InnerAreas { get; private set; }
        public int MinAreaSize { get; set; } = -1;
        public int MaxAreaSize { get; set; } = -1;
        
        public Color32 ColorToExtract { get; set; }
        public bool ConnectDiagonals { get; set; }
        
        [Obsolete("Will probably be removed version 2.0. Please use the other constructor.")]
        public AreaExtractor()
        {
            newArea = () => new Area();
        }

        public AreaExtractor(Func<Area> newArea)
        {
            this.newArea = newArea;
        }
        
        /// <summary>
        /// Returns whether or not a pixel is within a valid range.
        /// </summary>
        /// <param name="coords">The pixel coordinates.</param>
        /// <returns>True if the pixel is in a valid range, false otherwise.</returns>
        private bool IsPixel(Vector2Int coords)
        {
            if (coords.x < 0 || coords.y < 0) return false;
            if (coords.x >= width || coords.y >= height) return false;
            return true;
        }
        
        public void ExtractAreas(TextureData textureData)
        {
            areasList.Clear();
            outerAreasList.Clear();
            innerAreasList.Clear();
            donePixels.Clear();

            width = textureData.Width;
            height = textureData.Height;

            var checkPixels = new Stack<int>();
            var areaPoints = new List<Vector2Int>();
            
            // A flood fill is performed on each pixel to find areas of the given color.
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                if (donePixels.Contains(i) || !textureData[i].IsEqualTo(ColorToExtract)) continue;

                areaPoints.Clear();
                checkPixels.Clear();
                checkPixels.Push(i);

                var isOuterArea = false;

                while (checkPixels.Count > 0)
                {
                    // This pixel is of the area color and hasn't been added before. Add it to the area.
                    var pixelIndex = checkPixels.Pop();

                    var pixel = new Vector2Int(pixelIndex % width, pixelIndex / width);

                    var outerPixel = pixel.x == 0 || pixel.x == width - 1 || pixel.y == 0 || pixel.y == height - 1;
                    if (!isOuterArea && outerPixel)
                    {
                        isOuterArea = true;
                    }
                    
                    areaPoints.Add(pixel);

                    var west = new Vector2Int(pixel.x - 1, pixel.y);
                    var east = new Vector2Int(pixel.x + 1, pixel.y);
                    var north = new Vector2Int(pixel.x, pixel.y - 1);
                    var south = new Vector2Int(pixel.x, pixel.y + 1);

                    var westIndex = west.y * width + west.x;
                    var eastIndex = east.y * width + east.x;
                    var northIndex = north.y * width + north.x;
                    var southIndex = south.y * width + south.x;
                    
                    // Check which neighbouring pixels are part of the area and need to be added next.
                    if (IsPixel(west) && textureData[westIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(westIndex))
                    {
                        checkPixels.Push(westIndex);
                    }
                    
                    if (IsPixel(east) && textureData[eastIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(eastIndex))
                    {
                        checkPixels.Push(eastIndex);
                    }
                    
                    if (IsPixel(north) && textureData[northIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(northIndex))
                    {
                        checkPixels.Push(northIndex);
                    }
                    
                    if (IsPixel(south) && textureData[southIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(southIndex))
                    {
                        checkPixels.Push(southIndex);
                    }

                    if (ConnectDiagonals)
                    {
                        var northWest = new Vector2Int(pixel.x - 1, pixel.y - 1);
                        var northEast = new Vector2Int(pixel.x + 1, pixel.y - 1);
                        var southWest = new Vector2Int(pixel.x - 1, pixel.y + 1);
                        var southEast = new Vector2Int(pixel.x + 1, pixel.y + 1);
                        
                        var northWestIndex = northWest.y * width + northWest.x;
                        var northEastIndex = northEast.y * width + northEast.x;
                        var southWestIndex = southWest.y * width + southWest.x;
                        var southEastIndex = southEast.y * width + southEast.x;
                        
                        if (IsPixel(northWest) && textureData[northWestIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(northWestIndex))
                        {
                            checkPixels.Push(northWestIndex);
                        }
                    
                        if (IsPixel(northEast) && textureData[northEastIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(northEastIndex))
                        {
                            checkPixels.Push(northEastIndex);
                        }
                    
                        if (IsPixel(southWest) && textureData[southWestIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(southWestIndex))
                        {
                            checkPixels.Push(southWestIndex);
                        }
                    
                        if (IsPixel(southEast) && textureData[southEastIndex].IsEqualTo(ColorToExtract) && !donePixels.Contains(southEastIndex))
                        {
                            checkPixels.Push(southEastIndex);
                        }
                    }
                    
                    // Keep track of which pixels are done, so that they don't get checked more than once.
                    donePixels.Add(pixelIndex);
                }

                // If his area is smaller than the given min area size. Don't add it and move on to the next one.
                if (MinAreaSize > -1 && areaPoints.Count < MinAreaSize) continue;
                // If his area exceeds the given max area size. Don't add it and move on to the next one.
                if (MaxAreaSize > -1 && areaPoints.Count > MaxAreaSize) continue;

                var area = newArea();
                area.Set(areaPoints);
                
                areasList.Add(area);
            
                if (isOuterArea)
                {
                    outerAreasList.Add(area);
                }
                else
                {
                    innerAreasList.Add(area);
                }
            }

            Areas = areasList.ToArray();
            OuterAreas = outerAreasList.ToArray();
            InnerAreas = innerAreasList.ToArray();
        }

        public void Reset()
        {
            ColorToExtract = new Color32();
            ConnectDiagonals = false;
            MinAreaSize = -1;
            MaxAreaSize = -1;
        }
    }
}