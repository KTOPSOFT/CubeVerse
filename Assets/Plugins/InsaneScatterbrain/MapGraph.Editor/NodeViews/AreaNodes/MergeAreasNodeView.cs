using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(MergeAreasNode))]
    public class MergeAreasNodeView : ScriptNodeView
    {
        public MergeAreasNodeView(MergeAreasNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<MergeAreasNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(MergeAreasNode node)
        {
            var width = 1;
            var height = 1;

            foreach (var area in node.MergedAreas)
            {
                foreach (var borderPoint in area.Points)
                {
                    if (borderPoint.x >= width)
                    {
                        width = borderPoint.x + 1;
                    }

                    if (borderPoint.y >= height)
                    {
                        height = borderPoint.y + 1;
                    }
                }
            }

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);
                
            var textureData = texture.GetRawTextureData<Color32>();
                
            foreach (var area in node.MergedAreas)
            {
                var areaColor = new Color32(
                    (byte) Random.Range(0, 255), 
                    (byte) Random.Range(0, 255),
                    (byte) Random.Range(0, 255), 
                    255);
                    
                foreach (var point in area.Points)
                {
                    var index = point.y * width + point.x;
                    textureData[index] = areaColor;
                }
            }
                
            texture.Apply();

            return texture;
        }
    }
}