using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public class OutputParametersEntryRegistry : ParametersEntryRegistry
    {
        /// <inheritdoc cref="ParametersEntryRegistry.GroupTitle"/>
        protected override string GroupTitle => "Output";
        
        public OutputParametersEntryRegistry(ScriptGraphParameters parameters) : base(parameters) { }
        
        /// <inheritdoc cref="ParametersEntryRegistry.GetEntryData"/>
        protected override EntryData GetEntryData(IEnumerable<Type> portTypes, string parameterId) => 
            new EntryData(NodeType.Output, portTypes, data: parameterId);
    }
}