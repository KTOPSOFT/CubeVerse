using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawShortestPathNode))]
    public class DrawShortestPathConnectionsNodeView : ScriptNodeView
    {
        public DrawShortestPathConnectionsNodeView(DrawShortestPathNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawShortestPathNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawShortestPathNode node) => node.TextureData.ToTexture2D();
    }
}