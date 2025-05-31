using System;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Editor.Updates;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{ 
    /// <summary>
    /// Update action for Map Graph v1.10
    /// </summary>
    public class UpdateAction_1_10 : UpdateAction
    {
        public override Version Version => new Version("1.10");
        
        private readonly Updater updater;
        
        public UpdateAction_1_10(Updater updater)
        {
            this.updater = updater;
        }

        public override void UpdateScene()
        {
            // Do nothing.
        }

        public override void UpdateAssets()
        {
            var graphs = Assets.Find<ScriptGraphGraph>();
            var progressStep = 1f / graphs.Count;

            for (var i = 0; i < graphs.Count; ++i)
            {
                updater.SetActionProgress(progressStep * i);
                
                var graph = graphs[i];
                
                if (graph.Version >= Version) continue; // Already up-to-date.
                
                graph.NodePath = "Graphs";
                Save(graph);
            }
            
            updater.SetActionProgress(1f);
        }
    }
}