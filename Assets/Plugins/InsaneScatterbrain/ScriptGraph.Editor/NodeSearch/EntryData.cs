using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    /// <summary>
    /// Data object to add as userData for the search tree entries.
    /// </summary>
    public class EntryData
    {
        private readonly List<Type> inPortTypes = new List<Type>();
        private readonly List<Type> outPortTypes = new List<Type>();
        
        private ReadOnlyCollection<Type> readOnlyInPortTypes;
        private ReadOnlyCollection<Type> readOnlyOutPortTypes;
        
        /// <summary>
        /// Gets the type of node.
        /// </summary>
        public NodeType NodeType { get; }
        
        /// <summary>
        /// Gets the entry's data.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the in port types associated with this entry's node.
        /// </summary>
        public ReadOnlyCollection<Type> InPortTypes =>
            readOnlyInPortTypes ?? (readOnlyInPortTypes = inPortTypes.AsReadOnly());

        /// <summary>
        /// Gets the out port types associated with this entry's node.
        /// </summary>
        public ReadOnlyCollection<Type> OutPortTypes =>
            readOnlyOutPortTypes ?? (readOnlyOutPortTypes = outPortTypes.AsReadOnly());

        public EntryData(NodeType type, IEnumerable<Type> inPortTypes = null, IEnumerable<Type> outPortTypes = null, object data = null)
        {
            NodeType = type;
            Data = data;
            
            if (inPortTypes != null) this.inPortTypes.AddRange(inPortTypes);
            if (outPortTypes != null) this.outPortTypes.AddRange(outPortTypes);
        }
    }
}