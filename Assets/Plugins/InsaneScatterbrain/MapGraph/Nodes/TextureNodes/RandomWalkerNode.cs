using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    // TODO: better test mask and hex support
    /// <summary>
    /// Uses a random walk algorithm to draw a random path onto a texture.
    /// </summary>
    [ScriptNode("Random Walker", "Textures"), Serializable]
    public class RandomWalkerNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Fill Percentage", typeof(float), true), SerializeReference]
        private InPort fillPercentageIn = null;
        
        [InPort("Start", typeof(Vector2Int)), SerializeReference]
        private InPort startIn = null;

        [InPort("Color", typeof(Color32)), SerializeReference]
        private InPort colorIn = null;
        
        [InPort("Carve Radius", typeof(int)), SerializeReference]
        private InPort radiusIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference]
        private InPort maskIn = null;
        
        [InPort("Hex Support?", typeof(bool)), SerializeReference]
        private InPort hexSupportIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;

        private List<Vector2Int> validNextPoints;
        private List<Vector2Int> availableBorderPoints;
        private int width;
        private int height;
        private Vector2Int currentPoint;
        private Area area;
        private bool useHexSupport;
        private Mask mask;

        private TextureData textureData;
        public TextureData TextureData => textureData;

        private Rng rng;

        private bool IsValidNextPoint(Vector2Int point)
        {
            if (point.x < 0 || point.x >= width) return false;
            if (point.y < 0 || point.y >= height) return false;
            if (area.Contains(point)) return false;
            
            var index = point.y * width + point.x;
            if (mask != null && mask.IsPointMasked(index))
            {
                return false;
            }
            
            return true;
        }
        
        private Vector2Int Next()
        {
            var neighbours = currentPoint.GetRelativeNeighbours(useHexSupport);

            validNextPoints.Clear();
            
            foreach (var relativeNeighbour in neighbours)
            {
                var neighbour = currentPoint + relativeNeighbour;

                if (!IsValidNextPoint(neighbour))
                {
                    continue;
                }
                
                validNextPoints.Add(neighbour);
            }
            
            if (validNextPoints.Count < 1) return -Vector2Int.one;

            var index = rng.Next(validNextPoints.Count);

            return validNextPoints[index];
        }
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            rng = Get<Rng>();
            
            textureData = instanceProvider.Get<TextureData>(); 
            textureIn.Get<TextureData>().Clone(textureData);
            
            useHexSupport = hexSupportIn.Get<bool>();
            mask = maskIn.Get<Mask>();
            
            width = textureData.Width;
            height = textureData.Height;

            var radius = radiusIn.IsConnected ? radiusIn.Get<int>() : 1;
            
            var fillPercentage = fillPercentageIn.Get<float>();
            currentPoint = startIn.IsConnected
                ? startIn.Get<Vector2Int>()
                : new Vector2Int(width / 2, height / 2);

            var color = colorIn.Get<Color32>();

            var numPointsToFill = width * height / 100f * fillPercentage;
            if (mask != null)
            {
                numPointsToFill = mask.UnmaskedPoints.Count / 100f * fillPercentage;
            }
            
            area = instanceProvider.Get<Area>();
            area.Add(currentPoint);
            
            validNextPoints = instanceProvider.Get<List<Vector2Int>>();
            validNextPoints.EnsureCapacity(4);
            
            availableBorderPoints = instanceProvider.Get<List<Vector2Int>>();

            List<Vector2Int> circlePoints = null;
            if (radius > 1)
            {
                circlePoints = instanceProvider.Get<List<Vector2Int>>();
                TrigMath.CalculateAllPointsWithinCircle(radius, circlePoints);  // TODO: hex support
            }
            
            while (area.Points.Count < numPointsToFill)
            {
                currentPoint = Next();
                if (currentPoint != -Vector2Int.one)
                {
                    if (circlePoints == null)
                    {
                        area.Add(currentPoint);
                        continue;
                    }

                    foreach (var circlePoint in circlePoints)
                    {
                        var worldCirclePoint = currentPoint + circlePoint;
                        
                        if (!IsValidNextPoint(worldCirclePoint)) continue;
                        
                        area.Add(worldCirclePoint);
                    }
                }
                
                availableBorderPoints.Clear();
                availableBorderPoints.AddRange(area.BorderPoints);
                availableBorderPoints.Shuffle(rng);

                foreach (var borderPoint in availableBorderPoints)
                {
                    currentPoint = borderPoint;
                    currentPoint = Next();

                    if (currentPoint != -Vector2Int.one) break;
                }
                
                if (currentPoint == -Vector2Int.one)
                {
                    break;
                }
                
                area.Add(currentPoint);
            }
            
            textureData.DrawArea(area, color);
            textureOut.Set(() => textureData);
        }
    }
}