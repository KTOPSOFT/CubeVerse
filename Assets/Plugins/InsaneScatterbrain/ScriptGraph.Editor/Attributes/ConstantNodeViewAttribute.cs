using System;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This attribute is used to mark classes as node views for constant nodes. Marking a constant node view of a
    /// certain type, will allow for the creation of constant nodes with that type in the script graph editor.
    /// </summary>
    public class ConstantNodeViewAttribute : Attribute
    {
        /// <summary>
        /// The type of the constant node value.
        /// </summary>
        public Type Type { get; }

        public ConstantNodeViewAttribute(Type type)
        {
            Type = type;
        }
    }
}