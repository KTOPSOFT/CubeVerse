using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// The editor window to display the script graph view in.
    /// </summary>
    public class ScriptGraphViewWindow : EditorWindow, IHasCustomMenu
    {
        private ScriptGraphSearchBar searchBar;
        
        public ScriptGraphView GraphView { get; private set; }
        public ScriptGraphGraph GraphInstance { get; set; }

        [SerializeField] private ScriptGraphGraph graph;
        [SerializeField] private Vector2 graphViewPosition;
        [SerializeField] private Vector3 graphViewScale = Vector3.one;
        [SerializeField] private bool showDebugInfo;

        private static readonly Dictionary<ScriptGraphGraph, ScriptGraphViewWindow> instances = new Dictionary<ScriptGraphGraph, ScriptGraphViewWindow>();
        private static bool initialized;

        public static IEnumerable<ScriptGraphViewWindow> Instances => instances?.Values;
        
        public static ScriptGraphViewWindow Get(ScriptGraphGraph graph) => instances.ContainsKey(graph) ? instances[graph] : null;

        public static bool Contains(ScriptGraphGraph graph) => instances.ContainsKey(graph);

        private string Title { get; set; }

        public static ScriptGraphViewWindow CreateGraphViewWindow(ScriptGraphGraph graph)
        {
            ScriptGraphViewWindow window; 
            if (instances != null && instances.TryGetValue(graph, out var instance))
            {
                window = instance;
            }
            else
            {
                window = CreateWindow<ScriptGraphViewWindow>(typeof(ScriptGraphViewWindow));
                window.graph = graph;
                SetTitle(window);
            }

            window.Focus();
            window.Load(graph); 

            return window;
        }

        private static void InitializeGlobal()
        {
            if (initialized) return;
            
            var existingWindows = (ScriptGraphViewWindow[]) Resources.FindObjectsOfTypeAll(typeof(ScriptGraphViewWindow));
            foreach (var existingWindow in existingWindows)
            {
                if (existingWindow.graph == null) continue;
                    
                if (instances.ContainsKey(existingWindow.graph)) continue;
                
                instances.Add(existingWindow.graph, existingWindow);
            }

            initialized = true;
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += UpdateTitle;
            
            InitializeGlobal();

            if (graph != null)
            {
                // If a graph was already opened in the window make sure it's opened again, at the position the user
                // left off.
                Load(graph);
                GraphView.UpdateViewTransform(graphViewPosition, graphViewScale);
            }
            else
            {
                // Otherwise the script graph view hasn't been initialized yet, do so now.
                NewGraphView();
                AddElements();
            }
        }

        private void AddElements()
        {
            rootVisualElement.Add(GraphView);
            searchBar = new ScriptGraphSearchBar(GraphView);
            rootVisualElement.Add(searchBar);
        }
        
        private void NewGraphView()
        {
            GraphView = new ScriptGraphView(Title, showDebugInfo);
            GraphView.OnShowDebugInfoChanged += val => showDebugInfo = val;    // Store that here, so that it persists.
            GraphView.Initialize();
            GraphView.style.flexGrow = 1;
            GraphView.viewTransformChanged += graphView => 
            {
                // Store position and zoom information, so we can restore it after a domain reload.
                graphViewScale = graphView.viewTransform.scale;
                graphViewPosition = GraphView.viewTransform.position;
            };
            
            GraphView.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    searchBar.Hide();
                }
                
                if (evt.keyCode == KeyCode.F && evt.modifiers == EventModifiers.Control)
                {
                    searchBar.Show();
                }
                
                if (evt.keyCode == KeyCode.F5)
                {
                    Reload();
                }
            });
        }
    
        private void OnDisable()
        {
            EditorApplication.projectChanged -= UpdateTitle;
            
            if (graph != null) instances.Remove(graph); 
            
            if (GraphView == null) return;
            
            GraphView.RemoveFromHierarchy();
            GraphView.Dispose();
        }
        
        private static void SetTitle(ScriptGraphViewWindow window)
        {
            if (window.graph == null) return;
            
            window.Title = string.IsNullOrEmpty(window.graph.name) ? "[Untitled Graph]" : window.graph.name;
            window.titleContent = new GUIContent(window.Title);
        }

        private void UpdateTitle()
        {
            SetTitle(this);
        }

        /// <summary>
        /// Load the given graph into the script graph view window.
        /// </summary>
        /// <param name="loadGraph">The graph to load.</param>
        public void Load(ScriptGraphGraph loadGraph)
        {
            // Validate and repair (if necessary) the graph before opening it. It might have been corrupted through
            // external factors, such as certain node classes having been renamed or removed, but the connections
            // to those nodes might still exist, which will cause the graph to fail to load.
            var validator = new ScriptGraphValidator();
            validator.ValidateAndRepair(loadGraph);
            
            if (GraphView != null)
            {
                rootVisualElement.Remove(GraphView);
                rootVisualElement.Remove(searchBar);
                GraphView.Dispose();
            }
            
            NewGraphView();
            LoadGraph();
            AddElements();

            graph = loadGraph; 

            instances[graph] = this;
        }
        
        private void LoadGraph()
        {
            try
            {
                GraphView?.Load(graph);
            }
            catch
            {
                Debug.LogError("Cannot load graph view window. See error log for details.", graph);
                Close();
                throw;
            }
        }

        /// <summary>
        /// Reload the currently assigned graph into the graph view window.
        /// </summary>
        public void Reload()
        {
            LoadGraph();
        }

        public static void ReloadAll()
        {
            if (Instances == null) return;

            foreach (var instance in Instances)
            {
                if (instance == null) continue;
                
                // If graph editor windows are open, reload them, so the updated data is loaded.
                instance.Reload();
            }
        }

        private void OnGUI()
        {
            if (graph == null)
            {
                Close();
            }
            
            searchBar.OnGUI();
            
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                GraphView.TriggerKeyDown(e.keyCode, e.modifiers, e.mousePosition);
            }
        }
        
        /// <inheritdoc cref="IHasCustomMenu.AddItemsToMenu"/>
        public void AddItemsToMenu(GenericMenu menu)
        {
            var reloadWindowContent = new GUIContent("Reload Window _F5");
            menu.AddItem(reloadWindowContent, false, () =>
            {
                Get(graph)?.Reload();
            });
            
            menu.AddSeparator(string.Empty);
            
            var searchContent = new GUIContent("Search In Graph %f");
            var searchEnabled = searchBar.style.display != DisplayStyle.None;
            menu.AddItem(searchContent, searchEnabled, searchBar.Toggle);
            
            var inspectGraphContent = new GUIContent("Inspect Graph");
            menu.AddItem(inspectGraphContent, false, () =>
            {
                Selection.activeObject = graph;
            });
            
            menu.AddSeparator(string.Empty);
            
            var debugInfoContent = new GUIContent("Debug Info");
            menu.AddItem(debugInfoContent, GraphView.ShowDebugInfo, () =>
            {
                GraphView.ShowDebugInfo = !GraphView.ShowDebugInfo;
                Reload();
            });
            
            var clearErrors = new GUIContent("Clear Error Highlights");
            menu.AddItem(clearErrors, false, () =>
            {
                GraphView.ClearHighlights();
            });
        }

        /// <summary>
        /// Gets the window for the given graph, if it doesn't exist, create it now.
        /// </summary>
        /// <returns></returns>
        public static ScriptGraphViewWindow GetOrCreate(ScriptGraphGraph graph)
        {
            var window = Get(graph);
            
            if (window != null) return window;
            
            window = CreateGraphViewWindow(graph);
            return window;
        }
    }
}
