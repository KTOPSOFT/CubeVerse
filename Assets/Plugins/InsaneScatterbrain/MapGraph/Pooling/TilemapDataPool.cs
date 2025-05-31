using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class TilemapDataPool : Pool<TilemapData>
    {
        protected override TilemapData New() => new TilemapData();

        protected override void Reset(TilemapData instance) => instance.Reset();
    }
}