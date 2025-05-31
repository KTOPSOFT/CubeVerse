using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawManhattanConnectionsNode))]
    public class DrawManhattanConnectionsNodeView : ScriptNodeView
    {
        public DrawManhattanConnectionsNodeView(DrawManhattanConnectionsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawManhattanConnectionsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawManhattanConnectionsNode node) => node.TextureData.ToTexture2D();
    }
}