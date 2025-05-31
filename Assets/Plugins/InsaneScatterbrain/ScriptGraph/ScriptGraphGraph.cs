using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.Dependencies;
using InsaneScatterbrain.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class contains all the data of a script graph. The nodes, connections, parameters, etc.
    /// </summary>
    public class ScriptGraphGraph : VersionedScriptableObject
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        /// <summary>
        /// Global event that's called every time a script graph has been enabled.
        /// </summary>
        public static event Action<ScriptGraphGraph> OnEnabled;
        
        [SerializeField] 
        private string id;

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = Guid.NewGuid().ToString();
                }

                return id;
            }
            set => id = value;
        }

        private void OnEnable() => OnEnabled?.Invoke(this);

        public event Action<IScriptNode> OnScriptNodeAdded;
        public event Action<IScriptNode> OnScriptNodeRemoved;

        [SerializeReference, HideInInspector] private List<IScriptNode> nodes = new List<IScriptNode>();
        [SerializeReference, HideInInspector] private List<IConsumerNode> consumerNodes = new List<IConsumerNode>();
        [SerializeReference, HideInInspector] private List<IProcessorNode> processorNodes = new List<IProcessorNode>();
        [SerializeReference, HideInInspector] private List<InputNode> inputNodes = new List<InputNode>();
        [SerializeReference, HideInInspector] private List<OutputNode> outputNodes = new List<OutputNode>();
        
        [Obsolete("Determining the order of execution should now be done using an execution graph. This field will be removed in version 2.0.")]
        [SerializeReference, HideInInspector] private List<IProcessorNode> sortedProcessorNodes = new List<IProcessorNode>();
        
        [SerializeField, HideInInspector] private ScriptGraphParameters inputParameters = new ScriptGraphParameters();
        [SerializeField, HideInInspector] private ScriptGraphParameters outputParameters = new ScriptGraphParameters();
        
        [SerializeField] private List<DataDependency> dataDependencies = new List<DataDependency>();
        
        private readonly DependencyContainer dataDependencyContainer = new DependencyContainer();

        public DependencyContainer DependencyContainer => dataDependencyContainer;
        
        public T Get<T>() where T : ScriptableObject
        {
            return dataDependencyContainer.Get<T>();
        }
        
#if UNITY_EDITOR
#if UNITY_2020_1_OR_NEWER
        [SerializeReference, HideInInspector] private List<Note> notes = new List<Note>();
        
        public event Action<Note> OnNoteAdded;
        public event Action<Note> OnNoteRemoved;
#endif

        [FormerlySerializedAs("duplicateConnections")] [SerializeReference, HideInInspector] private List<ReferenceNodeConnection> referenceNodeConnections = new List<ReferenceNodeConnection>();
        [FormerlySerializedAs("providerNodeDuplicates")] [SerializeReference, HideInInspector] private List<ReferenceNode> referenceNodes = new List<ReferenceNode>();
        [SerializeReference, HideInInspector] private List<GroupNode> groupNodes = new List<GroupNode>();

        public void UpdateDataDependencies()
        {
            dataDependencyContainer.Clear();
            foreach (var dataDependency in dataDependencies) 
            {
                if (dataDependency.ScriptableObject == null || dataDependency.Type == null)
                {
                    continue; 
                }
                
                dataDependencyContainer.Register(
                    dataDependency.Type, 
                    () => dataDependency.ScriptableObject
                );
            }
        }
        
        public event Action<GroupNode> OnGroupNodeAdded;
        public event Action<GroupNode> OnGroupNodeRemoved;

        public IList<ReferenceNode> ReferenceNodes => referenceNodes;
        
        [SerializeField] private bool canBeAddedAsNode;
        public bool CanBeAddedAsNode
        {
            get => canBeAddedAsNode;
            set => canBeAddedAsNode = value;
        }

        [SerializeField] private string nodePath = "Graphs";
        public string NodePath
        {
            get => nodePath;
            set => nodePath = value;
        }
        
        public IPreviewBehaviour CustomPreviewBehaviour { get; set; }
