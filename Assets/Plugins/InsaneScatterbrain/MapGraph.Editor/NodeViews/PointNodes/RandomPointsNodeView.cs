using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomPointsNode))]
    public class RandomPointsNodeView : ScriptNodeView
    {
        public RandomPointsNodeView(RandomPointsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomPointsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomPointsNode node) 
        {
            var width = node.Bounds.x;
            var height = node.Bounds.y;

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            var textureData = texture.GetRawTextureData<Color32>();

            foreach (var point in node.Points)
            {
                var i = point.y * width + point.x;
                textureData[i] = Color.white;
            }
                
            texture.Apply();

            return texture;
        }
    }
}