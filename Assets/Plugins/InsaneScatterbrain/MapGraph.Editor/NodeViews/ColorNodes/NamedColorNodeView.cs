using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(NamedColorNode))]
    public class NamedColorNodeView : ScriptNodeView
    {
        private readonly NamedColorNode namedColorNode;
        private readonly MapGraphGraph graph;
        private readonly List<string> colorNames;
        private PopupField<string> namePopup;
        
        private const string EmptyTexture = "[None]";

        public NamedColorNodeView(NamedColorNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            namedColorNode = node;
            graph = (MapGraphGraph) graphView.Graph;
            colorNames = new List<string>();
            
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                if (graph.NamedColorSet == null) return;
                
                graph.NamedColorSet.OnRenamed -= ColorRenamed;
                graph.NamedColorSet.OnRenamed += ColorRenamed;

                graph.NamedColorSet.OnAdded -= ColorAdded;
                graph.NamedColorSet.OnAdded += ColorAdded;
                
                graph.NamedColorSet.OnRemoved -= ColorRemoved;
                graph.NamedColorSet.OnRemoved += ColorRemoved;
                
                graph.NamedColorSet.OnMoved -= ColorMoved;
                graph.NamedColorSet.OnMoved += ColorMoved;
            });

            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (graph.NamedColorSet == null) return;
                
                graph.NamedColorSet.OnRenamed -= ColorRenamed;
                graph.NamedColorSet.OnAdded -= ColorAdded;
                graph.NamedColorSet.OnRemoved -= ColorRemoved;
                graph.NamedColorSet.OnMoved -= ColorMoved;
            });
        }

        private void ColorRenamed(string id, string oldColorName, string newColorName)
        {
            var index = colorNames.IndexOf(oldColorName);
            colorNames[index] = newColorName;

            if (namePopup.value != oldColorName) return;

            namePopup.value = newColorName;
        }

        private void ColorAdded(string colorId)
        {
            var colorName = graph.NamedColorSet.GetName(colorId);
            colorNames.Add(colorName);
        }

        private void ColorRemoved(string colorId, string colorName)
        {
            // Color was just removed, that should've created a new undo group. Use the same name here.
            var undoName = Undo.GetCurrentGroupName();
            
            Undo.RegisterCompleteObjectUndo(graph, undoName);
            
            if (namePopup.value == colorName)
            {
                namePopup.index = 0;
            }

            var index = colorNames.IndexOf(colorName);
            
            colorNames.RemoveAt(index);
        }

        private void ColorMoved(int oldIndex, int newIndex)
        {
            var colorName = colorNames[oldIndex + 1];
            colorNames.RemoveAt(oldIndex + 1);
            colorNames.Insert(newIndex + 1, colorName);
        }
        
        public override void Initialize()
        {
            base.Initialize();

            colorNames.Clear();
            colorNames.Add(EmptyTexture);
            
            var selectedIndex = -1;
            if (graph.NamedColorSet != null) 
            {
                foreach (var namedColorId in graph.NamedColorSet.OrderedIds)
                {
                    var colorName = graph.NamedColorSet.GetName(namedColorId);
                    colorNames.Add(colorName);
                }
                
                selectedIndex = graph.NamedColorSet.OrderedIds.IndexOf(namedColorNode.NamedColorId) + 1;
            }

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            
            namePopup = new PopupField<string>(colorNames, selectedIndex);
            namePopup.RegisterValueChangedCallback(e =>
            {
                var index = namePopup.index - 1;
                var id = index < 0 ? null : graph.NamedColorSet.OrderedIds[index];
                
                namedColorNode.NamedColorId = id;
                EditorUtility.SetDirty(graph);
            });

            inputContainer.Add(namePopup);

            RefreshExpandedState();
            RefreshPorts(); 
        }
    }
}