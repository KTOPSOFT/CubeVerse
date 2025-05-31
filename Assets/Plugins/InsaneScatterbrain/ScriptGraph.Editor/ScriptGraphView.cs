using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsaneScatterbrain.ScriptGraph.Editor.NodeSearch;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// The ScriptGraphView allows for the visual editing of a script graph.
    /// </summary>
    public class ScriptGraphView : GraphView
    {
        public static float MinZoom { get; set; } = .1f;
        public static float MaxZoom { get; set; } = ContentZoomer.DefaultMaxScale;
        public static float ZoomStep { get; set; } = ContentZoomer.DefaultScaleStep;
        
        private Dictionary<string, GraphElement> elementsByNodeIds;
        private Dictionary<string, ScriptNodeView> viewsByNodeId;
        private Dictionary<ReferenceNode, ScriptNodeView> nodeViewsByReferenceNode;
        private Dictionary<IScriptNode, List<ScriptNodeView>> referenceNodeViewsByNode;
        private Dictionary<GroupNode, GroupNodeView> groupNodeViewsByGroupNode;
        
        private ScriptGraphGraph graph;
        private NodeSearchWindow nodeSearchWindow;
        
        /// <summary>
        /// Gets the graph that's currently being displayed/edited.
        /// </summary>
        public ScriptGraphGraph Graph => graph;

        private ScriptNodeViewTypesRegistry nodeViewTypesRegistry;
        
        /// <summary>
        /// Gets the registry that contains all the node view types for each node type.
        /// </summary>
        protected ScriptNodeViewTypesRegistry NodeViewTypesRegistry => nodeViewTypesRegistry;

        private bool showDebugInfo;
        
        /// <summary>
        /// Gets/sets whether to show debug info on the node views.
        /// </summary>
        public bool ShowDebugInfo
        {
            get => showDebugInfo;
            set
            {
                showDebugInfo = value;
                OnShowDebugInfoChanged?.Invoke(showDebugInfo);
            }
        }

        /// <summary>
        /// Event triggers when the debug info has been updated.
        /// </summary>
        public event Action<bool> OnShowDebugInfoChanged;

        private ScriptGraphContextMenu contextMenu;
        private ScriptNodeViewFactory nodeViewFactory;

        private static CopyPaste copyPaste;

        private ScriptGraphViewSelection viewSelection;
        public ScriptGraphViewSelection Selection => viewSelection;

        private bool initialized;

        /// <summary>
        /// Event triggers whenever the script graph view has been initialized.
        /// </summary>
        public event Action OnInitialized;
        
        private bool isDirty;

        private void FlagAsDirty()
        {
            isDirty = true;
        }
        
        /// <summary>
        /// Initializes the script graph view.
        /// </summary>
        public void Initialize()
        {
            if (initialized) return;
            
            // By default, the graph view is marked as dirty whenever an undo/redo is performed. 
            Undo.undoRedoPerformed += FlagAsDirty;

            // Reload the graph whenever an undo/redo is performed as it might apply to something that's changed
            // in the graph.
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                // If this graph view is visible, we can reload it immediately, whenever an undo/redo is performed.
                Undo.undoRedoPerformed += Reload;
                
                // And we don't need to mark it as dirty anymore, as it gets reloaded immediately.
                Undo.undoRedoPerformed -= FlagAsDirty;

                // If this graph view becomes visible as is flagged as dirty, reload it now, as it might have been changed
                // while it was invisible.
                if (!isDirty)
                {
                    return;
                }

                Reload();
                isDirty = false;
            });
            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                // If this graph view is no longer visible, don't reload it immediately when an undo/redo is performed.
                Undo.undoRedoPerformed -= Reload;
                
                // But mark it as dirty, so it gets reloaded when it becomes visible again.
                Undo.undoRedoPerformed += FlagAsDirty;
            });
            
            viewSelection = new ScriptGraphViewSelection(this);
            if (copyPaste == null)
            {
                copyPaste = new CopyPaste();
            }
            
            nodeViewTypesRegistry = new ScriptNodeViewTypesRegistry();
            nodeViewTypesRegistry.Initialize();

            nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            nodeSearchWindow.Initialize(this, nodeViewTypesRegistry);
            
            contextMenu = new ScriptGraphContextMenu(this, viewSelection, copyPaste, nodeSearchWindow);
            nodeViewFactory = new ScriptNodeViewFactory(nodeViewTypesRegistry);

            RegisterCallback<MouseUpEvent>(e =>
            {
                if (!e.ctrlKey) return;

                UnselectEdges();
            });

            RegisterCallback<KeyDownEvent>(e =>
            {
                if (!e.ctrlKey || e.keyCode != KeyCode.E) return;

                UnselectEdges();
            });

            OnInitialized?.Invoke();
            
            initialized = true;
        }

        private void UnselectEdges()
        {
            foreach (var edge in edges.ToList())
            {
                RemoveFromSelection(edge);
            }
        }
        
        
        public Edge Connect(InPort inPort, OutPort outPort, ReferenceNode referenceNode = null)
        {
            var consumerNodeView = viewsByNodeId[inPort.Owner.Id];
            var providerNodeView = referenceNode == null ? viewsByNodeId[outPort.Owner.Id] : nodeViewsByReferenceNode[referenceNode];
            if (referenceNode != null)
            {
                Graph.ConnectReferenceNode(referenceNode, inPort, outPort);
            }
            
            var inPortView = consumerNodeView.Inputs[inPort.Name];
            var outPortView = providerNodeView.Outputs[outPort.Name];
            
            foreach (var oldEdge in inPortView.connections)
            {
                // Multiple connections aren't allowed on the in port side, remove any existing connections.
                oldEdge.RemoveFromHierarchy();
                
                // Disconnect on the output side, to make sure the cap's no longer lit, if it's no longer connected.
                oldEdge.output.Disconnect(oldEdge);
            }
            // Disconnect the input side from anything (should only be one) it's connected to.
            inPortView.DisconnectAll();

            var edge = inPortView.ConnectTo(outPortView);
            AddElement(edge);

            return edge;
        }

        public void Dispose()
        {
            ClearElements();    // Clear all elements from the graph view, to make sure everything is disposed of.
            Undo.undoRedoPerformed -= Reload;
        }
        
        public void Reload()
        {
            Load(graph);
        }

        /// <inheritdoc cref="GraphView.BuildContextualMenu"/>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent e)
        {
            contextMenu.Open(e);
        }

        /// <summary>
        /// Adds the given node to the graph and graph view at the given position.
        /// </summary>
        /// <param name="newNode">The node to add.</param>
        /// <param name="position">The position in the graph view.</param>
        /// <param name="autoAddToGroup">If set to true, nodes will be automatically added to any group node they are on top of.</param>
        public void AddNewNode(IScriptNode newNode, Vector2 position, bool autoAddToGroup = true)
        {
            Undo.RegisterCompleteObjectUndo(graph, "Add Node");
            
            newNode.Position = new Rect(position, Vector2.zero);
            graph.Add(newNode);

            if (autoAddToGroup)
            {
                for (var i = graph.GroupNodes.Count - 1; i >= 0; --i)
                {
                    var groupNode = graph.GroupNodes[i];
                    var groupNodeView = groupNodeViewsByGroupNode[groupNode];
                    if (!groupNodeView.localBound.Contains(position)) continue;

                    groupNodeView.GroupNode.Add(newNode);

                    var newNodeView = GetView(newNode);
                    groupNodeView.AddElement(newNodeView);
                
                    break;
                }
            }
            
            EditorUtility.SetDirty(graph);
        }

        public void AddNewGroupNode(GroupNode newGroupNode, Vector2 position)
        {
            Undo.RegisterCompleteObjectUndo(graph, "Add Group");

            newGroupNode.Position = new Rect(position, Vector2.zero);
            graph.AddGroup(newGroupNode);
            
            EditorUtility.SetDirty(graph);
        }

        /// <summary>
        /// Loads the given script graph into the graph view.
        /// </summary>
        /// <param name="graphToLoad">The graph to load.</param>
        public void Load(ScriptGraphGraph graphToLoad)
        {
            // Setup the zoom in case the zoom values have changed.
            SetupZoom();
            
            graph = graphToLoad;
            BuildGraphView();
        }

        private void RegisterEvents()
        {
            graph.OnScriptNodeAdded -= ScriptNodeAdded;
            graph.OnScriptNodeAdded += ScriptNodeAdded;
            
            graph.OnScriptNodeRemoved -= ScriptNodeRemoved;
            graph.OnScriptNodeRemoved += ScriptNodeRemoved;

            graph.OnGroupNodeAdded -= GroupNodeAdded;
            graph.OnGroupNodeAdded += GroupNodeAdded;
            
            graph.OnGroupNodeRemoved -= GroupNodeRemoved;
            graph.OnGroupNodeRemoved += GroupNodeRemoved;

#if UNITY_2020_1_OR_NEWER
            graph.OnNoteAdded -= NoteAdded;
            graph.OnNoteAdded += NoteAdded;
            
            graph.OnNoteRemoved -= NoteRemoved;
            graph.OnNoteRemoved += NoteRemoved;
#endif
        }

        private void UnregisterEvents()
        {
            graph.OnScriptNodeAdded -= ScriptNodeAdded;
            graph.OnScriptNodeRemoved -= ScriptNodeRemoved;
            
            graph.OnGroupNodeAdded -= GroupNodeAdded;
            graph.OnGroupNodeRemoved -= GroupNodeRemoved;
            
#if UNITY_2020_1_OR_NEWER
            graph.OnNoteAdded -= NoteAdded;
            graph.OnNoteRemoved -= NoteRemoved;
#endif
        }

        public void TriggerKeyDown(KeyCode keyCode, EventModifiers modifiers, Vector2 originalMousePosition)
        {
            switch (keyCode)
            {
                case KeyCode.Space:
                    var screenPos = GUIUtility.GUIToScreenPoint(originalMousePosition);
                    var placementPos = this.ChangeCoordinatesTo(contentViewContainer, originalMousePosition);
                    nodeSearchWindow.Open(screenPos, placementPos);
                    break;
                case KeyCode.Delete:
                case KeyCode.Backspace:
                    DeleteSelection();
                    break;
                case KeyCode.D when modifiers == EventModifiers.Control:
                    viewSelection.Update();

                    if (!viewSelection.IsNodeSelected) break;
                        
                    copyPaste.DuplicateSelection(this);
                    break;
                case KeyCode.C when modifiers == EventModifiers.Control:
                    viewSelection.Update();

                    if (!viewSelection.IsNodeSelected) break;
                        
                    copyPaste.CopySelection(this);
                    break;
                case KeyCode.X when modifiers == EventModifiers.Control:
                    viewSelection.Update();

                    if (!viewSelection.IsNodeSelected) break;
                        
                    copyPaste.CutSelection(this);
                    break;
                case KeyCode.V when modifiers == EventModifiers.Control:
                    if (!copyPaste.HasCopyData) break;
                        
                    copyPaste.PasteSelection(this);
                    break;
            }
        }

        private void SetupZoom()
        {
            SetupZoom(MinZoom, MaxZoom, ZoomStep, ContentZoomer.DefaultReferenceScale);
        }

        /// <summary>
        /// Creates a new graph view. 
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="showDebugInfo">Whether or not to display the debug info on node views.</param>
        public ScriptGraphView(string title, bool showDebugInfo)
        {
            name = title;
            this.showDebugInfo = showDebugInfo;
            
            styleSheets.Add(Resources.Load<StyleSheet>("ScriptGraphView"));

            // Find any custom styling USS files, and load them.
            var customStyling = Resources.LoadAll<StyleSheet>("MapGraph");
            foreach (var styleSheet in customStyling)
            {
                styleSheets.Add(styleSheet);
            }
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(10, 10, 200, 140));
            Add(miniMap);

            SetupZoom();

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            RegisterCallback<KeyDownEvent>(e => TriggerKeyDown(e.keyCode, e.modifiers, e.originalMousePosition));

            // Whenever the graph view changes, the changes made need to be applied to the script graph object itself
            // as well.
            graphViewChanged += change => 
            {
                if (change.movedElements != null)
                {
                    // An object has been moved.
                    Undo.RegisterCompleteObjectUndo(graph, "Move Node");
                    
                    foreach (var movedElement in change.movedElements)
                    {
                        switch (movedElement)
                        {
                            case GroupNodeView movedGroupNodeView:
                            {
                                var movedGroupNode = movedGroupNodeView.GroupNode;
                                movedGroupNode.Position = movedElement.GetPosition();
                                
                                // Update the position of the nodes inside the group as well.
                                foreach (var node in movedGroupNode.Nodes)
                                {
                                    node.Position = GetView(node).GetPosition();
                                }

                                foreach (var referenceNode in movedGroupNode.ReferenceNodes)
                                {
                                    referenceNode.Position = GetReferenceNodeView(referenceNode).GetPosition();
                                }
                                
                                break;
                            }
                            case ScriptNodeView movedNodeView when movedNodeView.userData != null:
                            {
                                // It's a reference node that has been moved. Store the new position on the reference node, not the original node.
                                var referenceNode = (ReferenceNode) movedNodeView.userData;
                                referenceNode.Position = movedElement.GetPosition();
                                break;
                            }
                            case ScriptNodeView movedNodeView:
                            {
                                // Store the new position on the node.
                                var movedNode = movedNodeView.Node;
                                movedNode.Position = movedElement.GetPosition();
                                break;
                            }
#if UNITY_2020_1_OR_NEWER
                            case NoteView movedNodeView:
                            {
                                var movedNote = movedNodeView.Note;
                                movedNote.Position = movedElement.GetPosition();
                                break;
                            }
#endif
                        }
                    } 
                }
                
                if (change.edgesToCreate != null)
                {
                    // An edge has been created, meaning that a new connection has been made between ports.
                    
                    Undo.RegisterCompleteObjectUndo(graph, "Connect Node");
                    foreach (var newEdge in change.edgesToCreate)
                    {
                        var consumerNodeView = (ScriptNodeView) newEdge.input.node;
                        var inputPortName = newEdge.input.portName; 

                        var providerNodeView = (ScriptNodeView) newEdge.output.node;
                        var outputPortName = newEdge.output.portName;
                    
                        var consumerNode = (IConsumerNode) consumerNodeView.Node;
                        var providerNode = (IProviderNode) providerNodeView.Node;
                        
                        var inputPort = consumerNode.GetInPort(inputPortName);
                        var outputPort = providerNode.GetOutPort(outputPortName);

                        if (providerNodeView.userData != null)
                        {
                            // The connection is made with a reference node, store on which reference node this connection is made.
                            var referenceNode = (ReferenceNode) providerNodeView.userData;

                            graph.ConnectReferenceNode(referenceNode, inputPort, outputPort);
                        }

                        inputPort.Connect(outputPort);
                    }
                }

                if (change.elementsToRemove != null)
                {
                    // Something has been removed from the graph view.
                    
                    Undo.RegisterCompleteObjectUndo(graph, "Delete Nodes/Connections");
                    foreach (var element in change.elementsToRemove)
                    {
                        switch (element)
                        {
                            case Edge edge:
                            {
                                // If an edge has been removed, it means two ports have been disconnected from each other.
                                
                                var consumerNodeView = (ScriptNodeView) edge.input.node;
                                var inputPortName = edge.input.portName;
                            
                                var consumerNode = (IConsumerNode) consumerNodeView.Node;
                                var inputPort = consumerNode.GetInPort(inputPortName);
                                
                                inputPort.Disconnect();
                            
                                var providerNodeView = (ScriptNodeView) edge.output.node;

                                if (providerNodeView.userData == null) continue;

                                // If the connection was made with a reference node, make sure to remove it there as well.
                                var referenceNode = (ReferenceNode) providerNodeView.userData;
                                graph.DisconnectReferenceNode(referenceNode, inputPort);
                                break;
                            }
                            case ScriptNodeView nodeView:
                            {
                                // A node view has been removed. Remove the corresponding node from the graph.
                                
                                var node = nodeView.Node;

                                if (node is IProviderNode && nodeView.userData is ReferenceNode referenceNode)
                                {
                                    // The removed node is a reference node.
                                    graph.RemoveReferenceNode(referenceNode);
                                    RemoveReferenceNodeView(referenceNode);
                                }
                                else
                                {
                                    graph.Remove(node);
                                    RemoveNodeView(node);
                                }

                                break;
                            }
                            case GroupNodeView groupNodeView:
                            {
                                var groupNode = groupNodeView.GroupNode;
                                graph.RemoveGroup(groupNode);
                                RemoveGroupNodeView(groupNode);
                                break;
                            }
#if UNITY_2020_1_OR_NEWER
                            case NoteView nodeView:
                            {
                                var note = nodeView.Note;
                                graph.Remove(note);
                                break;
                            }
#endif
                        }
                    }
                }
                
                EditorUtility.SetDirty(graph);
                return change; 
            };
            
            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                if (graph == null) return;

                UnregisterEvents();
            });
            
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                if (graph == null) return;

                RegisterEvents();
            });
        }

        private async void ScriptNodeAdded(IScriptNode node)
        {
            var nodeView = BuildScriptNodeView(node);

            // If there's an text field on this node view, give it focus.
            var textField = nodeView.Q(className: "const-input-field")?.Q(TextInputBaseField<string>.textInputUssName);
            textField?.RegisterCallback<AttachToPanelEvent>(_ => textField.Focus());
            await Task.Delay(1);    // Wait a bit until the style change has been processed, before focussing on the element.
            textField?.Focus();
        }

        private void GroupNodeAdded(GroupNode groupNode) => BuildGroupNodeView(groupNode);
        
        private void BuildGroupNodeView(GroupNode groupNode)
        {
            var groupView = new GroupNodeView(groupNode, this)
            {
                title = groupNode.Title
            };
            groupView.AddManipulator(new ContextualMenuManipulator(evt => { }));
            groupView.SetPosition(groupNode.Position);
            
            AddElement(groupView);

            foreach (var node in groupNode.Nodes)
            {
                if (!viewsByNodeId.ContainsKey(node.Id)) continue;

                var nodeView = viewsByNodeId[node.Id];
                groupView.AddElement(nodeView);
            }

            foreach (var referenceNode in groupNode.ReferenceNodes)
            {
                if (!nodeViewsByReferenceNode.ContainsKey(referenceNode)) continue;
                
                var referenceNodeView = nodeViewsByReferenceNode[referenceNode];
                groupView.AddElement(referenceNodeView);
            }

            groupNodeViewsByGroupNode.Add(groupNode, groupView);
            elementsByNodeIds.Add(groupNode.Id, groupView);
        }

        private void ScriptNodeRemoved(IScriptNode node) => RemoveNodeView(node);

        private void GroupNodeRemoved(GroupNode groupNode) => RemoveGroupNodeView(groupNode);

        /// <summary>
        /// Creates all the visual elements required to show the connections with the given node in the graph view.
        /// </summary>
        /// <param name="node">The consumer node.</param>
        private void ConnectOutputNode(IConsumerNode node)
        {
            var nodeView = viewsByNodeId[node.Id];
            
            foreach (var inPort in node.InPorts)
            {
                if (inPort.ConnectedOut == null) continue;    // No connection, so nothing to draw for this port.
                
                var connectedPort = inPort.ConnectedOut;

                var connectedNode = (IProviderNode) connectedPort.Owner;

                var connectedNodeView = viewsByNodeId[connectedNode.Id];    // Find the node view for the connected node.

                var referenceNodeConnection = graph.GetReferenceNodeConnection(inPort, connectedPort);
                if (referenceNodeConnection != null)
                {
                    // This is a reference node connection so swap out the node view with that of the correct reference
                    // node instead of the original node.
                    var referenceNode = referenceNodeConnection.ReferenceNode;

                    connectedNodeView = nodeViewsByReferenceNode[referenceNode];
                }
                
                // Get the connected ports and create an edge between them.
                var outputPortView = connectedNodeView.Outputs[connectedPort.Name];
                var inputPortView = nodeView.Inputs[inPort.Name];
                var edge = inputPortView.ConnectTo(outputPortView);
                AddElement(edge);
            }
        }

        public bool IsClearing { get; private set; }
        
        /// <summary>
        /// Removes all visual elements from the graph view.
        /// </summary>
        private void ClearElements()
        {
            IsClearing = true;
            
            foreach (var node in nodes.ToList())
            {
                if (node is ScriptNodeView nodeView)
                {
                    RemoveNodeViewElement(nodeView);
                }
                else
                {
                    RemoveElement(node);
                }
            }

            foreach (var edge in edges.ToList())
            {
                RemoveElement(edge); 
            }

            var groupNodeViews = this.Query<GroupNodeView>().ToList();
            foreach (var groupNodeView in groupNodeViews)
            {
                RemoveElement(groupNodeView);
            }
            
#if UNITY_2020_1_OR_NEWER
            var noteViews = this.Query<NoteView>().ToList();
            foreach (var noteView in noteViews)
            {
                RemoveElement(noteView);
            }
#endif

            IsClearing = false;
        }

        /// <summary>
        /// Builds a node view element for the given node and adds it this graph view.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The node view.</returns>
        private ScriptNodeView BuildScriptNodeView(IScriptNode node)
        {
            var nodeView = nodeViewFactory.CreateNodeViewForNode(node, this);

            InitializeScriptNodeView(nodeView);
            
            viewsByNodeId.Add(node.Id, nodeView);
            elementsByNodeIds.Add(node.Id, nodeView);
            
            return nodeView;
        }

        private void InitializeScriptNodeView(ScriptNodeView nodeView)
        {
            var node = nodeView.Node;
            
            AddElement(nodeView); 
                            
            // If this node has reference nodes, highlight them whenever the mouse is hovering over the node view.
            nodeView.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (!referenceNodeViewsByNode.ContainsKey(node)) return;
                    
                var referenceNodes = referenceNodeViewsByNode[node];
                foreach (var referenceNode in referenceNodes)
                {
                    referenceNode.AddToClassList("highlight");
                }
            });
            nodeView.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!referenceNodeViewsByNode.ContainsKey(node)) return;
                    
                var referenceNodes = referenceNodeViewsByNode[node];
                foreach (var referenceNode in referenceNodes)
                {
                    referenceNode.RemoveFromClassList("highlight");
                }
            });
        }

        public void InitializePort(Port port)
        {
            if (port.node.userData != null) return;
            
            port.AddManipulator(new EdgeConnector<Edge>(new OutPortEdgeConnectListener(this, nodeSearchWindow, (ScriptGraphPort) port.userData)));
        }
        
        private void RemoveNodeViewElement(ScriptNodeView nodeView)
        {
            nodeView.Dispose();
            RemoveElement(nodeView);
        }
        
        private void RemoveReferenceNodeView(ReferenceNode referenceNode)
        {
            if (!nodeViewsByReferenceNode.ContainsKey(referenceNode)) return;
            
            var referenceNodeView = nodeViewsByReferenceNode[referenceNode];
            referenceNodeViewsByNode[referenceNode.ProviderNode].Remove(referenceNodeView);
            nodeViewsByReferenceNode.Remove(referenceNode);
            elementsByNodeIds.Remove(referenceNode.Id);

            RemoveNodeViewElement(referenceNodeView);
        }
        
        /// <summary>
        /// Removes the node view element for the given node and its reference nodes from this graph view.
        /// </summary>
        /// <param name="node">The node.</param>
        private void RemoveNodeView(IScriptNode node)
        {
            if (viewsByNodeId.ContainsKey(node.Id))
            {
                var nodeView = viewsByNodeId[node.Id];
                RemoveNodeViewEdges(nodeView);
                RemoveNodeViewElement(nodeView);

                viewsByNodeId.Remove(node.Id);
                elementsByNodeIds.Remove(node.Id);
            }
            
            if (!referenceNodeViewsByNode.ContainsKey(node)) return;

            var referenceNodeViews = referenceNodeViewsByNode[node];
            foreach (var referenceNodeView in referenceNodeViews)
            {
                // Remove all the edges connected to the reference node.
                RemoveNodeViewEdges(referenceNodeView);
                RemoveNodeViewElement(referenceNodeView);
            }
        }

        private void RemoveGroupNodeView(GroupNode groupNode)
        {
            if (!groupNodeViewsByGroupNode.ContainsKey(groupNode)) return;
            
            var view = groupNodeViewsByGroupNode[groupNode];
            groupNodeViewsByGroupNode.Remove(groupNode);
            elementsByNodeIds.Remove(groupNode.Id);
            
            RemoveElement(view);
        }

        private void RemoveNodeViewEdges(ScriptNodeView nodeView)
        {
            foreach (var input in nodeView.Inputs)
            {
                var inPort = input.Value;
                var removeEdges = inPort.connections.ToArray();
                foreach (var edge in removeEdges)
                {
                    edge.input?.Disconnect(edge);
                    edge.output?.Disconnect(edge);
                    RemoveElement(edge);
                }
            }
            
            foreach (var output in nodeView.Outputs)
            {
                var outPort = output.Value;
                var removeEdges = outPort.connections.ToArray();
                foreach (var edge in removeEdges)
                {
                    edge.input?.Disconnect(edge);
                    edge.output?.Disconnect(edge);
                    RemoveElement(edge);
                }
            }
        }
        
        /// <summary>
        /// Creates node views for the given nodes.
        /// </summary>
        /// <param name="buildNodes">The nodes to build views for.</param>
        private void BuildScriptNodes(IEnumerable<IScriptNode> buildNodes)
        {
            foreach (var node in buildNodes)
            {
                BuildScriptNodeView(node);
            }
        }

        private void BuildGroupNodes(IEnumerable<GroupNode> groupNodes)
        {
            foreach (var groupNode in groupNodes)
            {
                BuildGroupNodeView(groupNode);
            }
        }

        private void BuildReferenceNodeView(ReferenceNode referenceNode)
        {
            var providerNode = referenceNode.ProviderNode;
            var referenceNodeView = nodeViewFactory.CreateNodeViewForReferenceNode(referenceNode, this);

            // Highlight the original node when hovering over the reference node.
            referenceNodeView.RegisterCallback<MouseEnterEvent>(evt =>
            {
                var originalNodeView = viewsByNodeId[providerNode.Id];
                originalNodeView.AddToClassList("highlight");
            });
            referenceNodeView.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                var originalNodeView = viewsByNodeId[providerNode.Id];
                originalNodeView.RemoveFromClassList("highlight"); 
            });
            referenceNodeView.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (!viewsByNodeId.ContainsKey(providerNode.Id)) return;
                
                var originalNodeView = viewsByNodeId[providerNode.Id];
                originalNodeView.RemoveFromClassList("highlight"); 
            });
            referenceNodeView.IsNoteReadOnly = true;
            
            foreach (var output in referenceNodeView.Outputs.Values)
            {
                output.AddManipulator(
                    new EdgeConnector<Edge>(
                        new OutPortEdgeConnectListener(
                            this, 
                            nodeSearchWindow, 
                            (ScriptGraphPort) output.userData,
                            referenceNode)));
            }
                
            // Reference nodes only hove output ports, so remove anything that might be on the input side of the node view.
            referenceNodeView.inputContainer.Clear();

            nodeViewsByReferenceNode.Add(referenceNode, referenceNodeView);

            // Store reference nodes of a node so they can be found easily for each original node.
            if (!referenceNodeViewsByNode.ContainsKey(providerNode))
            {
                referenceNodeViewsByNode.Add(providerNode, new List<ScriptNodeView>());
            }
            referenceNodeViewsByNode[providerNode].Add(referenceNodeView);
            elementsByNodeIds.Add(referenceNode.Id, referenceNodeView);
            
            AddElement(referenceNodeView);
        }

        public ScriptNodeView GetView(IScriptNode node) => viewsByNodeId[node.Id];
        public ScriptNodeView GetReferenceNodeView(ReferenceNode referenceNode) => nodeViewsByReferenceNode[referenceNode];
        public GroupNodeView GetGroupView(GroupNode groupNode) => groupNodeViewsByGroupNode[groupNode];
        
