using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomlyStampTilemapsNode))]
    public class RandomlyStampTilemapsNodeView : ScriptNodeView
    {
        public RandomlyStampTilemapsNodeView(RandomlyStampTilemapsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomlyStampTilemapsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomlyStampTilemapsNode node) => node.TextureData.ToTexture2D();
    }
}