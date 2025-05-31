#if UNITY_EDITOR

using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public interface IPreviewBehaviour
    {
        bool IsCompatibleWith(ScriptGraphGraph graph);
        Texture2D GetPreviewTexture(ProcessGraphNode nodeInstance);
    }
}

#endif