#endif

        public ScriptGraphParameters InputParameters => inputParameters;
        public ScriptGraphParameters OutputParameters => outputParameters;
        
        /// <summary>
        /// Gets/sets the processor currently processing this graph.
        /// </summary>
        [Obsolete("The graph should no longer be dependent on the processor. Will be removed in version 2.0.")]
        public ScriptGraphProcessor Processor { get; set; }

        /// <summary>
        /// Remove the input parameter with given name from the graph.
        /// </summary>
        /// <param name="parameterId">The input parameters name.</param>
        public void RemoveInputParameterNodes(string parameterId)
        {
            // Remove all input nodes that represent this parameter from the graph.
            var removeInputNodes = inputNodes.FindAll(node => node.InputParameterId == parameterId);

            foreach (var inputNode in removeInputNodes)
            {
                Remove(inputNode);
            }
        }

        /// <summary>
        /// Remove the output parameter with given name from the graph.
        /// </summary>
        /// <param name="parameterId">The output parameters name.</param>
        public void RemoveOutputParameterNodes(string parameterId)
        {
            var removeOutputNodes = outputNodes.FindAll(node => node.OutputParameterId == parameterId);

            foreach (var outputNode in removeOutputNodes)
            {
                Remove(outputNode);
            }
        }

#if UNITY_EDITOR
#if UNITY_2020_1_OR_NEWER
        public void Add(Note note)
        {
            notes.Add(note);
            OnNoteAdded?.Invoke(note);
        }
        
        public void Remove(Note note)
        {
            notes.Remove(note);
            OnNoteRemoved?.Invoke(note);
        }
        
        private ReadOnlyCollection<Note> readOnlyNotes;
        public ReadOnlyCollection<Note> Notes => readOnlyNotes ?? (readOnlyNotes = notes.AsReadOnly());
#endif
        
        /// <summary>
        /// Adds a reference node to the graph.
        /// </summary>
        /// <param name="referenceNode">The reference node to add.</param>
        public void AddReferenceNode(ReferenceNode referenceNode)
        {
            referenceNodes.Add(referenceNode);
        }

        /// <summary>
        /// Connect a reference node. Only available in the editor.
        /// </summary>
        /// <param name="referenceNode">The reference node.</param>
        /// <param name="inPort">The in port.</param>
        /// <param name="outPort">The out port.</param>
        public void ConnectReferenceNode(ReferenceNode referenceNode, InPort inPort, OutPort outPort)
        {
            var connection = new ReferenceNodeConnection(referenceNode, inPort, outPort);
            referenceNodeConnections.Add(connection);
        }

        /// <summary>
        /// Disconnect a reference node from the given in port. Only available in the editor.
        /// </summary>
        /// <param name="referenceNode">The reference node.</param>
        /// <param name="inPort">The in port to disconnect from.</param>
        public void DisconnectReferenceNode(ReferenceNode referenceNode, InPort inPort)
        {
            referenceNodeConnections.RemoveAll(connection => connection.InPort == inPort && connection.ReferenceNode == referenceNode);
        }

        /// <summary>
        /// Remove the reference node from the graph. Only available in the editor.
        /// </summary>
        /// <param name="referenceNode">The reference node.</param>
        public void RemoveReferenceNode(ReferenceNode referenceNode)
        {
            referenceNodes.Remove(referenceNode);
        }

        /// <summary>
        /// Finds the reference node connection between an in and out port and returns it. Only available in the editor.
        /// </summary>
        /// <param name="inPort">The in port.</param>
        /// <param name="outPort">The out port.</param>
        /// <returns></returns>
        public ReferenceNodeConnection GetReferenceNodeConnection(InPort inPort, OutPort outPort)
        {
            return referenceNodeConnections.FirstOrDefault(connection => connection.InPort == inPort && connection.OutPort == outPort);
        }

        /// <summary>
        /// Removes all reference nodes of the given provider node.
        /// </summary>
        /// <param name="providerNode">The provider node.</param>
        private void RemoveReferenceNodes(IProviderNode providerNode)
        {
            referenceNodes.RemoveAll(referenceNode => referenceNode.ProviderNode == providerNode);
        }
        
        public void AddGroup(GroupNode groupNode)
        {
            groupNodes.Add(groupNode);
            OnGroupNodeAdded?.Invoke(groupNode);
        }

        public void RemoveGroup(GroupNode groupNode)
        {
            groupNodes.Remove(groupNode);
            OnGroupNodeRemoved?.Invoke(groupNode);
        }

        private ReadOnlyCollection<GroupNode> readOnlyGroupNodes;
        public ReadOnlyCollection<GroupNode> GroupNodes => readOnlyGroupNodes ?? (readOnlyGroupNodes = groupNodes.AsReadOnly());
