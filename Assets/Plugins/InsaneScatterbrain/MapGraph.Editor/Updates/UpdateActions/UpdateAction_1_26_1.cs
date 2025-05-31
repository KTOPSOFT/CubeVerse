using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Editor.Updates;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class UpdateAction_1_26_1 : UpdateAction
    {
        private readonly Dictionary<(ScriptGraphGraph, string), string> newIdsByGraphAndOldIds = new Dictionary<(ScriptGraphGraph, string), string>();
        private readonly Dictionary<string, string> oldByNewIds = new Dictionary<string, string>();
        
        public override Version Version => new Version("1.26.1");
        
        private readonly Updater updater;
        
        public UpdateAction_1_26_1(Updater updater)
        {
            this.updater = updater;
        }
        public override void UpdateScene()
        {
            // If the input parameter ID is still using an old deduplicated parameter ID, update it to the new deduplicated parameter ID.
            var inputs = Resources.FindObjectsOfTypeAll<ScriptGraphInput>();
            foreach (var input in inputs)
            {
                if (input.Runner == null || input.Runner.Graph == null || input.Runner.Graph.InputParameters == null)
                    continue;   // This input isn't connected to a graph, skip.

                var key = (input.Runner.Graph, input.ParameterId);
                if (!newIdsByGraphAndOldIds.ContainsKey(key))
                    continue;   // This ID hasn't been deduplicated, skip.
                
                input.ParameterId = newIdsByGraphAndOldIds[key];

                Save(input);
            }

            // Go through all the runner databags to move the values from the old input parameter ID to the new input parameter ID.
            var newIds = oldByNewIds.Keys;
            var runners = Resources.FindObjectsOfTypeAll<ScriptGraphRunner>();
            foreach (var runner in runners)
            {
                if (runner.Graph == null || runner.Graph.InputParameters == null)
                    continue;   // This runner isn't connected to a graph, skip.

                var inputParameters = runner.Graph.InputParameters;
                foreach (var newId in newIds)
                {
                    if (!inputParameters.ContainsId(newId))
                        continue;   // Doesn't contain the new ID, skip.

                    var oldId = oldByNewIds[newId];
                    
                    if (!runner.ParamsInData.Contains(oldId))
                        continue;   // Doesn't contain the old ID, skip.

                    var value = runner.ParamsInData.Get(oldId);
                    runner.ParamsInData.Set(newId, value);
                    runner.ParamsInData.Remove(oldId);
                }
                
                Save(runner);
            }
        }

        public override void UpdateAssets()
        {
            var graphs = Assets.Find<ScriptGraphGraph>();

            var totalNodes = graphs.SelectMany(g => g.Nodes).Count();
            var totalInputParameters = graphs.SelectMany(g => g.InputParameters.OrderedIds).Count();
            var totalOutputParameters = graphs.SelectMany(g => g.OutputParameters.OrderedIds).Count();
            
            var totalSteps = totalNodes + totalInputParameters + totalOutputParameters;
            
            var step = 1f / totalSteps;
            var totalProgress = 0f;

            var processedNodeIds = new HashSet<string>();
            var processedInputParameterIds = new HashSet<string>();
            var processedOutputParameterIds = new HashSet<string>();

            var deduplicator = new IdDeduplicator();

            foreach (var graph in graphs)
            {
                // Call the ID to trigger the creation of an ID for the graph that will actually be persisted.
                var id = graph.Id;
                
                foreach (var node in graph.Nodes)
                {
                    totalProgress += step;
                    updater.SetActionProgress(totalProgress);
                    
                    if (processedNodeIds.Add(node.Id))
                        continue;   // Node ID is unique, so far. Move on to the next node.
                    
                    // Node ID is not unique. Generate a new ID.
                    node.Id = Guid.NewGuid().ToString();
                    processedNodeIds.Add(node.Id);
                }
                
                foreach (var oldId in graph.InputParameters.OrderedIds.ToArray())
                {
                    totalProgress += step;
                    updater.SetActionProgress(totalProgress);
                    
                    if (processedInputParameterIds.Add(oldId))
                        continue;   // Input parameter ID is unique, so far. Move on to the next input parameter.
                    
                    var newId = deduplicator.DeduplicateInputParameter(graph, oldId);

                    newIdsByGraphAndOldIds.Add((graph, oldId), newId);
                    oldByNewIds.Add(newId, oldId);
                }
                
                foreach (var oldId in graph.OutputParameters.OrderedIds.ToArray())
                {
                    totalProgress += step;
                    updater.SetActionProgress(totalProgress);
                    
                    if (processedOutputParameterIds.Add(oldId))
                        continue;   // Output parameter ID is unique, so far. Move on to the next output parameter.
                    
                    deduplicator.DeduplicateOutputParameter(graph, oldId);
                }
                
                Save(graph);
            }
            
            updater.SetActionProgress(1f);
        }
    }
}