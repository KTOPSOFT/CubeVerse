using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ExtractPointsNode))]
    public class ExtractPointsNodeView : ScriptNodeView
    {
        public ExtractPointsNodeView(ExtractPointsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ExtractPointsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ExtractPointsNode node)
        {
            var width = node.InputTextureData.Width;
            var height = node.InputTextureData.Height;

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            var textureData = texture.GetRawTextureData<Color32>();
                
            foreach (var point in node.Points)
            {
                var index = point.y * width + point.x;
                textureData[index] = Color.white;
            }
                
            texture.Apply();

            return texture;
        }
    }
}