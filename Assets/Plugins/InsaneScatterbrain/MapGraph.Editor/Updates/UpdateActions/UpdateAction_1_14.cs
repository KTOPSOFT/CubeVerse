using System;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Editor.Updates;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class UpdateAction_1_14 : UpdateAction
    {
        public override Version Version => new Version("1.14");
        
        private readonly Updater updater;
        
        public UpdateAction_1_14(Updater updater)
        {
            this.updater = updater;
        }
        public override void UpdateScene()
        {
            // Do nothing.
        }

        public override void UpdateAssets()
        {
            var idField = GetPrivateField<ScriptNode>("id"); 
            
            var graphs = Assets.Find<ScriptGraphGraph>();

            var totalNodes = graphs.SelectMany(g => g.Nodes).Count();
            var step = 1f / totalNodes;
            var totalProgress = 0f;

            foreach (var graph in graphs)
            {
                foreach (var node in graph.Nodes)
                {
                    if (!(node is ScriptNode)) continue;
                    
                    if (!string.IsNullOrEmpty(node.Id)) continue;

                    idField.SetValue(node, Guid.NewGuid().ToString());

                    totalProgress += step;
                    
                    updater.SetActionProgress(totalProgress);
                }
            }
            
            updater.SetActionProgress(1f);
        }
    }
}