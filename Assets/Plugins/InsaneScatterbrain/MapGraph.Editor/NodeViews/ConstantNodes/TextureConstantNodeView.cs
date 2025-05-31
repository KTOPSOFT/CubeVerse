using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(Texture2D))]
    public class TextureConstantNodeView : ConstantNodeView
    {
        public TextureConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var texture = node.Value as Texture2D;
            AddDefaultObjectField(texture);
        }
    }
}