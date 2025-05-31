using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityNode = UnityEditor.Experimental.GraphView.Node;

namespace InsaneScatterbrain.ScriptGraph
{
    public class CopyPaste
    {
        private static readonly Vector2 DuplicationOffset = new Vector2(20, 20);

        private ScriptGraphView sourceGraphView;
        private ScriptGraphView targetGraphView;
        
        private readonly List<ISelectable> newSelectionQueue = new List<ISelectable>();
        
        private CopyData copyCutData;

        public bool HasCopyData => copyCutData != null && !copyCutData.IsEmpty;

        public void CopySelection(ScriptGraphView source)
        {
            sourceGraphView = source;
            copyCutData = CopySelectionData();
        }

        public void CutSelection(ScriptGraphView source)
        {
            sourceGraphView = source;
            Undo.RegisterCompleteObjectUndo(sourceGraphView.Graph, "Cut");
            
            copyCutData = CopySelectionData();
            sourceGraphView.DeleteSelection();
            
            EditorUtility.SetDirty(sourceGraphView.Graph);
        }
        
        public void PasteSelection(ScriptGraphView target, Vector2? position = null)
        {
            targetGraphView = target;
            Undo.RegisterCompleteObjectUndo(targetGraphView.Graph, "Paste");

            if (position != null)
            {
                PasteSelection(-copyCutData.SelectionCenter + position.Value, copyCutData);
            }
            else
            {
                var viewPortCenter = targetGraphView.ChangeCoordinatesTo(targetGraphView.contentViewContainer, targetGraphView.localBound.center);
                PasteSelection(-copyCutData.SelectionCenter + viewPortCenter, copyCutData);
            }
        }
        
        public void DuplicateSelection(ScriptGraphView graphView)
        {
            sourceGraphView = graphView;
            targetGraphView = graphView;
            
            Undo.RegisterCompleteObjectUndo(sourceGraphView.Graph, "Duplicate");
            
            var duplicateData = CopySelectionData();
            PasteSelection(DuplicationOffset, duplicateData);
        }

        private CopyData CopySelectionData()
        {
            var selection = sourceGraphView.Selection;
            if (selection.CountSelected == 1 && selection.GroupNodeViews.Count == 1)
            {
                // If a single group has been selected, duplicate the entire group.
                selection.SelectGroup(selection.GroupNodeViews.First());
            }
            
            var factory = new NodeFactory(sourceGraphView);
            var duplicator = new NodeDuplicator(factory);

            var data = new CopyData();

            var copyNodesByOriginalView = new Dictionary<UnityNode, IScriptNode>();
            var referenceNodesByOriginalView = new Dictionary<UnityNode, ReferenceNode>();
            var copyNodesByOriginalNode = new Dictionary<IScriptNode, IScriptNode>();
            var referenceNodesByOriginalNode = new Dictionary<ReferenceNode, ReferenceNode>();
            
            data.SelectionCenter = Vector2.zero;
            var numSelectedNodes = 0;

            foreach (var selectedNodeView in selection.ScriptNodeViews)
            {
                if (selectedNodeView.userData != null) continue;
                
                var newNode = duplicator.CreateCopy(selectedNodeView.Node);
                newNode.Position = selectedNodeView.Node.Position;
                data.ScriptNodes.Add(newNode);
                    
                copyNodesByOriginalView.Add(selectedNodeView, newNode);
                copyNodesByOriginalNode.Add(selectedNodeView.Node, newNode);
                    
                data.SelectionCenter += newNode.Position.position;
                numSelectedNodes++;
            }

            foreach (var selectedNodeView in selection.ScriptNodeViews)
            {
                if (selectedNodeView.userData == null) continue;
                
                // Is reference node.
                var referenceNode = (ReferenceNode) selectedNodeView.userData;
                var providerNode = (IProviderNode) selectedNodeView.Node;
                    
                // If referred to node is also being copied, make the reference refer to the copy of that node,
                // instead of the original one.
                if (copyNodesByOriginalNode.ContainsKey(providerNode))
                {
                    providerNode = (IProviderNode) copyNodesByOriginalNode[providerNode];
                }

                var newReferenceNode = new ReferenceNode(providerNode);
                newReferenceNode.Position = referenceNode.Position;

                data.ReferenceNodes.Add(newReferenceNode);
                    
                referenceNodesByOriginalView.Add(selectedNodeView, newReferenceNode);
                referenceNodesByOriginalNode.Add(referenceNode, newReferenceNode);

                data.SelectionCenter += referenceNode.Position.position;
                numSelectedNodes++;
            }

            foreach (var selectedGroupNodeView in selection.GroupNodeViews)
            {
                var originalGroupNode = selectedGroupNodeView.GroupNode;
                var copyGroupNode = new GroupNode(originalGroupNode.Title);
                copyGroupNode.Position = originalGroupNode.Position;

                foreach (var originalNode in originalGroupNode.Nodes)
                {
                    if (!copyNodesByOriginalNode.ContainsKey(originalNode)) continue;
                    
                    var copyNode = copyNodesByOriginalNode[originalNode];
                    copyGroupNode.Add(copyNode);
                }
                
                foreach (var originalReferenceNode in originalGroupNode.ReferenceNodes)
                {
                    if (!referenceNodesByOriginalNode.ContainsKey(originalReferenceNode)) continue;
                    
                    var copyReferenceNode = referenceNodesByOriginalNode[originalReferenceNode];
                    copyGroupNode.Add(copyReferenceNode);
                }

                data.GroupNodes.Add(copyGroupNode);
            }
            
#if UNITY_2020_1_OR_NEWER
            foreach (var selectedNoteView in selection.NoteViews)
            {
                var originalNote = selectedNoteView.Note;
                var copyNote = CopyNote(originalNote);

                data.Notes.Add(copyNote);
                
                data.SelectionCenter += copyNote.Position.position;
                numSelectedNodes++;
            }
#endif

            data.SelectionCenter /= numSelectedNodes;
            
            foreach (var selectedEdge in selection.Edges)
            {
                var inputNode = selectedEdge.input.node;
                var outputNode = selectedEdge.output.node;
                
                if (!selection.ScriptNodeViews.Contains(inputNode) || 
                    !selection.ScriptNodeViews.Contains(outputNode)) continue;

                var consumerNode = (IConsumerNode) copyNodesByOriginalView[inputNode];

                ReferenceNode referenceNode = null;
                IProviderNode providerNode = null;
                if (referenceNodesByOriginalView.ContainsKey(outputNode))
                {
                    referenceNode = referenceNodesByOriginalView[outputNode];
                    providerNode = referenceNode.ProviderNode;
                }
                else
                {
                    providerNode = (IProviderNode) copyNodesByOriginalView[outputNode];
                }
                

                var inPortName = selectedEdge.input.portName;
                var outPortName = selectedEdge.output.portName;

                data.Connections.Add((consumerNode, inPortName, providerNode, outPortName, referenceNode));
            }

            return data;
        }

