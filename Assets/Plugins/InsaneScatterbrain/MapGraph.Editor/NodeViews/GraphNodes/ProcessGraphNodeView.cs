using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ProcessGraphNode))]
    public class ProcessGraphNodeView : ScriptNodeView
    {
        private readonly ProcessGraphNode node;

        private OutPort firstTextureOut;
        
        private VisualElement imageContainer;

        public override Texture2D Icon =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Plugins/InsaneScatterbrain/ScriptGraph.Editor.Assets/Icons/graph.png");

        public ProcessGraphNodeView(ProcessGraphNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.node = node;
            var fieldContainer = new VisualElement();
            var divider = new VisualElement {name = "divider"};
            divider.AddToClassList("horizontal");

            if (!node.IsNamed)
            {
                fieldContainer.AddToClassList("field-container");

                var field = new ObjectField();
                field.SetValueWithoutNotify(this.node.SubGraph);
                field.RegisterValueChangedCallback(e =>
                {
                    var newSubGraph = (ScriptGraphGraph)e.newValue;

                    if (newSubGraph == Graph)
                    {
                        EditorUtility.DisplayDialog("Invalid graph selected",
                            "Graph cannot contain a node to process itself.", "OK");

                        field.SetValueWithoutNotify(e.previousValue);
                        return;
                    }

                    node.ClearPorts();

                    UnregisterSubGraph();
                    node.SubGraph = newSubGraph;
                    RegisterSubGraph();

                    node.OnLoadInputPorts();
                    node.OnLoadOutputPorts();

                    EditorUtility.SetDirty(Graph);

                    AssignFirstTextureOut();

                    RefreshExpandedState();
                    RefreshPorts();
                });

                fieldContainer.Add(divider);
                fieldContainer.Add(field);
                mainContainer.Add(fieldContainer);

                field.objectType = typeof(ScriptGraphGraph);
            }

            RegisterSubGraph();

            AssignFirstTextureOut();

            var customPreviewBehaviour = node.SubGraph.CustomPreviewBehaviour;
            if (customPreviewBehaviour != null)
            {
                this.AddPreview<ProcessGraphNode>(customPreviewBehaviour.GetPreviewTexture);
            }
            else if (firstTextureOut != null)
            {
                this.AddPreview<ProcessGraphNode>(GetPreviewTexture);
            }

            Refresh();
        }

        private Texture2D GetPreviewTexture(ProcessGraphNode nodeInstance)
        {
            OutPort instanceFirstTextureOut = null;
            
            foreach (var outPort in nodeInstance.OutPorts)
            {
                if (outPort.Type != typeof(TextureData)) continue;
                
                instanceFirstTextureOut = outPort;
                break;
            }

            if (instanceFirstTextureOut == null) return null;
            
            var textureData = instanceFirstTextureOut.Get<TextureData>();
            
            return textureData.ToTexture2D();
        }

        protected override void InitializeTitle()
        {
            if (node.SubGraph == null)
            {
                title = "[Graph Not Found]";
                return;
            }
            
            title = node.IsNamed ? node.SubGraph.name : "Process Graph";

            AddIcon();
        }

        private void RegisterSubGraph()
        {
            if (node.SubGraph == null) return;
            
            node.OnInPortAdded += AddInPort;
            node.OnInPortRemoved += RemoveInPort;
            node.OnInPortRenamed += RenameInPort;
            node.OnInPortMoved += MoveInPort;
            
            node.OnOutPortAdded += AddOutPort;
            node.OnOutPortRemoved += RemoveOutPort;
            node.OnOutPortRenamed += RenameOutPort;
            node.OnOutPortMoved += MoveOutPort;
        }
        
        private void UnregisterSubGraph()
        {
            if (node.SubGraph == null) return;
            
            node.OnInPortAdded -= AddInPort;
            node.OnInPortRemoved -= RemoveInPort;
            node.OnInPortRenamed -= RenameInPort;
            node.OnInPortMoved -= MoveInPort;
            
            node.OnOutPortAdded -= AddOutPort;
            node.OnOutPortRemoved -= RemoveOutPort;
            node.OnOutPortRenamed -= RenameOutPort;
            node.OnOutPortMoved -= MoveOutPort;
        }

        private void AssignFirstTextureOut()
        {
            imageContainer?.RemoveFromHierarchy();

            imageContainer = null;
            firstTextureOut = null;
            
            foreach (var outPort in node.OutPorts)
            {
                if (outPort.Type != typeof(TextureData)) continue;
                
                firstTextureOut = outPort;
                break;
            }
        }
    }
}