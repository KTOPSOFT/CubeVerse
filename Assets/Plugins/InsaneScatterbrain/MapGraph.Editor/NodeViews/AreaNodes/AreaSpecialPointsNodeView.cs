using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(AreasSpecialBorderPointsNode))]
    public class AreaSpecialPointsNodeView : ScriptNodeView
    {
        public AreaSpecialPointsNodeView(AreasSpecialBorderPointsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<AreasSpecialBorderPointsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(AreasSpecialBorderPointsNode node)
        {
            var width = 1;
            var height = 1;

            foreach (var area in node.Areas)
            {
                var bounds = area.BoundingBox;

                if (bounds.xMax > width) width = bounds.xMax;
                if (bounds.yMax > height) height = bounds.yMax;
            }

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);

            foreach (var area in node.Areas)
            {
                DrawPoints(area.Points, texture, Color.gray);
            }
                
            DrawPoints(node.TopLeftPoints, texture, Color.white);
            DrawPoints(node.TopPoints, texture, Color.white);
            DrawPoints(node.TopRightPoints, texture, Color.white);

            DrawPoints(node.LeftPoints, texture, Color.white);
            DrawPoints(node.RightPoints, texture, Color.white);
                
            DrawPoints(node.BottomLeftPoints, texture, Color.white);
            DrawPoints(node.BottomPoints, texture, Color.white);
            DrawPoints(node.BottomRightPoints, texture, Color.white);
                
            texture.Apply();

            return texture;
        }
        
        private void DrawPoints(IEnumerable<Vector2Int> points, Texture2D texture, Color32 color)
        {
            foreach (var point in points)
            {
                texture.SetPixel(point.x, point.y, color);
            }
        }
    }
}