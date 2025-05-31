using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(FastNoiseLite.NoiseType))]
    public class NoiseTypeNodeView : EnumConstantNodeView<FastNoiseLite.NoiseType>
    {
        public NoiseTypeNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}