#endif
        private ReadOnlyCollection<IScriptNode> readOnlyNodes;
        private ReadOnlyCollection<IProcessorNode> readOnlyProcessorNodes;
        private ReadOnlyCollection<IConsumerNode> readOnlyConsumerNodes;
        private ReadOnlyCollection<InputNode> readOnlyInputNodes;
        private ReadOnlyCollection<OutputNode> readOnlyOutputNodes;
        private ReadOnlyCollection<IProcessorNode> readOnlySortedProcessorNodes;
        
        /// <summary>
        /// Gets all the graph's nodes.
        /// </summary>
        public ReadOnlyCollection<IScriptNode> Nodes => readOnlyNodes ?? (readOnlyNodes = nodes.AsReadOnly());

        /// <summary>
        /// Gets all the graph's processor nodes.
        /// </summary>
        public ReadOnlyCollection<IProcessorNode> ProcessorNodes =>
            readOnlyProcessorNodes ?? (readOnlyProcessorNodes = processorNodes.AsReadOnly());

        /// <summary>
        /// Gets all the graph's consumer nodes.
        /// </summary>
        public ReadOnlyCollection<IConsumerNode> ConsumerNodes =>
            readOnlyConsumerNodes ?? (readOnlyConsumerNodes = consumerNodes.AsReadOnly());

        /// <summary>
        /// Gets all the graph's input nodes.
        /// </summary>
        public ReadOnlyCollection<InputNode> InputNodes =>
            readOnlyInputNodes ?? (readOnlyInputNodes = inputNodes.AsReadOnly());

        /// <summary>
        /// Gets all the graph's output nodes.
        /// </summary>
        public ReadOnlyCollection<OutputNode> OutputNodes =>
            readOnlyOutputNodes ?? (readOnlyOutputNodes = outputNodes.AsReadOnly());

        /// <summary>
        /// Gets all the graph's processor nodes in the order they should be processed in.
        /// </summary>
        [Obsolete("Determining the order of execution should now be done using an execution graph. This property will be removed in version 2.0.")]
        public ReadOnlyCollection<IProcessorNode> SortedProcessorNodes => 
            readOnlySortedProcessorNodes ?? (readOnlySortedProcessorNodes = sortedProcessorNodes.AsReadOnly());

        [Obsolete("Determining the order of execution should now be done using an execution graph. This field will be removed in version 2.0.")]
        private readonly HashSet<IProcessorNode> visited = new HashSet<IProcessorNode>();
        
        [Obsolete("Determining the order of execution should now be done using an execution graph. This field will be removed in version 2.0.")]
        private readonly Queue<IProcessorNode> sortedNodesQueue = new Queue<IProcessorNode>();
        
        /// <summary>
        /// Sorts the graph so that processor nodes are processed in the right order, i.e. a node will never be processed
        /// before the processor nodes it receiving input from.
        /// </summary>
        [Obsolete("Determining the order of execution should now be done using an execution graph. This method will be removed in version 2.0.")]
        public void Sort()
        {
            visited.Clear();
            sortedNodesQueue.Clear();
            sortedProcessorNodes.Clear();
            
            foreach (var node in processorNodes)
            {
                if (visited.Contains(node)) continue;
                
                SortNode(node);
            }
            
            sortedProcessorNodes.AddRange(sortedNodesQueue);
        }

        public virtual void RegisterDependencies(DependencyContainer container)
        {
            // Don't need to add any dependencies for the base ScriptGraphGraph. Do nothing.
        }

        [Obsolete("Determining the order of execution should now be done using an execution graph. This method will be removed in version 2.0.")]
        private void SortNode(IProcessorNode node)
        {
            if (visited.Contains(node)) return;
            
            visited.Add(node);
            foreach (var port in node.InPorts)
            {
                if (port.ConnectedOut == null) continue;                                    // Port is not connected to anything, skip it
                
                var connectedNode = port.ConnectedOut.Owner;

                if (!(connectedNode is IProcessorNode connectedProcessorNode)) continue;    // Not a processor node, skip it
                
                SortNode(connectedProcessorNode);
            }
            sortedNodesQueue.Enqueue(node);
        }

        /// <summary>
        /// Add the given node to the graph.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void Add(IScriptNode node)
        {
            nodes.Add(node);
            if (node is IProcessorNode processableNode)
            {
                processorNodes.Add(processableNode);
            }
            else if (node is InputNode parameterIn)
            {
                inputNodes.Add(parameterIn);
            }
            else if (node is OutputNode parameterOut)
            {
                outputNodes.Add(parameterOut);
            }

            if (node is IConsumerNode consumerNode)
            {
                consumerNodes.Add(consumerNode);
            }
            
            OnScriptNodeAdded?.Invoke(node);
        }

        /// <summary>
        /// Remove the given node from the graph.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(IScriptNode node)
        {
            nodes.Remove(node);

            if (node is IConsumerNode consumerNode)
            {
                consumerNodes.Remove(consumerNode);
            }
            
            if (node is IProviderNode providerNode)
            {
#if UNITY_EDITOR
                RemoveReferenceNodes(providerNode);
#endif
                
                // Find all in ports that were connected to the removed node and disconnect them.
                var inPorts = consumerNodes.SelectMany(otherConsumerNode => otherConsumerNode.InPorts).ToList();
                var inPortsConnectedToRemovedNode = inPorts.FindAll(inPort =>
                    inPort.ConnectedOut != null && inPort.ConnectedOut.Owner == providerNode);
                inPortsConnectedToRemovedNode.ForEach(inPort => inPort.Disconnect());
            }
            
            if (node is IProcessorNode processableNode)
            {
                processorNodes.Remove(processableNode);
            }
            else if (node is InputNode parameterIn)
            {
                inputNodes.Remove(parameterIn);
            }
            else if (node is OutputNode parameterOut)
            {
                outputNodes.Remove(parameterOut);
            }
            
            OnScriptNodeRemoved?.Invoke(node);
        }

