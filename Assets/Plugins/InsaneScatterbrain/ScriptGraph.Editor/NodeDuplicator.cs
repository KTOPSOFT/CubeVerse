namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class NodeDuplicator
    {
        private readonly NodeFactory factory;
        
        public NodeDuplicator(NodeFactory factory)
        {
            this.factory = factory;
        }
        
        public IScriptNode CreateCopy(IScriptNode node)
        {
            IScriptNode newNode;
            
            switch (node)
            {
                case ConstantNode constantNode:
                    var newConstantNode = factory.CreateConstantNode(constantNode.ConstType);
                    newConstantNode.Value = constantNode.Value;
                    newNode = newConstantNode;
                    break;
                case InputNode inputNode:
                    newNode = factory.CreateInputNode(inputNode.InputParameterId);
                    break;
                case OutputNode outputNode:
                    newNode = factory.CreateOutputNode(outputNode.OutputParameterId);
                    break;
                case ProcessGraphNode graphNode:
                    var newGraphNode = factory.CreateGraphNode(graphNode.SubGraph);
                    newGraphNode.IsNamed = graphNode.IsNamed;
                    newNode = newGraphNode;
                    break;
                default:
                    newNode = factory.CreateDefaultNode(node.GetType());
                    break;
            }
            newNode.Note = node.Note;

            return newNode;
        }
    }
}