using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(GenerateRstAreaGraphNode))]
    public class GenerateRstAreaGraphNodeView : ScriptNodeView
    {
        public GenerateRstAreaGraphNodeView(GenerateRstAreaGraphNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<GenerateRstAreaGraphNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(GenerateRstAreaGraphNode node)
        {
            var width = 1;
            var height = 1;
                
            foreach (var vert in node.AreaGraph.Vertices)
            {
                if (vert.Centroid.x > width) width = vert.Centroid.x;
                if (vert.Centroid.y > height) height = vert.Centroid.y;
            }

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            foreach (var edge in node.AreaGraph.Edges)
            {
                var startPoint = edge.Source.Centroid;
                var endPoint = edge.Target.Centroid;

                texture.DrawLine(startPoint, endPoint, Color.white);
            }

            texture.Apply();

            return texture;
        }
    }
}