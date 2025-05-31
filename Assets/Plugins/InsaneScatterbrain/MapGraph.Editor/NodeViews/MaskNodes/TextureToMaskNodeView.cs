using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [MovedFrom(false, "InsaneScatterbrain.MapGraph.Editor.NodeViews.MaskNodes")]
    [ScriptNodeView(typeof(TextureToMaskNode))]
    public class TextureToMaskNodeView : ScriptNodeView
    {
        public TextureToMaskNodeView(TextureToMaskNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<TextureToMaskNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(TextureToMaskNode node) 
        {
            var width = node.TextureData.Width;
            var height = node.TextureData.Height;

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);

            var colors = texture.GetRawTextureData<Color32>();
            foreach (var unmaskedPoint in node.Mask.UnmaskedPoints)
            {
                colors[unmaskedPoint] = default;
            }
                
            texture.Apply();

            return texture;
        }
    }
}