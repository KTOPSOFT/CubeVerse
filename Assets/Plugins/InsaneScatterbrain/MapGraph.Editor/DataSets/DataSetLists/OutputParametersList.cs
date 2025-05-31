using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing output parameters.
    /// </summary>
    public class OutputParametersList : ParametersList
    {
        /// <inheritdoc cref="ParametersList.DefaultName"/>
        protected override string DefaultName => "New Output Parameter";
        
        /// <inheritdoc cref="ParametersList.LabelText"/>
        protected override string LabelText => "Output Parameters";

        private static IEnumerable<Type> OutputTypes { get; }

        static OutputParametersList()
        {
            var outputTypeSet = new HashSet<Type>();

            foreach (var outPortAttribute in GetFieldAttributes<IProviderNode, OutPortAttribute>())
            {
                outputTypeSet.Add(outPortAttribute.Type);
            }
            
            foreach (var explicitOutPortTypesAttribute in GetClassAttributes<IProviderNode, ExplicitOutPortTypesAttribute>())
            foreach (var type in explicitOutPortTypesAttribute.Types)
            {
                outputTypeSet.Add(type);
            }

            OutputTypes = SortTypes(outputTypeSet);
        }

        /// <inheritdoc cref="ParametersList.Types"/>
        protected override IEnumerable<Type> Types => OutputTypes;

        public OutputParametersList(ScriptGraphParameters parameters, ScriptGraphGraph graph) : base(parameters, graph)
        {
            OnRemove += graph.RemoveOutputParameterNodes;
        }
    }
}