using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomWalkerNode))]
    public class RandomWalkerNodeView : ScriptNodeView
    {
        public RandomWalkerNodeView(RandomWalkerNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomWalkerNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomWalkerNode node) => node.TextureData.ToTexture2D();
    }
}