using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawPointsNode))]
    public class DrawPointsNodeView : ScriptNodeView
    {
        public DrawPointsNodeView(DrawPointsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawPointsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawPointsNode node) => node.TextureData.ToTexture2D();
    }
}