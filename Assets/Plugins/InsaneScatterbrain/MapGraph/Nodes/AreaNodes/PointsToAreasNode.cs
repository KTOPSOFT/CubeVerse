using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Converts each point in the points set to its own area.
    /// </summary>
    [ScriptNode("Points To Areas", "Convert"), Serializable]
    public class PointsToAreasNode : ProcessorNode
    {
        [InPort("Points", typeof(Vector2Int[]), true), SerializeReference]
        private InPort pointsIn = null;

        [InPort("Connect Diagonals?", typeof(bool)), SerializeReference]
        private InPort connectDiagonalsIn = null;

        [InPort("Single Point Areas?", typeof(bool)), SerializeReference]
        private InPort singlePointAreasIn = null;
        
        
        [OutPort("Areas", typeof(Area[])), SerializeReference]
        private OutPort areasOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var points = pointsIn.Get<Vector2Int[]>();
            var connectDiagonals = connectDiagonalsIn.Get<bool>();
            var singlePointAreas = singlePointAreasIn.Get<bool>();

            if (singlePointAreas)
            {
                var areaArray = new Area[points.Length];
                for (var i = 0; i < points.Length; ++i)
                {
                    var area = instanceProvider.Get<Area>();
                    area.Add(points[i]);
                    areaArray[i] = area;
                }

                areasOut.Set(() => areaArray);
                return;
            }

            var color = Color.black;
            
            var width = 0;
            var height = 0;
            foreach (var point in points)
            {
                if (point.x >= width)
                {
                    width = point.x + 1;
                }
                if (point.y >= height)
                {
                    height = point.y + 1;
                }
            }

            var textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);
            textureData.FillPoints(points, color);
            
            var areaExtractor = Get<AreaExtractor>();
            areaExtractor.Reset();
            areaExtractor.ColorToExtract = color;
            areaExtractor.ConnectDiagonals = connectDiagonals;

            areaExtractor.ExtractAreas(textureData);

            var areas = areaExtractor.Areas;
            
            areasOut.Set(() => areas);
        }
    }
}