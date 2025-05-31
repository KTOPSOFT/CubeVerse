using System.Collections.Generic;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ExecutionGraph
    {
        private readonly HashSet<IProcessorNode> firstNodes = new HashSet<IProcessorNode>();
        private readonly Dictionary<IProcessorNode, HashSet<IProcessorNode>> previousNodes =
            new Dictionary<IProcessorNode, HashSet<IProcessorNode>>();
        private readonly Dictionary<IProcessorNode, HashSet<IProcessorNode>> nextNodes =
            new Dictionary<IProcessorNode, HashSet<IProcessorNode>>();

        public IReadOnlyCollection<IProcessorNode> FirstNodes => firstNodes;

        public void Clear()
        {
            firstNodes.Clear();
            previousNodes.Clear();
            nextNodes.Clear();
        } 

        public void AddNode(IProcessorNode node)
        {
            firstNodes.Add(node);
        }

        public void Connect(IProcessorNode source, IProcessorNode target)
        {
            firstNodes.Remove(target);
            
            if (!previousNodes.ContainsKey(target))
            {
                previousNodes.Add(target, new HashSet<IProcessorNode>());
            }
            previousNodes[target].Add(source);
            
            if (!nextNodes.ContainsKey(source))
            {
                nextNodes.Add(source, new HashSet<IProcessorNode>());
            }
            nextNodes[source].Add(target);
        }

        public IReadOnlyCollection<IProcessorNode> Next(IProcessorNode node) =>
            !nextNodes.ContainsKey(node) ? null : nextNodes[node];

        public IReadOnlyCollection<IProcessorNode> Previous(IProcessorNode node) =>
            !previousNodes.ContainsKey(node) ? null : previousNodes[node];
    }
}