using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.Services;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Contains data on which script node view goes with which script node.
    /// </summary>
    public class ScriptNodeViewTypesRegistry
    {
        private readonly Dictionary<Type, Type> nodeViewTypeByConstantType = new Dictionary<Type, Type>();
        
        /// <summary>
        /// Gets the node view types by their constant types.
        /// </summary>
        public IReadOnlyDictionary<Type, Type> NodeViewTypeByConstantType => nodeViewTypeByConstantType;
        
        private readonly Dictionary<Type, Type> nodeViewTypeByNodeType = new Dictionary<Type, Type>();
        
        /// <summary>
        /// Gets the node views types by their node types.
        /// </summary>
        public IReadOnlyDictionary<Type, Type> NodeViewTypeByNodeType => nodeViewTypeByNodeType;

        /// <summary>
        /// Initializes the registry.
        /// </summary>
        public void Initialize()
        {
            RegisterNodeViewTypes();
        }

        /// <summary>
        /// Search all the assemblies for classes using the ScriptNodeView and ConstantNodeView attributes and stores
        /// them with their respected types in the registry.
        /// </summary>
        private void RegisterNodeViewTypes()
        {
            var nodeViewTypes = Types.WithAttribute<ScriptNodeViewAttribute>();
            foreach (var nodeViewType in nodeViewTypes)
            {
                var nodeViewAttribute = nodeViewType.GetAttribute<ScriptNodeViewAttribute>();
                nodeViewTypeByNodeType.Add(nodeViewAttribute.Type, nodeViewType);
            }

            var constantNodeViewTypes = Types.WithAttribute<ConstantNodeViewAttribute>();
            foreach (var constantNodeViewType in constantNodeViewTypes)
            {
                var constantNodeViewAttribute = constantNodeViewType.GetAttribute<ConstantNodeViewAttribute>();
                nodeViewTypeByConstantType.Add(constantNodeViewAttribute.Type, constantNodeViewType);
            }
        }
    }
}