using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InsaneScatterbrain.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityNode = UnityEditor.Experimental.GraphView.Node;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// The default script node view.
    /// </summary>
    public class ScriptNodeView : UnityNode
    {
        private readonly ScriptGraphView graphView;
        private readonly IScriptNode node;
        private readonly Dictionary<string, Port> inputs = new Dictionary<string, Port>();
        private readonly Dictionary<string, Port> outputs = new Dictionary<string, Port>();
        
        private TextField noteText;
        private Label executionTimeLabel;

        /// <summary>
        /// Gets the node this node view represents.
        /// </summary>
        public IScriptNode Node => node;
        
        /// <summary>
        /// Gets the graph this node is a part of.
        /// </summary>
        protected ScriptGraphGraph Graph => graphView.Graph;

        protected ScriptGraphView GraphView => graphView;
        
        /// <summary>
        /// Gets all the input port views.
        /// </summary>
        public IReadOnlyDictionary<string, Port> Inputs => inputs;
        
        /// <summary>
        /// Gets all the output port views.
        /// </summary>
        public IReadOnlyDictionary<string, Port> Outputs => outputs;

        public bool IsNoteReadOnly
        {
            get => noteText.isReadOnly;
            set => noteText.isReadOnly = value;
        }

        public virtual Texture2D Icon => null;
        
        public event Action OnDispose;

        /// <summary>
        /// Creates a new script node view for the given node and script graph view.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="graphView">The graph view.</param>
        public ScriptNodeView(IScriptNode node, ScriptGraphView graphView)
        {
            this.node = node;
            this.graphView = graphView;
        }

        private void NodeProcessingCompleted(IProcessorNode processedNode)
        {
            if (processedNode.Id != node.Id) return;
            
            executionTimeLabel.text = $"{processedNode.LatestExecutionTime} ms";
        }

        /// <summary>
        /// Adds the default USS classes to the port.
        /// </summary>
        /// <param name="port">The port.</param>
        private static void AddTypeClassesToPort(Port port)
        {
            port.AddToClassList(GetTypeClassName(port.portType, true));
            port.AddToClassList(GetTypeClassName(port.portType, false));
        }

        /// <summary>
        /// Gets the USS classname for the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fullname">Whether or not the type's fullname should be used.</param>
        /// <returns>The USS classname for the given type.</returns>
        private static string GetTypeClassName(Type type, bool fullname)
        {
            var typeName = fullname ? type.GetFriendlyFullname() : type.GetFriendlyName();
            
            return typeName
                .Replace('.', '-')
                .Replace("[]", "Array")
                .Replace("<", "_")
                .Replace(">", "_");
        }

        protected void AddIcon()
        {
            if (Icon == null)
            {
                return;
            }
            
            var iconWrapper = new VisualElement {name = "icon-wrapper"};
            var icon = new Image {name = "icon"};
            icon.AddToClassList("icon");
            icon.image = Icon;
            icon.style.maxHeight = 12;
            
            // Vertically center the icon.
            iconWrapper.style.alignSelf = Align.Center;
            iconWrapper.style.paddingRight = 8;
            
            iconWrapper.Add(icon);
            titleContainer.Add(iconWrapper);
        }

        protected virtual void InitializeTitle()
        {
            var type = node.GetType();
            
            // Get the name from the ScriptNodeAttribute and use it as the title for the node view.
            var nodeAttribute = (ScriptNodeAttribute) type.
                GetCustomAttributes(typeof(ScriptNodeAttribute), true).
                FirstOrDefault();

            if (nodeAttribute == null) return;
            
            title = nodeAttribute.Name;
                
            // If this is a node native to Map Graph, add a link to the Node Index page.
            if (type.Namespace == null || !type.Namespace.StartsWith("InsaneScatterbrain"))
            {
                // Custom nodes can have custom icons, check if one is set and if so, make sure it's added to
                // the title bar.
                AddIcon();
                return;
            }
            
            var nodeAnchor = nodeAttribute.Name.Replace(' ', '-').ToLower();
            nodeAnchor = Regex.Replace(nodeAnchor, @"[^\w\-]", string.Empty);
                    
            var indexLink = $"{Urls.NodeIndex}#{nodeAnchor}";
                    
            var indexLinkLabel = new Button(() => { Application.OpenURL(indexLink); }) {text = "?"};
            indexLinkLabel.AddToClassList("index-link");
                
            var sizeAuto = new StyleLength(StyleKeyword.Auto);
            indexLinkLabel.style.marginBottom = sizeAuto;
            indexLinkLabel.style.marginTop = sizeAuto;
            indexLinkLabel.style.fontSize = new StyleLength(15);

            titleContainer.Add(indexLinkLabel);
        }

        /// <summary>
        /// Initializes the node view.
        /// </summary>
        public virtual void Initialize()
        {
            InitializeTitle();

            AddNote();
            InitializePorts();
            
            if (graphView.ShowDebugInfo && node is IProcessorNode processorNode)
            {
                // If we want to see the debug information add a additional label to all processor nodes that display
                // the latest execution time.
                executionTimeLabel = new Label($"{processorNode.LatestExecutionTime} ms");
                executionTimeLabel.AddToClassList("latest-execution-time");
                mainContainer.Add(executionTimeLabel);

                ProcessorNode.NodeProcessingCompleted += NodeProcessingCompleted;
                OnDispose += () =>
                {
                    ProcessorNode.NodeProcessingCompleted -= NodeProcessingCompleted;
                };
            }
        }

        private void AddNote()
        {
            var nodeBorder = this.Q("node-border");
            var titleElement = nodeBorder.Q("title");
            var titleIndex = nodeBorder.IndexOf(titleElement);
            
            noteText = new TextField();
            noteText.AddToClassList("node-note");
            noteText.multiline = true;

            noteText.RegisterValueChangedCallback(evt =>
            {
                node.Note = noteText.value.Trim();
                EditorUtility.SetDirty(Graph);
            });

            noteText.RegisterCallback<BlurEvent>(evt =>
            {
                node.Note = noteText.value.Trim();
                
                if (node.Note.Trim() != string.Empty) return;
                
                noteText.style.display = DisplayStyle.None;
            });

            if (string.IsNullOrEmpty(node.Note))
            {
                noteText.style.display = DisplayStyle.None;
            }
            else
            {
                noteText.value = node.Note;
            }

            nodeBorder.Insert(titleIndex+1, noteText);
        }
        
        public async void ShowNote()
        {
            noteText.style.display = DisplayStyle.Flex;
            await Task.Delay(1);    // Wait a bit until the style change has been processed, before focussing on the element.
            noteText.ElementAt(0).Focus();
        }

        public void HideNote()
        {
            noteText.value = string.Empty;
            noteText.style.display = DisplayStyle.None;
        }

        private void InitializePorts()
        {
            if (node is IConsumerNode outputNode)
            {
                foreach (var inPort in outputNode.InPorts)
                {
                    AddInPort(inPort);
                }
            }

            if (node is IProviderNode inputNode)
            {
                foreach (var outPort in inputNode.OutPorts)
                {
                    AddOutPort(outPort);
                }
            }

            Refresh();
        }

        protected void Refresh()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
        
        private void AddPort(ScriptGraphPort port, Port portView, VisualElement container, Dictionary<string, Port> ports)
        {
            portView.portName = port.Name;
            portView.userData = port;
            AddTypeClassesToPort(portView);

            container.Add(portView);
            ports.Add(port.Name, portView);

            Refresh();
            
            GraphView.InitializePort(portView);
            
            EditorUtility.SetDirty(Graph);
        }
        
        protected void AddInPort(InPort input)
        {
            if (inputs.ContainsKey(input.Name)) return;
            
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, input.Type);
            AddPort(input, inputPort, inputContainer, inputs);
        }
        
        protected void AddOutPort(OutPort output)
        {
            if (outputs.ContainsKey(output.Name)) return;
            
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, output.Type);
            AddPort(output, outputPort, outputContainer, outputs);
        }

        private void RemovePort(ScriptGraphPort port, VisualElement container, Dictionary<string, Port> ports)
        {
            var portView = ports[port.Name];
            var removeEdges = portView.connections.ToArray();
            foreach (var edge in removeEdges)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
            container.Remove(portView);
            ports.Remove(port.Name);
            
            Refresh();
            
            EditorUtility.SetDirty(Graph);
        }

        protected void RemoveInPort(InPort input) => RemovePort(input, inputContainer, inputs);
        protected void RemoveOutPort(OutPort output) => RemovePort(output, outputContainer, outputs);

        private void MovePort(ScriptGraphPort port, int newIndex, VisualElement container,
            Dictionary<string, Port> ports)
        {
            var portView = ports[port.Name];
            container.Remove(portView);
            container.Insert(newIndex, portView);
        }

        protected void MoveInPort(InPort input, int newIndex) => MovePort(input, newIndex, inputContainer, inputs);
        protected void MoveOutPort(OutPort output, int newIndex) => MovePort(output, newIndex, outputContainer, outputs);

        private void RenamePort(ScriptGraphPort port, string oldName, string newName, Dictionary<string, Port> ports)
        {
            var portView = ports[oldName];
            ports.Remove(oldName);
            
            portView.portName = newName;
            ports.Add(port.Name, portView);
        }
        
        protected void RenameInPort(InPort input, string oldName, string newName) => RenamePort(input, oldName, newName, inputs);

        protected void RenameOutPort(OutPort output, string oldName, string newName) => RenamePort(output, oldName, newName, outputs);
        
        public void Dispose() => OnDispose?.Invoke();
    }
}
