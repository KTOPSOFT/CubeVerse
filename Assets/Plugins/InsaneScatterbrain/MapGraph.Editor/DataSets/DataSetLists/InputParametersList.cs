using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing input parameters.
    /// </summary>
    public class InputParametersList : ParametersList
    {
        /// <inheritdoc cref="ParametersList.DefaultName"/>
        protected override string DefaultName => "New Input Parameter";
        
        /// <inheritdoc cref="ParametersList.LabelText"/>
        protected override string LabelText => "Input Parameters";

        /// <summary>
        /// Gets the types available for creating input parameters.
        /// </summary>
        private static IEnumerable<Type> InputTypes { get; }

        static InputParametersList()
        {
            var inputTypeSet = new HashSet<Type>();

            foreach (var inPortAttribute in GetFieldAttributes<IConsumerNode, InPortAttribute>())
            {
                inputTypeSet.Add(inPortAttribute.Type);
            }

            foreach (var explicitInPortTypesAttribute in GetClassAttributes<IConsumerNode, ExplicitInPortTypesAttribute>())
            foreach (var type in explicitInPortTypesAttribute.Types)
            {
                inputTypeSet.Add(type);
            }

            InputTypes = SortTypes(inputTypeSet);
        }

        protected override IEnumerable<Type> Types => InputTypes;

        public InputParametersList(ScriptGraphParameters parameters, ScriptGraphGraph graph) : base(parameters, graph)
        {
            OnRemove += graph.RemoveInputParameterNodes;
        }
    }
}