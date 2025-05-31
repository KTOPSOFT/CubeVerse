using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class BspNodePool : Pool<BspNode>
    {
        protected override BspNode New() => new BspNode();
        protected override void Reset(BspNode instance) => instance.Reset();
    }
}