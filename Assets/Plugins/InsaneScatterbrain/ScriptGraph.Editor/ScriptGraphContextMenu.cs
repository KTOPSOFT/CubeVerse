using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor.NodeSearch;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This class represents the context menu of the script graph editor.
    /// </summary>
    public class ScriptGraphContextMenu
    {
        private readonly ScriptGraphView graphView;
        private readonly ScriptGraphViewSelection selection;
        private readonly CopyPaste copyPaste;
        private readonly NodeSearchWindow nodeSearchWindow;

        private IEnumerable<Type> nodeTypes;
        private HashSet<Type> referenceNodeTypes;

        private bool initialized;

        public ScriptGraphContextMenu(ScriptGraphView graphView, ScriptGraphViewSelection selection, CopyPaste copyPaste, NodeSearchWindow nodeSearchWindow)
        {
            this.graphView = graphView;
            this.selection = selection;
            this.copyPaste = copyPaste;
            this.nodeSearchWindow = nodeSearchWindow;
        }
        
        /// <summary>
        /// Builds the context menu entries based on all the available types of nodes.
        /// </summary>
        private void Initialize()
        {
            // Look for all the classes that are using the script node attribute in all assemblies.
            nodeTypes = Types.WithAttribute<ScriptNodeAttribute>();

            referenceNodeTypes = new HashSet<Type>();
            referenceNodeTypes.Add(typeof(ProcessGraphNode));
            foreach (var nodeType in nodeTypes)
            {
                var attribute = nodeType.GetAttribute<ScriptNodeAttribute>();
                // Keeps track of which node types can be created reference nodes for.
                if (attribute.CanBeReferenceNode)
                {
                    referenceNodeTypes.Add(nodeType);
                }
            }

            initialized = true;
        }

        private static readonly Vector2 DuplicationOffset = new Vector2(20, 20);

        /// <summary>
        /// Displays the context menu.
        /// </summary>
        /// <param name="e"></param>
        public void Open(ContextualMenuPopulateEvent e)
        {
            // If the context menu hasn't been created, do so now.
            if (!initialized)
            {
                Initialize();
            }
            
            selection.Update();
            
            var screenPos = GUIUtility.GUIToScreenPoint(e.localMousePosition);
            var placementPos = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, e.localMousePosition);

            var target = e.target;
            if (target is ScriptNodeView nodeView)
            {
                if (nodeView.Node is ProcessGraphNode)
                {
                    var processGraphNode = (ProcessGraphNode) nodeView.Node;
                    
                    e.menu.AppendAction("Edit Graph", action =>
                    {
                        ScriptGraphViewWindow.CreateGraphViewWindow(processGraphNode.SubGraph);
                    });
                    
                    e.menu.AppendAction("Inspect Graph", action =>
                    {
                        // Select the graph asset in the project window.
                        Selection.activeObject = processGraphNode.SubGraph;
                    });
                    
                    e.menu.AppendSeparator();
                }
                
                if (nodeView.userData == null)
                {
                    // If the context menu is opened on a node, open the context menu specific to existing node.
                    if (string.IsNullOrEmpty(nodeView.Node.Note))
                    {
                        e.menu.AppendAction("Add Comment", action =>
                        {
                            nodeView.ShowNote();
                        });
                    }
                    else
                    {
                        e.menu.AppendAction("Remove Comment", action =>
                        {
                            nodeView.Node.Note = string.Empty;
                            nodeView.HideNote();
                        });
                    }
                }

                if (nodeView.Node is IProviderNode providerNode)
                {
                    // Check if the node can be a reference node. If so add the option to the context menu.
                    // The node can't be a reference node, if it's an constant or input node, 
                    // or if it's already a reference node of another node.
                    if (!(providerNode is ConstantNode) && 
                        !(providerNode is InputNode) && 
                        referenceNodeTypes.Contains(providerNode.GetType()) &&
                        nodeView.userData == null) // If userData is filled this is already a reference node, so don't create a reference node from that.
                    {
                        e.menu.AppendAction("Create Reference Node", action =>
                        {
                            Undo.RegisterCompleteObjectUndo(graphView.Graph, "New Reference Node");

                            var referenceNode = new ReferenceNode(providerNode);
                            graphView.AddNewReferenceNode(referenceNode, nodeView.Node.Position.position + DuplicationOffset);
                        });
                    }
                    else if (nodeView.userData != null)
                    {
                        e.menu.AppendAction("Show Original Node", action =>
                        {
                            var referenceNode = (ReferenceNode) nodeView.userData;
                            graphView.FrameNode(referenceNode.ProviderNode);
                        });
                    }
                }
                
                e.menu.AppendSeparator();
            }
#if UNITY_2020_1_OR_NEWER
            else if (!(target is NoteView))
#else
            else
#endif
            {
                // The context menu is opened on an empty space. Show the context menu options for that.
                var addNodeText = target is GroupNodeView ? "Add Node To Group" : "Add Node";
                e.menu.AppendAction(addNodeText, action =>
                {
                    nodeSearchWindow.Open(screenPos, placementPos);
                });
                e.menu.AppendSeparator();
                
                if (!(target is GroupNodeView))
                {
                    e.menu.AppendAction("Add Group", action =>
                    {
                        var newGroupNode = new GroupNode("New Group");
                        graphView.AddNewGroupNode(newGroupNode, placementPos);
                    });
                    e.menu.AppendSeparator();
                }
                else if (graphView.Selection.CountSelected == 1)
                {
                    e.menu.AppendAction("Duplicate Group", action =>
                    {
                        copyPaste.DuplicateSelection(graphView);
                    });
                    e.menu.AppendSeparator();
                }

#if UNITY_2020_1_OR_NEWER
                e.menu.AppendAction("Add Note", action =>
                {
                    var notePosition = new Rect(placementPos, Vector2.zero);
                    notePosition.width = 250;
                    notePosition.height = 200;
                    var note = new Note
                    {
                        Position = notePosition,
                        Title = "Note Title",
                        Contents = "Note"
                    };
                    graphView.AddNewNote(note, notePosition);
                });
                
                e.menu.AppendSeparator();
#endif
            }
            
            var selectionLabel = "Selection";
            if (selection.CountSelected < 2)
            {
#if UNITY_2020_1_OR_NEWER
                selectionLabel = selection.NoteViews.Count > 0 ? "Note" : "Node";
#else
                selectionLabel = "Node";
#endif
            }

            if (selection.CountSelected > 0)
            {
                selection.Update();

                if (!selection.IsNodeSelected) return;

                if (!(selection.CountSelected == 1 && target is GroupNodeView))
                {
                    var duplicateText = $"Duplicate {selectionLabel}";
                    e.menu.AppendAction(duplicateText, action =>
                    {
                        copyPaste.DuplicateSelection(graphView);
                    });
                }

                var cutText = $"Cut {selectionLabel}";
                e.menu.AppendAction(cutText, action => 
                {
                    copyPaste.CutSelection(graphView);
                });
                
                var copyText = selection.CountSelected > 1 
                    ? "Copy Selection" 
                    : target is GroupNodeView ? "Copy Group" : $"Copy {selectionLabel}";
                
                e.menu.AppendAction(copyText, action =>
                {
                    copyPaste.CopySelection(graphView);
                });
            }

            if (copyPaste.HasCopyData)
            {
                e.menu.AppendAction("Paste", action =>
                {
                    copyPaste.PasteSelection(graphView, placementPos); 
                });
            }

            e.menu.AppendSeparator();

            if (selection.CountSelected == 0) return;
            
            if (selection.SelectedItemInGroup)
            {
                var detachText = selection.ScriptNodeViews.Count > 1 ? "Detach Selection From Group(s)" : "Detach From Group";
                Undo.RegisterCompleteObjectUndo(graphView.Graph, detachText);
                
                e.menu.AppendAction(detachText, action =>
                {
                    foreach (var selectedNodeView in selection.ScriptNodeViews)
                    {
                        var referenceNode = selectedNodeView.userData as ReferenceNode;

                        foreach (var groupNode in graphView.Graph.GroupNodes)
                        {
                            if (referenceNode != null && groupNode.Contains(referenceNode))
                            {
                                graphView.RemoveFromGroup(groupNode, referenceNode);
                                break;
                            }

                            if (groupNode.Contains(selectedNodeView.Node))
                            {
                                graphView.RemoveFromGroup(groupNode, selectedNodeView.Node);
                                break;
                            }
                        }
                    }
                });
                
                e.menu.AppendSeparator();
            }
            
            e.menu.AppendAction($"Delete {selectionLabel}", action =>
            {
                Undo.RegisterCompleteObjectUndo(graphView.Graph, "Delete");
                
                graphView.DeleteSelection();
            });
        }
    }
}