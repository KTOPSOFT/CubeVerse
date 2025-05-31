using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(AStarPathsNode))]
    public class AStarPathsNodeView : ScriptNodeView
    {
        public AStarPathsNodeView(AStarPathsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<AStarPathsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(AStarPathsNode node)
        {
            var xMax = 0;
            var yMax = 0;
            foreach (var connection in node.Connections)
            {
                if (connection.First.x > xMax)
                {
                    xMax = connection.First.x;
                }
                
                if (connection.Second.x > xMax)
                {
                    xMax = connection.Second.x;
                }
                
                if (connection.First.y > yMax)
                {
                    yMax = connection.First.y;
                }
                
                if (connection.Second.y > yMax)
                {
                    yMax = connection.Second.y;
                }
            }

            var texture = Texture2DFactory.CreateDefault(node.Bounds.x, node.Bounds.y);
            texture.Fill(Color.black);
            
            foreach (var path in node.Paths)
            {
                var color = Random.ColorHSV();
                foreach (var point in path)
                {
                    texture.SetPixel(point.x, point.y, color);
                }
            }
            
            texture.Apply();

            return texture;
        }
    }
}