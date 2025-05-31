using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class NodeFactory
    {
        private ScriptGraphView graphView;
        
        public NodeFactory(ScriptGraphView graphView)
        {
            this.graphView = graphView;
        }
        
        public ConstantNode CreateConstantNode(Type constantType)
        {
            var newNode = ConstantNode.Create(constantType);
            if (constantType == typeof(Color32))
            {
                newNode.Value = new Color32(0, 0, 0, 255);
            }

            return newNode;
        }

        public InputNode CreateInputNode(string parameterId)
        {
            var inputParameters = graphView.Graph.InputParameters;
            
            var parameterType = inputParameters.GetType(parameterId);
            
            return InputNode.Create(parameterId, parameterType);
        }

        public OutputNode CreateOutputNode(string parameterId)
        {
            var outputParameters = graphView.Graph.OutputParameters;
            
            var parameterType = outputParameters.GetType(parameterId);

            return OutputNode.Create(parameterId, parameterType);
        }
        
        public IScriptNode CreateDefaultNode(Type type)
        {
            var node = (IScriptNode) Activator.CreateInstance(type);
            if (node is IProviderNode provider)
            {
                provider.OnLoadOutputPorts();
            }
                    
            if (node is IConsumerNode consumer)
            {
                consumer.OnLoadInputPorts();
            }

            return node;
        }

        public ProcessGraphNode CreateGraphNode(ScriptGraphGraph subGraph)
        {
            var node = (ProcessGraphNode) Activator.CreateInstance(typeof(ProcessGraphNode));
            node.SubGraph = subGraph;
            node.IsNamed = true;
            node.OnLoadInputPorts();
            node.OnLoadOutputPorts();
            return node;
        }
    }
}