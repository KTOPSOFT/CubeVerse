using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityNode = UnityEditor.Experimental.GraphView.Node;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class ScriptGraphViewSelection
    {
        private readonly ScriptGraphView graphView;

        private readonly List<ScriptNodeView> scriptNodeViews = new List<ScriptNodeView>();
        private readonly List<GroupNodeView> groupNodeViews = new List<GroupNodeView>();
        private readonly List<Edge> edges = new List<Edge>();
        
        private ReadOnlyCollection<ScriptNodeView> readOnlyScriptNodeViews;
        private ReadOnlyCollection<GroupNodeView> readOnlyGroupNodeViews;
        private ReadOnlyCollection<Edge> readOnlyEdges;

        public ReadOnlyCollection<ScriptNodeView> ScriptNodeViews =>
            readOnlyScriptNodeViews ?? (readOnlyScriptNodeViews = scriptNodeViews.AsReadOnly());

        public ReadOnlyCollection<GroupNodeView> GroupNodeViews =>
            readOnlyGroupNodeViews ?? (readOnlyGroupNodeViews = groupNodeViews.AsReadOnly());

        public ReadOnlyCollection<Edge> Edges => readOnlyEdges ?? (readOnlyEdges = edges.AsReadOnly());
        
        public bool SelectedItemInGroup { get; private set; }
        
#if UNITY_2020_1_OR_NEWER
        private readonly List<NoteView> noteViews = new List<NoteView>();
        private ReadOnlyCollection<NoteView> readOnlyNoteViews;
        public ReadOnlyCollection<NoteView> NoteViews => readOnlyNoteViews ?? (readOnlyNoteViews = noteViews.AsReadOnly());

        public bool IsNodeSelected => ScriptNodeViews.Count > 0 || GroupNodeViews.Count > 0 || NoteViews.Count > 0;
#else
        public bool IsNodeSelected => ScriptNodeViews.Count > 0 || GroupNodeViews.Count > 0;
#endif
        
        public int CountSelected
        {
            get
            { 
                var count = 0;
                if (scriptNodeViews != null) count += scriptNodeViews.Count;
                if (groupNodeViews != null) count += groupNodeViews.Count;
#if UNITY_2020_1_OR_NEWER
                if (noteViews != null) count += noteViews.Count;
#endif
                return count;
            }
        }

        public ScriptGraphViewSelection(ScriptGraphView graphView)
        {
            this.graphView = graphView;
        }
        
        public void Update()
        {
            scriptNodeViews.Clear();
            groupNodeViews.Clear();
            edges.Clear();
            
#if UNITY_2020_1_OR_NEWER
            noteViews.Clear();
#endif
            
            SelectedItemInGroup = false;
            
            foreach (var selectable in graphView.selection)
            {
                if (selectable is Edge edge)
                {
                    edges.Add(edge);
                    continue;
                }

                if (selectable is GroupNodeView selectedGroupNode)
                {
                    groupNodeViews.Add(selectedGroupNode);
                    continue;
                }
                
#if UNITY_2020_1_OR_NEWER
                if (selectable is NoteView noteView)
                {
                    noteViews.Add(noteView);
                    continue;
                }
#endif
                
                if (!(selectable is ScriptNodeView selectedNodeView)) continue;

                scriptNodeViews.Add(selectedNodeView);

                var referenceNode = selectedNodeView.userData as ReferenceNode;
                
                if (!graphView.Graph.GroupNodes.Any(groupNode => groupNode.Contains(selectedNodeView.Node) || referenceNode != null && groupNode.Contains(referenceNode))) continue;
                
                if (!SelectedItemInGroup)
                {
                    SelectedItemInGroup = true;
                }
            }
        }

        public void SelectGroup(GroupNodeView selectedGroupNodeView)
        {
            graphView.ClearSelection();
            graphView.AddToSelection(selectedGroupNodeView);

            foreach (var element in selectedGroupNodeView.containedElements)
            {
                if (!(element is UnityNode selectedNodeView)) continue;

                var ports = selectedNodeView.Query<Port>().ToList();

                foreach (var port in ports)
                {
                    foreach (var connection in port.connections)
                    {
                        graphView.AddToSelection(connection);
                    }
                }
                            
                graphView.AddToSelection(element);
            }

            Update();
        }
    }
}