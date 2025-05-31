using System;
using InsaneScatterbrain.Versioning;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ScriptGraphMonoBehaviour : VersionedMonoBehaviour
    {
        protected override Version DefaultVersion => ScriptGraphEditorInfo.Version;
    }
}