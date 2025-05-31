using QuikGraph;

namespace InsaneScatterbrain.ScriptGraph
{
    public struct NodeEdge : IEdge<IProcessorNode>
    {
        public NodeEdge(IProcessorNode source, IProcessorNode target)
        {
            Source = source;
            Target = target;
        }
        public IProcessorNode Source { get; }
        public IProcessorNode Target { get; }
    }  
}