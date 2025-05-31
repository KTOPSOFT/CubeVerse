using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    public class InputParametersEntryRegistry : ParametersEntryRegistry
    {
        /// <inheritdoc cref="ParametersEntryRegistry.GroupTitle"/>
        protected override string GroupTitle => "Input";
        
        public InputParametersEntryRegistry(ScriptGraphParameters parameters) : base(parameters) {}
        
        /// <inheritdoc cref="ParametersEntryRegistry.GetEntryData"/>
        protected override EntryData GetEntryData(IEnumerable<Type> portTypes, string parameterId) 
            => new EntryData(NodeType.Input, outPortTypes: portTypes, data: parameterId);
    }
}