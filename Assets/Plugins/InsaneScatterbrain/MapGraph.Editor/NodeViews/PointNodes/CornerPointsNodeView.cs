using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(CornerPointsNode))]
    public class CornerPointsNodeView : ScriptNodeView
    {
        public CornerPointsNodeView(CornerPointsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<CornerPointsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(CornerPointsNode node)
        {
            var texture = Texture2DFactory.CreateDefault(node.Width, node.Height);
            texture.Fill(Color.black);
            foreach (var point in node.Points)
            {
                texture.SetPixel(point.x, point.y, Color.black);
            }

            foreach (var point in node.CornerPoints)
            {
                texture.SetPixel(point.x, point.y, Color.white);
            }
                
            texture.Apply();

            return texture;
        }
    }
}