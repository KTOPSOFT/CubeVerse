using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class TextureDataPool : Pool<TextureData>
    {
        protected override TextureData New() => new TextureData();

        protected override void Reset(TextureData instance) => instance.Reset();
    }
}