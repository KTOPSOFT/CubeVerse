using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ReplaceColorNode))]
    public class ReplaceColorNodeView : ScriptNodeView
    {
        public ReplaceColorNodeView(ReplaceColorNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ReplaceColorNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ReplaceColorNode node) => node.TextureData.ToTexture2D();
    }
}