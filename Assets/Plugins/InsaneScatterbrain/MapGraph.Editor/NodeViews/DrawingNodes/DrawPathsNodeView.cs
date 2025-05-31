using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawPathsNode))]
    public class DrawPathsNodeView : ScriptNodeView
    {
        public DrawPathsNodeView(DrawPathsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawPathsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawPathsNode node) => node.TextureData.ToTexture2D();
    }
}
