using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ScaleTextureVector2Node))]
    public class ScaleTextureVector2NodeView : ScriptNodeView
    {
        public ScaleTextureVector2NodeView(ScaleTextureVector2Node node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ScaleTextureVector2Node>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ScaleTextureVector2Node node) => node.TextureData.ToTexture2D();
    }
}