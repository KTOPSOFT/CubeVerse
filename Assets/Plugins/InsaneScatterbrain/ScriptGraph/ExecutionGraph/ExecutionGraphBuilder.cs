namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Builds the execution graph for a script graph, 
    /// </summary>
    public class ExecutionGraphBuilder
    {
        public void Build(ScriptGraphGraph graph, ExecutionGraph executionGraph)
        {
            executionGraph.Clear();
            
            foreach (var node in graph.ProcessorNodes)
            {
                executionGraph.AddNode(node);
            }
            
            foreach (var node in graph.ProcessorNodes)
            foreach (var port in node.OutPorts)
            foreach (var connectedInPort in port.ConnectedInPorts)
            {
                if (!(connectedInPort.Owner is IProcessorNode connectedProcessorNode)) continue;

                executionGraph.Connect(node, connectedProcessorNode);
            }
        }
    }
}