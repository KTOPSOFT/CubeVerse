using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomPointsPerAreaNode))]
    public class RandomPointsPerAreaNodeView : ScriptNodeView
    {
        public RandomPointsPerAreaNodeView(IScriptNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomPointsPerAreaNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomPointsPerAreaNode node)
        {
            var width = 1;
            var height = 1;
        
            foreach (var area in node.Areas)
            {
                width = Mathf.Max(width, area.BoundingBox.xMax);
                height = Mathf.Max(height, area.BoundingBox.yMax);
            }

            var textureData = new TextureData();
            textureData.Set(width, height);
        
            textureData.Fill(Color.black);
        
            foreach (var area in node.Areas)
            {
                textureData.DrawArea(area, Color.white);
            }

            textureData.FillPoints(node.Points, Color.red);
        
            return textureData.ToTexture2D();
        }
    }

}