        private void PasteSelection(Vector2 offset, CopyData data)
        {
            targetGraphView.ClearSelection();
            newSelectionQueue.Clear();
            
            var factory = new NodeFactory(targetGraphView);
            var duplicator = new NodeDuplicator(factory);

            var pasteNodeByCopyNode = new Dictionary<IScriptNode, IScriptNode>();
            var pasteNodeByCopyNodeReferences = new Dictionary<ReferenceNode, ReferenceNode>();
            
            foreach (var copyScriptNode in data.ScriptNodes)
            {
                if (sourceGraphView.Graph != targetGraphView.Graph &&
                    (copyScriptNode is InputNode || copyScriptNode is OutputNode))
                {
                    // Copying input or output nodes from one graph doesn't work, as the parameter it's referring to doesn't
                    // exist in the target graph. So these are skipped.

                    if (copyScriptNode is InputNode inputNode)
                    {
                        var inputParameterName = sourceGraphView.Graph.InputParameters.GetName(inputNode.InputParameterId);
                        Debug.LogWarning($"Input node referring to parameter {inputParameterName} node that doesn't exist in this graph. Skipping.");
                    }
                    else if (copyScriptNode is OutputNode outputNode)
                    {
                        var outputParameterName = sourceGraphView.Graph.OutputParameters.GetName(outputNode.OutputParameterId);
                        Debug.LogWarning($"Output node referring to parameter {outputParameterName} node that doesn't exist in this graph. Skipping.");
                    }
                    
                    continue;
                }

                var newNode = duplicator.CreateCopy(copyScriptNode);
                var position = copyScriptNode.Position.position + offset;
                targetGraphView.AddNewNode(newNode, position, false);
                targetGraphView.AddToSelection(targetGraphView.GetView(newNode));
                
                pasteNodeByCopyNode.Add(copyScriptNode, newNode);
            }
            
            foreach (var copyReferenceNode in data.ReferenceNodes)
            {
                // If referred to node is also being copied, make the reference refer to the copy of that node,
                // instead of the original one.
                var providerNode = copyReferenceNode.ProviderNode;
                if (pasteNodeByCopyNode.ContainsKey(providerNode))
                {
                    providerNode = (IProviderNode) pasteNodeByCopyNode[providerNode];
                }

                if (!targetGraphView.Graph.Nodes.Contains(providerNode))
                {
                    // The target graph does not contain the node that the reference node is referring to, which
                    // is obviously not a valid state. So we're skipping this one and leaving a warning.
                    var providerNodeView = sourceGraphView.GetView(providerNode);
                    Debug.LogWarning($"Reference node referring to a {providerNodeView.title} node that doesn't exist in this graph. Skipping.");
                    continue;
                }
                
                var newReferenceNode = new ReferenceNode(providerNode);
                var position = copyReferenceNode.Position.position + offset;
                targetGraphView.AddNewReferenceNode(newReferenceNode, position);
                targetGraphView.AddToSelection(targetGraphView.GetReferenceNodeView(newReferenceNode));
                
                pasteNodeByCopyNodeReferences.Add(copyReferenceNode, newReferenceNode);
            }

#if UNITY_2020_1_OR_NEWER
            foreach (var copyNote in data.Notes)
            {
                var newNote = CopyNote(copyNote);
                var position = newNote.Position;
                position.position += offset;
                targetGraphView.AddNewNote(newNote, position);
                
                newSelectionQueue.Add(targetGraphView.GetNoteView(newNote));
            }
#endif

            foreach (var copyConnection in data.Connections)
            {
                var copyConsumerNode = copyConnection.Item1;
                var inPortName = copyConnection.Item2;
                var copyProviderNode = copyConnection.Item3;
                var outPortName = copyConnection.Item4;
                var copyReferenceNode = copyConnection.Item5;
                
                if (!pasteNodeByCopyNode.ContainsKey(copyConsumerNode))
                {
                    // This is a connection to a node that's not copied over, skip it.
                    continue;
                }
                
                var pasteConsumerNode = (IConsumerNode) pasteNodeByCopyNode[copyConsumerNode];

                ReferenceNode pasteReferenceNode = null;
                IProviderNode pasteProviderNode;
                if (copyReferenceNode != null)
                {
                    if (!pasteNodeByCopyNodeReferences.ContainsKey(copyReferenceNode))
                    {
                        // This is a connection to a node that's not copied over, skip it.
                        continue;
                    }
                    
                    pasteReferenceNode = pasteNodeByCopyNodeReferences[copyReferenceNode];
                    pasteProviderNode = pasteReferenceNode.ProviderNode;
                }
                else
                {
                    if (!pasteNodeByCopyNode.ContainsKey(copyProviderNode))
                    {
                        // This is a connection to a node that's not copied over, skip it.
                        continue;
                    }
                    
                    pasteProviderNode = (IProviderNode) pasteNodeByCopyNode[copyProviderNode];
                }

                var pasteInPort = pasteConsumerNode.GetInPort(inPortName);
                var pasteOutPort = pasteProviderNode.GetOutPort(outPortName);

                pasteInPort.Connect(pasteOutPort);
                var edge = targetGraphView.Connect(pasteInPort, pasteOutPort, pasteReferenceNode);

                newSelectionQueue.Add(edge);
            }
            
            foreach (var copyGroupNode in data.GroupNodes)
            {
                var newGroupNode = new GroupNode(copyGroupNode.Title);
                foreach (var copyNode in copyGroupNode.Nodes)
                {
                    var pasteNode = pasteNodeByCopyNode[copyNode];
                    newGroupNode.Add(pasteNode);
                }

                foreach (var copyReferenceNode in copyGroupNode.ReferenceNodes)
                {
                    var pasteReferenceNode = pasteNodeByCopyNodeReferences[copyReferenceNode];
                    newGroupNode.Add(pasteReferenceNode);
                }

                var position = copyGroupNode.Position.position + offset;
                
                targetGraphView.AddNewGroupNode(newGroupNode, position);

                newSelectionQueue.Add(targetGraphView.GetGroupView(newGroupNode));
            }

            QueueNewSelection();
            
            EditorUtility.SetDirty(targetGraphView.Graph);
        }
        
        private async void QueueNewSelection()
        {
            await Task.Delay(1);    // Wait a bit until the group node view has been initialized before adding it to the selection
            
            foreach (var selectable in newSelectionQueue)
            {
                targetGraphView.AddToSelection(selectable);
            }
        }
        
#if UNITY_2020_1_OR_NEWER
        private Note CopyNote(Note original)
        {
            return new Note
            {
                Position = original.Position,
                Title = original.Title,
                Contents = original.Contents
            };
        }
#endif
    }
}