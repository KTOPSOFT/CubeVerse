using System;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Attribute that marks a class as a node for script graph.
    /// </summary>
    public class ScriptNodeAttribute : Attribute
    {
        public string Name { get; }
        public string Category { get; }
        public bool CanBeReferenceNode { get; }

        public ScriptNodeAttribute(string name, string category, bool canBeReferenceNode = true)
        {
            Name = name;
            Category = category;
            CanBeReferenceNode = canBeReferenceNode;
        }
    }
}