#if UNITY_2020_1_OR_NEWER
        public NoteView GetNoteView(Note note) => viewsByNote[note];
#endif

        /// <summary>
        /// Creates node views for reference nodes.
        /// </summary>
        /// <param name="referenceNodes">The reference nodes.</param>
        private void BuildNodeReferenceNodes(IEnumerable<ReferenceNode> referenceNodes)
        {
            nodeViewsByReferenceNode = new Dictionary<ReferenceNode, ScriptNodeView>();
            referenceNodeViewsByNode = new Dictionary<IScriptNode, List<ScriptNodeView>>();
            
            foreach (var referenceNode in referenceNodes)
            {
                BuildReferenceNodeView(referenceNode);
            }
        }

        /// <summary>
        /// Builds the graph view.
        /// </summary>
        private void BuildGraphView()
        {
            ClearElements();

            elementsByNodeIds = new Dictionary<string, GraphElement>();
            viewsByNodeId = new Dictionary<string, ScriptNodeView>();
            groupNodeViewsByGroupNode = new Dictionary<GroupNode, GroupNodeView>();
            
            // Draw the nodes and reference nodes.
            BuildScriptNodes(graph.Nodes);
            BuildNodeReferenceNodes(graph.ReferenceNodes);
            
#if UNITY_2020_1_OR_NEWER
            viewsByNote = new Dictionary<Note, NoteView>();
            BuildNotes(graph.Notes);
#endif

            // Draw the group nodes and add the appropriate nodes to them.
            BuildGroupNodes(graph.GroupNodes);

            // Draw the connections between the nodes.
            foreach (var node in graph.Nodes)
            {
                if (!(node is IConsumerNode outputNode)) continue;
                
                ConnectOutputNode(outputNode);
            }
        }

        /// <summary>
        /// Colors the node's view red in the script graph view, to indicate that it failed.
        /// </summary>
        /// <param name="node">The node to mark.</param>
        /// <param name="warning">If true, it's just a warning, color orange instead of red.</param>
        public void HighlightFailedNode(IScriptNode node, bool warning = false)
        {
            var nodeView = viewsByNodeId[node.Id];
            nodeView.AddToClassList(warning ? "warning" : "failed");
        }
        
        /// <summary>
        /// Colors the node's view red in the script graph view, to indicate that it failed.
        /// </summary>
        /// <param name="markNodes">The nodes to mark.</param>
        /// <param name="warning">If true, it's just a warning, color orange instead of red.</param>
        public void HighlightFailedNodes(IEnumerable<IScriptNode> markNodes, bool warning = false)
        {
            foreach (var node in markNodes)
            {
                HighlightFailedNode(node, warning);
            }
        }
        
        public void HighlightSearchedNode(INode node)
        {
            var nodeView = elementsByNodeIds[node.Id];
            nodeView.AddToClassList("searched");
        }

        /// <summary>
        /// Removes all the highlight colors from nodes.
        /// </summary>
        public void ClearHighlights()
        {
            this.Query<ScriptNodeView>(className: "failed").ForEach(nodeView =>
            {
                nodeView.RemoveFromClassList("failed");
            });
            
            this.Query<ScriptNodeView>(className: "warning").ForEach(nodeView =>
            {
                nodeView.RemoveFromClassList("warning");
            });
            
            this.Query<GraphElement>(className: "searched").ForEach(nodeView =>
            {
                nodeView.RemoveFromClassList("searched");
            });
        }

        /// <summary>
        /// Checks whether a node view is a reference node of the other.
        /// </summary>
        /// <param name="possibleReferenceNodeView">The node view to check if it's a reference node of the original.</param>
        /// <param name="possibleOriginalNodeView">The node view to check if it's the original of the reference node.</param>
        /// <returns></returns>
        private bool NodeIsReferenceNodeOf(Node possibleReferenceNodeView, Node possibleOriginalNodeView)
        {
            if (possibleReferenceNodeView.userData == null) return false;   // This is not a reference node at all.
            
            var referenceNode = (ReferenceNode) possibleReferenceNodeView.userData;
            var originalNodeView = viewsByNodeId[referenceNode.ProviderNode.Id];

            return possibleOriginalNodeView == originalNodeView;
        }

        private bool NodeViewsReferToSameNode(Node nodeViewA, Node nodeViewB)
        {
            return NodeIsReferenceNodeOf(nodeViewA, nodeViewB) ||
                   NodeIsReferenceNodeOf(nodeViewB, nodeViewA);
        }

        /// <inheritdoc cref="GetCompatiblePorts"/>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            
            // There's a "ports" field, but it seems to be empty for some reason, so they're queried here.
            var allPorts = this.Query<Port>();

            allPorts.ForEach(port =>
            {
                if (startPort.direction == port.direction) return;  // These are both in ports or both out ports.
                if (startPort == port) return;                      // Port cannot connect to itself.
                if (startPort.node == port.node) return;            // Node cannot connect to itself.

                var inPort = startPort.direction == Direction.Input ? startPort : port;
                var outPort = startPort.direction == Direction.Output ? startPort : port;

                if (!inPort.portType.IsAssignableFrom(outPort.portType)) return;    // Port types aren't compatible.
                if (NodeViewsReferToSameNode(startPort.node, port.node)) return;           // A reference node cannot be connected to its original.
                
                compatiblePorts.Add(port); 
            });
    
            return compatiblePorts;
        }

        /// <summary>
        /// Adds a reference node to the graph.
        /// </summary>
        /// <param name="referenceNode">The reference node to add.</param>
        /// <param name="position">The position where the reference node should be placed in the graph.</param>
        public void AddNewReferenceNode(ReferenceNode referenceNode, Vector2 position)
        {
            Undo.RegisterCompleteObjectUndo(graph, "Add Reference Node");

            referenceNode.Position = new Rect(position, Vector2.zero);
            graph.AddReferenceNode(referenceNode);
            BuildReferenceNodeView(referenceNode);
            
            EditorUtility.SetDirty(graph);
        }

        /// <summary>
        /// Focus view on the node's view.
        /// </summary>
        /// <param name="node">The node.</param>
        public void FrameNode(INode node)
        {
            var restoreSelection = selection.ToList();
            
            ClearSelection();
            
            var nodeView = elementsByNodeIds[node.Id];
            AddToSelection(nodeView);
            
            FrameSelection();
            
            ClearSelection(); 
            
            foreach (var selectedNode in restoreSelection)
            {
                AddToSelection(selectedNode);
            }
        }

        /// <summary>
        /// Focus view on the nodes' view.
        /// </summary>
        /// <param name="frameNodes">The nodes.</param>
        public void FrameNodes(IEnumerable<INode> frameNodes)
        {
            var restoreSelection = selection.ToList();
            
            ClearSelection();

            foreach (var node in frameNodes)
            {
                var nodeView = elementsByNodeIds[node.Id];
                AddToSelection(nodeView);
            }
            
            FrameSelection();
            
            ClearSelection(); 
            
            foreach (var selectedNode in restoreSelection)
            {
                AddToSelection(selectedNode);
            }
        }

        public void RemoveFromGroup(GroupNode groupNode, IScriptNode node)
        {
            groupNode.Remove(node);
            var groupNodeView = groupNodeViewsByGroupNode[groupNode];
            var nodeView = viewsByNodeId[node.Id];
            groupNodeView.RemoveElement(nodeView);
            
            EditorUtility.SetDirty(graph);
        }

        public void RemoveFromGroup(GroupNode groupNode, ReferenceNode referenceNode)
        {
            groupNode.Remove(referenceNode);
            var groupNodeView = groupNodeViewsByGroupNode[groupNode];
            var referenceNodeView = nodeViewsByReferenceNode[referenceNode];
            groupNodeView.RemoveElement(referenceNodeView);
            
            EditorUtility.SetDirty(graph);
        }
        
