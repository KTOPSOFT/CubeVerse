using System.Collections.Generic;
using System.Threading.Tasks;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public static class ScriptGraphDebugger
    {
        public static void Initialize()
        {
            // Whenever checks fail, open the graph's window, show which nodes failed, but framing them and highlighting
            // them with an orange color.
            ScriptGraphRunner.OnChecksFailed += (runner, nodes) =>
            {
                var window = ScriptGraphViewWindow.GetOrCreate(runner.Graph);
                window.Focus();
                FrameAndHighlightNodes(window, nodes, true);
            };
            
            // Whenever processing a node fails (meaning throws an exception), open the graph's window, show which node
            // failed, frame it and highlight it with a red color.
            ScriptGraphProcessor.OnProcessFailed += (processor, node) =>
            {
                var window = ScriptGraphViewWindow.GetOrCreate(processor.Graph);
                window.Focus();
                FrameAndHighlightNode(window, node);
            };

            // Make sure any highlights are reset when the graph is processed again.
            ScriptGraphRunner.OnRun += runner =>
            {
                ClearHighlights(runner.Graph);
            };
            
            ScriptGraphProcessor.OnProcess += processor =>
            {
                ClearHighlights(processor.Graph);
            };
        }

        private static void ClearHighlights(ScriptGraphGraph graph)
        {
            var window = ScriptGraphViewWindow.Get(graph);

            if (window == null) return; // There's nothing to clear, the graph's not opened in a window currently.

            var graphView = window.GraphView;
            graphView.ClearHighlights();
        }

        private static async void FrameAndHighlightNode(ScriptGraphViewWindow window, IScriptNode node, bool warning = false)
        {
            await Task.Delay(100);    // Apparently we have to wait a bit for the view to be updated.

            var graphView = window.GraphView; 
            graphView.FrameNode(node);
            graphView.HighlightFailedNode(node, warning);
        }
        
        private static async void FrameAndHighlightNodes(ScriptGraphViewWindow window, IEnumerable<IScriptNode> frameNodes, bool warning = false)
        {
            await Task.Delay(100);    // Apparently we have to wait a bit for the view to be updated.

            var graphView = window.GraphView; 
            graphView.FrameNodes(frameNodes);
            graphView.HighlightFailedNodes(frameNodes, warning);
        }
    }
}