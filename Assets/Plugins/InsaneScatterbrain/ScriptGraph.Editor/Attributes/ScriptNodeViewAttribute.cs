using System;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This attribute is used to mark classes as node views for script nodes. Marking a node view
    /// will allow for the creation of nodes of that type in the script graph editor.
    /// </summary>
    public class ScriptNodeViewAttribute : Attribute
    {
        public Type Type { get; }

        public ScriptNodeViewAttribute(Type type)
        {
            Type = type;
        }
    }
}