using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public interface INode
    {
        string Id { get; set; }
        
#if UNITY_EDITOR
        /// <summary>
        /// Gets/sets the position in the graph editor. Only available in the editor.
        /// </summary>
        Rect Position { get; set; }
#endif
    }
}