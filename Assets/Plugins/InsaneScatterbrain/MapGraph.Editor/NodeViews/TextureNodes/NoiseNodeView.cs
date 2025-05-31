using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(NoiseNode))]
    public class NoiseNodeView : ScriptNodeView
    {
        public NoiseNodeView(NoiseNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<NoiseNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(NoiseNode node)
        {
            // Convert node noise data to preview texture.
            var texture = Texture2DFactory.CreateDefault(node.Size.x, node.Size.y);
            var colors = new Color[node.NoiseData.Length];
            for (var i = 0; i < node.NoiseData.Length; i++)
            {
                colors[i] = new Color(node.NoiseData[i], node.NoiseData[i], node.NoiseData[i], 1);
            }

            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}