#if UNITY_EDITOR        
        [SerializeField, HideInInspector] private string serializedCustomPreviewBehaviourType;

        public void OnBeforeSerialize()
        {
            serializedCustomPreviewBehaviourType = CustomPreviewBehaviour?.GetType().AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            UpdateDataDependencies();
            
            if (string.IsNullOrEmpty(serializedCustomPreviewBehaviourType)) return; 
            
            var previewBehaviourType = Type.GetType(serializedCustomPreviewBehaviourType);

            if (previewBehaviourType == null)
            {
                Debug.LogWarning("Custom preview texture behaviour type wasn't found for graph. Setting default behaviour.", this);
                serializedCustomPreviewBehaviourType = null;
                return;
            }

            CustomPreviewBehaviour = (IPreviewBehaviour) Activator.CreateInstance(previewBehaviourType);

            if (!CustomPreviewBehaviour.IsCompatibleWith(this))
            {
                Debug.LogError($"{previewBehaviourType} is not compatible with graph.", this);
            }
        }
#endif
        
        public void Clean()
        {
            nodes.RemoveAll(node => node == null);
            
#if UNITY_EDITOR
            referenceNodes.RemoveAll(referenceNode => referenceNode == null);
            referenceNodes.RemoveAll(referenceNode => referenceNode.ProviderNode == null);
            referenceNodeConnections.RemoveAll(connection => connection == null);
            referenceNodeConnections.RemoveAll(connection => connection.ReferenceNode == null);

            foreach (var group in groupNodes)
            {
                group.Clean();
            }
#endif
            
            consumerNodes.Clear();
            processorNodes.Clear();
            inputNodes.Clear();
            outputNodes.Clear();
            
            foreach (var node in nodes)
            {
                if (node is IConsumerNode consumerNode)
                {
                    consumerNodes.Add(consumerNode);
                }
                
                if (node is IProcessorNode processorNode)
                {
                    processorNodes.Add(processorNode);
                }
                
                if (node is InputNode inputNode)
                {
                    inputNodes.Add(inputNode);
                }
                
                if (node is OutputNode outputNode)
                {
                    outputNodes.Add(outputNode);
                }
            }
        }
    }
}