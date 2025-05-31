using System;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Editor.Updates;
using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class UpdateAction_1_15 : UpdateAction
    {
        public override Version Version => new Version("1.15");
        
        private readonly Updater updater;
        
        public UpdateAction_1_15(Updater updater)
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

            var totalNodes = graphs.SelectMany(g => g.Nodes).Count();
            var step = 1f / totalNodes;
            var totalProgress = 0f;

            foreach (var graph in graphs)
            {
                foreach (var node in graph.Nodes)
                {
                    // Connect all max. fill percentage ports to the min. fill percentage ports as well to create
                    // an effect that is most similar to how the single port worked before.
                    if (node is RandomRectsNode randomRectsNode)
                    {
                        var maxPort = randomRectsNode.GetInPort("Max. Fill Percentage");
                        
                        if (!maxPort.IsConnected) continue;
                        
                        randomRectsNode.OnLoadInputPorts();

                        var minPort = randomRectsNode.GetInPort("Min. Fill Percentage");

                        minPort.Connect(maxPort.ConnectedOut);
                    }
                    else if (node is RandomlyStampTilemapsNode randomlyStampTilemapsNode)
                    {
                        var maxPort = randomlyStampTilemapsNode.GetInPort("Max. Fill Percentage");
                        
                        if (!maxPort.IsConnected) continue;
                        
                        randomlyStampTilemapsNode.OnLoadInputPorts();

                        var minPort = randomlyStampTilemapsNode.GetInPort("Min. Fill Percentage");
                        
                        minPort.Connect(maxPort.ConnectedOut);
                    }
                    
                    totalProgress += step;
                    
                    updater.SetActionProgress(totalProgress);
                }
                
                Save(graph);
            }
            
            updater.SetActionProgress(1f);
        }
    }
}