using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ExtractColorNode))]
    public class ExtractColorNodeView : ScriptNodeView
    {
        public ExtractColorNodeView(ExtractColorNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ExtractColorNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ExtractColorNode node) => node.TextureData.ToTexture2D();
    }
}