using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(FastNoiseLite.RotationType3D))]
    public class RotationType3dConstantNodeView : EnumConstantNodeView<FastNoiseLite.RotationType3D>
    {
        public RotationType3dConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}