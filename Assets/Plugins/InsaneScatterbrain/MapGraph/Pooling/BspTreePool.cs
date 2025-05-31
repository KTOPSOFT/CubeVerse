using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class BspTreePool : Pool<BspTree>
    {
        protected override BspTree New() => new BspTree();
        protected override void Reset(BspTree instance) => instance.Reset();
    }
}