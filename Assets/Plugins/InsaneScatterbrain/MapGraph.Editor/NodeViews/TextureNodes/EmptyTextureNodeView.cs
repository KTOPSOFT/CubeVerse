using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(EmptyTextureNode))]
    public class EmptyTextureNodeView : ScriptNodeView
    {
        public EmptyTextureNodeView(EmptyTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<EmptyTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(EmptyTextureNode node) => node.TextureData.ToTexture2D();
    } 
}