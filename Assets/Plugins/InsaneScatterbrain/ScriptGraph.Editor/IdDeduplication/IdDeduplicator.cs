using System;
using System.Linq;

namespace InsaneScatterbrain.ScriptGraph
{
    public class IdDeduplicator 
    {
        public void DeduplicateGraph(ScriptGraphGraph graph)
        {
            // Assign a new ID to the graph.
            graph.Id = Guid.NewGuid().ToString();
            
            // All its nodes.
            foreach (var node in graph.Nodes)
            {
                node.Id = Guid.NewGuid().ToString();
            }
            
            // And all its parameters.
            foreach (var inputParameterId in graph.InputParameters.OrderedIds.ToArray())
            {
                DeduplicateInputParameter(graph, inputParameterId);
            }

            foreach (var outputParameterId in graph.OutputParameters.OrderedIds.ToArray())
            {
                DeduplicateOutputParameter(graph, outputParameterId);
            }
        }
        
        public string DeduplicateInputParameter(ScriptGraphGraph graph, string oldId)
        {
            var newId = Guid.NewGuid().ToString();
                    
            // Input parameter ID is not unique. Generate a new ID.
            graph.InputParameters.ChangeId(oldId, newId);

            // Find all the input nodes that use this particular parameter, and update their ID.
            foreach (var inputNode in graph.InputNodes)
            {
                // Doesn't match the input parameter ID of the node, so move on to the next input node.
                if (inputNode.InputParameterId != oldId)
                    continue;
                        
                // This input node uses the same input parameter ID as the node. Update the ID.
                inputNode.InputParameterId = newId;
            }

            return newId;
        }

        public string DeduplicateOutputParameter(ScriptGraphGraph graph, string oldId)
        {
            var newId = Guid.NewGuid().ToString();
                    
            // Output parameter ID is not unique. Generate a new ID.
            graph.OutputParameters.ChangeId(oldId, newId);
                    
            // Find all the output nodes that use this particular parameter, and update their ID.
            foreach (var outputNode in graph.OutputNodes)
            {
                // Doesn't match the output parameter ID of the node, so move on to the next output node.
                if (outputNode.OutputParameterId != oldId)
                    continue;
                        
                // This output node uses the same output parameter ID as the node. Update the ID.
                outputNode.OutputParameterId = newId;
            }

            return newId;
        }
    }
}