using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ExtractAreasNode))]
    public class ExtractAreasNodeView : ScriptNodeView
    {
        public ExtractAreasNodeView(ExtractAreasNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ExtractAreasNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ExtractAreasNode node)
        {
            var width = node.InputTextureData.Width;
            var height = node.InputTextureData.Height;

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            var textureData = texture.GetRawTextureData<Color32>();
                
            foreach (var area in node.Areas)
            {
                foreach (var point in area.Points)
                {
                    var index = point.y * width + point.x;
                    textureData[index] = Color.white;
                }
                
                foreach (var point in area.BorderPoints)
                {
                    var index = point.y * width + point.x;
                    textureData[index] = Color.gray;
                }
            }
                
            texture.Apply();

            return texture;
        }
    }
}