#if UNITY_2020_1_OR_NEWER
        private Dictionary<Note, NoteView> viewsByNote;
        
        private void BuildNotes(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                BuildNoteView(note);
            }
        }
        
        public void AddNewNote(Note note, Rect position)
        {
            note.Position = position;
            graph.Add(note);

            EditorUtility.SetDirty(graph);
        }
        
        private void BuildNoteView(Note note)
        {
            var noteView = new NoteView(note);
            noteView.SetPosition(note.Position);
            noteView.title = note.Title;
            noteView.contents = note.Contents;
            noteView.theme = note.Theme;
            noteView.fontSize = note.FontSize;
            noteView.RegisterCallback<StickyNoteChangeEvent>(e =>
            {
                switch (e.change)
                {
                    case StickyNoteChange.Title:
                        note.Title = noteView.title;
                        break;
                    case StickyNoteChange.Contents:
                        note.Contents = noteView.contents;
                        break;
                    case StickyNoteChange.Position:
                        note.Position = noteView.GetPosition();
                        break;
                    case StickyNoteChange.Theme:
                        note.Theme = noteView.theme;
                        break;
                    case StickyNoteChange.FontSize:
                        note.FontSize = noteView.fontSize;
                        break;
                }
            });
            
            viewsByNote.Add(note, noteView);
            elementsByNodeIds.Add(note.Id, noteView);

            AddElement(noteView); 
        }
        
        private void NoteAdded(Note note)
        {
            BuildNoteView(note);
        }
        
        private void NoteRemoved(Note note)
        {
            var noteView = viewsByNote[note];
            RemoveElement(noteView);
            viewsByNote.Remove(note);
            elementsByNodeIds.Remove(note.Id);
        }
#endif
    }
}
