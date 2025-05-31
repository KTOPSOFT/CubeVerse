using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(GenerateMstAreaGraphNode))]
    public class GenerateMstAreaGraphNodeView : ScriptNodeView
    {
        public GenerateMstAreaGraphNodeView(GenerateMstAreaGraphNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<GenerateMstAreaGraphNode>(GetPreviewTexture); 
        }

        private Texture2D GetPreviewTexture(GenerateMstAreaGraphNode node)
        {
            var width = 1;
            var height = 1;
                
            foreach (var vert in node.MstAreaGraph.Vertices)
            {
                if (vert.Centroid.x > width) width = vert.Centroid.x;
                if (vert.Centroid.y > height) height = vert.Centroid.y;
            }

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            foreach (var edge in node.MstAreaGraph.Edges)
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