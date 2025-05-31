using System;
using InsaneScatterbrain.Dependencies;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Interface each script graph node should inherit from.
    /// </summary>
    public interface IScriptNode : INode
    {
#if UNITY_EDITOR
        string Note { get; set; }
#endif
        event Action<IScriptNode, IDependencyContainer> OnSaveDependenciesState;
        event Action<IScriptNode, IDependencyContainer> OnRestoreDependenciesState;
        void Initialize();
        void LoadDependencies(DependencyContainer container);
    }
}