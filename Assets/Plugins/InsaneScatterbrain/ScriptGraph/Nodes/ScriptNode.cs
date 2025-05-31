using System;
using InsaneScatterbrain.Dependencies;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Base class for script graph nodes.
    /// </summary>
    [Serializable]
    public abstract class ScriptNode : IScriptNode
    {
        [SerializeReference] private ScriptGraphGraph graph;
        
        private DependencyContainer dependencyContainer;
        
        /// <summary>
        /// Gets the graph this node is part of.
        /// </summary>
        [Obsolete("Nodes aren't directly dependant on a graph anymore. This property one will be removed in version 2.0. Check the manual page about custom nodes to see how to access dependencies.")]
        protected ScriptGraphGraph Graph => graph;

        [Obsolete("Nodes aren't directly dependant on a graph anymore. You can use the parameterless constructor instead. This constructor will be removed with version 2.0.")]
        protected ScriptNode(ScriptGraphGraph graph)
        {
            this.graph = graph;
            id = Guid.NewGuid().ToString();
        }
        
        protected ScriptNode()
        {
            id = Guid.NewGuid().ToString();
        }

        public virtual void Initialize()
        {
        }

        public virtual void LoadDependencies(DependencyContainer container)
        {
            dependencyContainer = container;
        }
        
        public event Action<IScriptNode, IDependencyContainer> OnSaveDependenciesState;
        public event Action<IScriptNode, IDependencyContainer> OnRestoreDependenciesState;

        protected void SaveDependencyStates()
        {
            OnSaveDependenciesState?.Invoke(this, dependencyContainer);
        }
        
        protected void RestoreDependencyStates()
        {
            OnRestoreDependenciesState?.Invoke(this, dependencyContainer);
        }
        
        /// <summary>
        /// Returns dependency of the given type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The object.</returns>
        protected T Get<T>() where T : class => dependencyContainer.Get<T>();
        
#if UNITY_EDITOR
        [SerializeField] private Rect rect;

        /// <inheritdoc cref="IScriptNode.Position"/>
        public Rect Position
        {
            get => rect;
            set => rect = value;
        }
        
        [SerializeField] private string note;
        
        public string Note
        {
            get => note;
            set => note = value;
        }
#endif

        [SerializeField] private string id;

        public string Id
        {
            get => id;
            set => id = value;
        }
    }
}