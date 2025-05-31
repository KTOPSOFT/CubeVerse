using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [MovedFrom(false, "InsaneScatterbrain.MapGraph.Editor.NodeViews.MaskNodes")]
    [ScriptNodeView(typeof(InvertMaskNode))]
    public class InvertMaskNodeView : ScriptNodeView
    {
        public InvertMaskNodeView(InvertMaskNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<InvertMaskNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(InvertMaskNode node)
        {
            var width = node.Bounds.x;
            var height = node.Bounds.y;
                
            var texture = Texture2DFactory.CreateDefault(width, height);

            texture.Fill(Color.black);
            
            var colors = texture.GetRawTextureData<Color32>();
            foreach (var unmaskedPoint in node.InvertedMask.UnmaskedPoints)
            {
                colors[unmaskedPoint] = default;
            }
                
            texture.Apply();

            return texture;
        }
    }
}