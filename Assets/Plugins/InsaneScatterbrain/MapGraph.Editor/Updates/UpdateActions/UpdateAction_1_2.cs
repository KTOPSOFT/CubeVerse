using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Editor.Updates;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Serialization;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{ 
    /// <summary>
    /// Update action for Map Graph v1.2
    /// </summary>
    public class UpdateAction_1_2 : UpdateAction
    {
        public override Version Version => new Version("1.2");
        
        private readonly Updater updater;
        
        public UpdateAction_1_2(Updater updater)
        {
            this.updater = updater;
        }

        public override void UpdateAssets()
        {
            updater.SetActionProgress(.1f);
            UpdateTilesets();
            updater.SetActionProgress(.2f);
            UpdatePrefabSets();
            updater.SetActionProgress(.3f);
            UpdateTilemapSets();
            updater.SetActionProgress(.4f);
            UpdateNamedColorSets();
            updater.SetActionProgress(.5f);
            UpdateGraphs();
            updater.SetActionProgress(.6f);
        }
        
        public override void UpdateScene()
        {
            UpdateGraphRunners();
            updater.SetActionProgress(.8f);
            UpdateGraphInputComponents();
            updater.SetActionProgress(1f);
        }
        
        private void UpdateTilesets() 
        {
            var setField = GetPrivateField<Tileset>("openTileset");
            var orderedIdsField = GetPrivateField<DataSet<TileType>>("orderedIds");
            var tileTypesField = GetPrivateField<Tileset>("tileTypes");
            var elementsField = GetPrivateField<DataSet<TileType>>("elements");
            var tileTypeIdField = GetPrivateField<DataSetItem>("id");
            
            var tilesets = Assets.Find<Tileset>();

            foreach (var tileset in tilesets)
            {
                if (tileset.Version >= Version) continue; // Already up-to-date.

                var set = (DataSet<TileType>) setField.GetValue(tileset);
                
                var orderedIds = (List<string>) orderedIdsField.GetValue(set);
                var tileTypes = (List<TileType>) tileTypesField.GetValue(tileset);

                if (orderedIds.Count > 0 || tileTypes.Count == 0)
                { 
                    Save(tileset);
                    continue; // Nothing to update, just change the version
                }

                var elements = (List<TileType>) elementsField.GetValue(set);

                foreach (var tileType in tileTypes)
                {
                    if (tileType.Id == null)
                    {
                        tileTypeIdField.SetValue(tileType, Guid.NewGuid().ToString());
                    }
                    orderedIds.Add(tileType.Id);
                    elements.Add(tileType);
                }
                
                tileTypesField.SetValue(tileset, null);

                Save(tileset);
            }
        }
        
        private void UpdatePrefabSets()
        {
            var setField = GetPrivateField<PrefabSet>("openPrefabSet"); 
            var orderedIdsField = GetPrivateField<DataSet<PrefabType>>("orderedIds");
            var prefabTypesField = GetPrivateField<PrefabSet>("prefabTypes");
            var prefabTypeIdField = GetPrivateField<DataSetItem>("id");
            var elementsField = GetPrivateField<DataSet<PrefabType>>("elements");
            
            var prefabSets = Assets.Find<PrefabSet>();

            foreach (var prefabSet in prefabSets)
            {
                if (prefabSet.Version >= Version) continue; // Already up-to-date.
                
                var set = (DataSet<PrefabType>) setField.GetValue(prefabSet);
                
                var orderedIds = (List<string>) orderedIdsField.GetValue(set);
                var prefabTypes = (List<PrefabType>) prefabTypesField.GetValue(prefabSet);
                
                if (orderedIds.Count > 0 || prefabTypes.Count == 0)
                {
                    Save(prefabSet);
                    continue; // Nothing to update, just change the version
                }
                
                var elements = (List<PrefabType>) elementsField.GetValue(set);

                foreach (var prefabType in prefabTypes)
                {
                    if (prefabType.Id == null)
                    {
                        prefabTypeIdField.SetValue(prefabType, Guid.NewGuid().ToString());
                    }
                    orderedIds.Add(prefabType.Id);
                    elements.Add(prefabType);
                }
                
                prefabTypesField.SetValue(prefabSet, null);
                
                Save(prefabSet);
            }
        }
        
        private void UpdateTilemapSets()
        {
            var setField = GetPrivateField<TilemapSet>("openTilemapSet");
            var orderedIdsField = GetPrivateField<DataSet<TilemapType>>("orderedIds");
            var tilemapTypesField = GetPrivateField<TilemapSet>("tilemapTypes");
            var tilemapTypeIdField = GetPrivateField<DataSetItem>("id");
            var elementsField = GetPrivateField<DataSet<TilemapType>>("elements");
            
            var tilemapSets = Assets.Find<TilemapSet>();

            foreach (var tilemapSet in tilemapSets)
            {
                if (tilemapSet.Version >= Version) continue; // Already up-to-date.
                
                var set = (DataSet<TilemapType>) setField.GetValue(tilemapSet);
                
                var orderedIds = (List<string>) orderedIdsField.GetValue(set);
                var tilemapTypes = (List<TilemapType>) tilemapTypesField.GetValue(tilemapSet);

                if (orderedIds.Count > 0 || tilemapTypes.Count == 0)
                {
                    Save(tilemapSet);
                    continue; // Nothing to update, just change the version
                }
                
                var elements = (List<TilemapType>) elementsField.GetValue(set);

                foreach (var tilemapType in tilemapTypes)
                {
                    if (tilemapType.Id == null)
                    {
                        tilemapTypeIdField.SetValue(tilemapType, Guid.NewGuid().ToString());
                    }
                    orderedIds.Add(tilemapType.Id);
                    elements.Add(tilemapType);
                }
                
                tilemapTypesField.SetValue(tilemapSet, null);

                Save(tilemapSet);
            }
        }

        private void UpdateNamedColorSets()
        {
            var serializedNamesField = GetPrivateField<NamedColorSet>("serializedNames");
            var serializedColorsField = GetPrivateField<NamedColorSet>("serializedColors");

            var namedColorSets = Assets.Find<NamedColorSet>();

            foreach (var namedColorSet in namedColorSets)
            {
                if (namedColorSet.Version >= Version) continue; // Already up-to-date.
                
                var serializedNames = (string[]) serializedNamesField.GetValue(namedColorSet);
                
                if (serializedNames.Length == 0)
                {
                    Save(namedColorSet);
                    continue; // Nothing to update, just change the version
                }

                var serializedColors = (Color32[]) serializedColorsField.GetValue(namedColorSet);

                for (var i = 0; i < serializedNames.Length; ++i)
                {
                    namedColorSet.Add(new NamedColor(serializedNames[i], serializedColors[i]));
                }
                
                serializedNamesField.SetValue(namedColorSet, null);
                serializedColorsField.SetValue(namedColorSet, null);
                
                Save(namedColorSet);
            }
        }

        private void UpdateGraphs()
        {
            var graphs = Assets.Find<MapGraphGraph>();

            foreach (var graph in graphs)
            {
                if (graph.Version >= Version) continue;

                UpdateGraphNamedColorNodes(graph);
                UpdateGraphParameters(graph);
                UpdateGraphInputNodes(graph);
                UpdateGraphOutputNodes(graph);
                
                Save(graph);
            }
        }
        
        private void UpdateGraphParameters(MapGraphGraph graph)
        {
            UpdateScriptGraphParameters(graph.InputParameters);
            UpdateScriptGraphParameters(graph.OutputParameters);
        }
        
        private void UpdateGraphNamedColorNodes(MapGraphGraph graph)
        {
            foreach (var node in graph.Nodes)
            {
                if (node.GetType() != typeof(NamedColorNode)) continue;

                var namedColorNode = (NamedColorNode) node;
                var namedColorName = namedColorNode.NamedColorId;
                
                if (string.IsNullOrEmpty(namedColorName)) continue;    // No value selected.

                // This isn't a GUID yet, meaning it's still a name. We're using ID now, because that's immutable. So find the
                // proper ID and store that instead.
                if (namedColorName == "[None]")
                {
                    // Not set to a value, set to null.
                    namedColorNode.NamedColorId = null;
                    continue;
                }
                
                namedColorNode.NamedColorId = graph.NamedColorSet.GetId(namedColorName);
            }
        }
        
        private void UpdateScriptGraphParameters(ScriptGraphParameters parameters)
        {
            var namesField = GetPrivateField<ScriptGraphParameters>("names");
            var names = (string[]) namesField.GetValue(parameters);

            if (names.Length == 0) return;
                
            var typeNamesField = GetPrivateField<ScriptGraphParameters>("typeNames");
            var typeNames = (string[]) typeNamesField.GetValue(parameters);

            for (var i = 0; i < names.Length; ++i)
            {
                var name = names[i];
                var type = Type.GetType(typeNames[i]);
                    
                parameters.Add(new ScriptGraphParameter(name, type));
            }

            namesField.SetValue(parameters, null);
            typeNamesField.SetValue(parameters, null);
        }
        
        private void UpdateGraphInputNodes(MapGraphGraph graph)
        {
            foreach (var inputNode in graph.InputNodes)
            {
                var inputParameterName = inputNode.InputParameterId;

                inputNode.InputParameterId = graph.InputParameters.GetId(inputParameterName);
            }
        }
        
        private void UpdateGraphOutputNodes(MapGraphGraph graph)
        {
            foreach (var outputNode in graph.OutputNodes)
            {
                var outputParameterId = outputNode.OutputParameterId;

                outputNode.OutputParameterId = graph.OutputParameters.GetId(outputParameterId);
            }
        }

        private void UpdateGraphRunners()
        {
            var graphRunners = Resources.FindObjectsOfTypeAll<ScriptGraphRunner>();
                
            foreach (var graphRunner in graphRunners)
            {
                if (graphRunner.Version >= Version) continue; // Already up-to-date.
                
                var graphProcessor = graphRunner.GraphProcessor;

                if (graphProcessor.Graph == null)
                {
                    // No graph selected, nothing to update except the version number.
                    Save(graphRunner);
                    continue;
                }

                var inputParameters = graphProcessor.Graph.InputParameters;
                
                var paramsInField = GetPrivateField<ScriptGraphRunner>("paramsIn");
                var paramsIn = (DataBag) paramsInField.GetValue(graphRunner);
                
                var names = paramsIn.Names.ToList();
                
                foreach (var paramInName in names)
                {
                    // Otherwise, convert to IDs now.
                    if (!inputParameters.ContainsName(paramInName)) continue;    // Unknown parameter, skip it, will be removed later.
                
                    var id = inputParameters.GetId(paramInName);
                
                    var value = paramsIn.Get(paramInName);
                    paramsIn.Remove(paramInName);
                    paramsIn.Set(id, value);
                }
                
                Save(graphRunner);
            }
        }
        
        private void UpdateGraphInputComponents()
        {
            var inputComponents = Resources.FindObjectsOfTypeAll<ScriptGraphInput>();

            foreach (var inputComponent in inputComponents)
            {
                if (inputComponent.Version >= Version) continue; // Already up-to-date.
                
                var parameterIdField = GetPrivateField<ScriptGraphInput>("parameterId");
                var parameterName = (string) parameterIdField.GetValue(inputComponent);

                if (string.IsNullOrEmpty(parameterName)) 
                {
                    // No value selected, nothing to update except for the version number.
                    Save(inputComponent);
                    continue;    
                }

                var graphRunnerField = GetPrivateField<ScriptGraphInput>("runner");
                var graphRunner = graphRunnerField.GetValue(inputComponent) as ScriptGraphRunner;
                
                if (graphRunner == null) continue;
                
                var parameterId = graphRunner.GraphProcessor.Graph.InputParameters.GetId(parameterName);

                parameterIdField.SetValue(inputComponent, parameterId);
                
                Save(inputComponent);
            }
        }
    }
}