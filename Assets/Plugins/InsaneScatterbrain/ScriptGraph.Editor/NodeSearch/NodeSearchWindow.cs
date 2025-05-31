using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor.NodeSearch
{
    /// <summary>
    /// Node search window for adding new nodes.
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private ScriptGraphView graphView;
        private Texture2D indentationImage;

        private DefaultEntryRegistry defaultEntryRegistry;
        private ConstantEntryRegistry constantEntryRegistry;
        private InputParametersEntryRegistry inputParametersEntryRegistry;
        private OutputParametersEntryRegistry outputParametersEntryRegistry;
        private GraphEntryRegistry graphEntryRegistry;
        
        private ISearchFilter searchFilter;
        private Action<IScriptNode> nodeAddedCallback;
        
        private Vector2 placementPosition;
        
        /// <summary>
        /// Initializes the node search window.
        /// </summary>
        /// <param name="view">The script graph view that it's a part of.</param>
        /// <param name="registry">The node view types registry.</param>
        public void Initialize(ScriptGraphView view, ScriptNodeViewTypesRegistry registry)
        {
            graphView = view;

            // Initialize registries that don't change without recompilation.
            defaultEntryRegistry = new DefaultEntryRegistry();
            defaultEntryRegistry.Build();
            
            constantEntryRegistry = new ConstantEntryRegistry(registry);
            constantEntryRegistry.Build();
        }

        /// <summary>
        /// Opens the node search window at the given position.
        /// </summary>
        /// <param name="menuPosition">The position to open the menu.</param>
        /// <param name="newPlacementPosition">The position a new node will be placed, if one's created.</param>
        /// <param name="applySearchFilter">The search filter to apply on the entries.</param>
        /// <param name="applyNodeAddedCallback">The callback, called when a new node has been added.</param>
        public void Open(Vector2 menuPosition, Vector2 newPlacementPosition, ISearchFilter applySearchFilter = null, Action<IScriptNode> applyNodeAddedCallback = null)
        {
            placementPosition = newPlacementPosition;
            searchFilter = applySearchFilter;
            nodeAddedCallback = applyNodeAddedCallback;
            SearchWindow.Open(new SearchWindowContext(menuPosition, 400), this);
        }
        
        private void AddRegistryEntries(SortedDictionary<string, SearchTreeEntry> sortedEntries, IEntryRegistry registry)
        {
            var pathBuilder = new StringBuilder();
            
            for (var pathIndex = 0; pathIndex < registry.Paths.Count; ++pathIndex)
            {
                var path = registry.Paths[pathIndex];

                sortedEntries.Add(path, registry.Entries[pathIndex]);

                var pathParts = path.Split('/');
                
                if (pathParts.Length < 2) continue;

                pathBuilder.Clear();
                for (var pathPartIndex = 0; pathPartIndex < pathParts.Length - 1; ++pathPartIndex)
                {
                    var pathPart = pathParts[pathPartIndex];
                    pathBuilder.Append(pathPart);

                    var parentPath = pathBuilder.ToString();
                    if (!sortedEntries.ContainsKey(parentPath))
                    {
                        sortedEntries.Add(parentPath,
                            new SearchTreeGroupEntry(new GUIContent(pathPart), pathPartIndex + 1));
                    }

                    pathBuilder.Append('/');
                }
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // We wait with creating the registries here, because the graph won't be assigned yet when the initialize
            // method is called.
            if (inputParametersEntryRegistry == null)
            {
                inputParametersEntryRegistry = new InputParametersEntryRegistry(graphView.Graph.InputParameters);
            }

            if (outputParametersEntryRegistry == null)
            {
                outputParametersEntryRegistry = new OutputParametersEntryRegistry(graphView.Graph.OutputParameters);
            }

            if (graphEntryRegistry == null)
            {
                graphEntryRegistry = new GraphEntryRegistry(graphView.Graph);
            }
            
            // Build the search menu entries that may have changed.
            inputParametersEntryRegistry.Build();
            outputParametersEntryRegistry.Build();
            graphEntryRegistry.Build();
            
            var tree = new List<SearchTreeEntry>
            {
                EntryFactory.CreateGroup("Add Node", 0)
            };
            
            var sortedEntries = new SortedDictionary<string, SearchTreeEntry>();
            
            if (constantEntryRegistry.Entries.Count > 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("• Constant"), 1));
                tree.AddRange(constantEntryRegistry.Entries);
            }

            if (inputParametersEntryRegistry.Entries.Count > 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("• Input"), 1));
                tree.AddRange(inputParametersEntryRegistry.Entries);
            }
            
            if (outputParametersEntryRegistry.Entries.Count > 0)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("• Output"), 1));
                tree.AddRange(outputParametersEntryRegistry.Entries);
            }
            
            AddRegistryEntries(sortedEntries, graphEntryRegistry);
            AddRegistryEntries(sortedEntries, defaultEntryRegistry);
            
            tree.AddRange(sortedEntries.Values);

            // If a search filter is set, apply it now, before returning the tree.
            return searchFilter == null 
                ? tree 
                : searchFilter.Apply(tree);
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            // If an option was selected from the node search window, create a new node of the selected type.
            var nodeData = (EntryData) entry.userData;
            IScriptNode newNode;

            switch (nodeData.NodeType)
            {
                case NodeType.Constant:
                    newNode = CreateConstantNode((Type) nodeData.Data);
                    break;
                case NodeType.Input:
                    newNode = CreateInputNode((string) nodeData.Data);
                    break;
                case NodeType.Output:
                    newNode = CreateOutputNode((string) nodeData.Data);
                    break;
                case NodeType.Graph:
                    newNode = CreateGraphNode((ScriptGraphGraph) nodeData.Data);
                    break;
                default:
                    newNode = CreateDefaultNode((Type) nodeData.Data);
                    break;
            }
            
            if (newNode == null) return false;

            graphView.AddNewNode(newNode, placementPosition);
            nodeAddedCallback?.Invoke(newNode);
            
            return true;
        }

        private ConstantNode CreateConstantNode(Type constantType)
        {
            var newNode = ConstantNode.Create(constantType);
            if (constantType == typeof(Color32))
            {
                newNode.Value = new Color32(0, 0, 0, 255);
            }

            return newNode;
        }

        private InputNode CreateInputNode(string parameterId)
        {
            var inputParameters = graphView.Graph.InputParameters;
            
            var parameterType = inputParameters.GetType(parameterId);
            
            return InputNode.Create(parameterId, parameterType);
        }

        private OutputNode CreateOutputNode(string parameterId)
        {
            var outputParameters = graphView.Graph.OutputParameters;
            
            var parameterType = outputParameters.GetType(parameterId);

            return OutputNode.Create(parameterId, parameterType);
        }
        
        private IScriptNode CreateDefaultNode(Type type)
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

        private IScriptNode CreateGraphNode(ScriptGraphGraph subGraph)
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