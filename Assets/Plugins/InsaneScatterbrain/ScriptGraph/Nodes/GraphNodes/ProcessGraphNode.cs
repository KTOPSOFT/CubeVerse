using System;
using System.Collections.Generic;
using InsaneScatterbrain.Services;
using InsaneScatterbrain.Threading;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Processes another graph inside this graph.
    /// 
    /// Input parameters of the selected graph will be assignable through in ports.
    /// Output parameters will be accessible through out ports. 
    /// </summary>
    [MovedFrom(false, "InsaneScatterbrain.MapGraph", "InsaneScatterbrain.MapGraph")]
    [Serializable]
    public class ProcessGraphNode : ProcessorNode, ISerializationCallbackReceiver
    {
        [SerializeReference] private ScriptGraphGraph subGraph;
        [SerializeField] private bool isNamed;

        private ScriptGraphProcessor processor;
        private ScriptGraphProcessor parentProcessor;

        public bool IsNamed
        {
            get => isNamed;
            set => isNamed = value;
        }

        public ScriptGraphGraph SubGraph
        {
            get => subGraph;
            set
            {
                UnregisterSubGraph();
                subGraph = value;
                RegisterSubGraph();
            }
        }

        public override void OnLoadInputPorts()
        {
            if (subGraph == null) return;

            // Add an input port for each input parameter
            var inputParameters = subGraph.InputParameters;
            foreach (var id in inputParameters.OrderedIds)
            {
                var name = inputParameters.GetName(id);
                var type = inputParameters.GetType(id);

                AddIn(name, type);
            }

            // Remove input ports for any parameters that no longer exist.
            var inactivePortNames = new List<string>();
            foreach (var inPort in InPorts)
            {
                if (!inputParameters.ContainsName(inPort.Name))
                {
                    inactivePortNames.Add(inPort.Name);
                }
            }

            foreach (var inactivePortName in inactivePortNames)
            {
                RemoveIn(inactivePortName);
            }
        }

        public override void OnLoadOutputPorts()
        {
            if (subGraph == null) return;
            
            var outputParameters = subGraph.OutputParameters;
            foreach (var id in outputParameters.OrderedIds)
            {
                var name = outputParameters.GetName(id); 
                var type = outputParameters.GetType(id);

                AddOut(name, type);
            }
            
            // Remove input ports for any parameters that no longer exist.
            var inactivePortNames = new List<string>();
            foreach (var outPort in OutPorts)
            {
                if (!outputParameters.ContainsName(outPort.Name))
                {
                    inactivePortNames.Add(outPort.Name);
                }
            }

            foreach (var inactivePortName in inactivePortNames)
            {
                RemoveOut(inactivePortName);
            }

            // Update the required status of the input ports.
            foreach (var inPort in InPorts)
            {
                var id = subGraph.InputParameters.GetId(inPort.Name);
                // If the in port is connected to a required port inside of the subgraph, treat this node as required as well.
                foreach (var inputNode in subGraph.InputNodes)
                {
                    if (id != inputNode.InputParameterId) continue;

                    foreach (var connectedInPort in inputNode.OutPort.ConnectedInPorts)
                    {
                        if (!connectedInPort.IsConnectionRequired) continue;

                        inPort.IsConnectionRequired = true;
                        break;
                    }

                    if (inPort.IsConnectionRequired) break;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            
            parentProcessor = Get<ScriptGraphProcessor>();
            
            var disposer = Get<Disposer>();
            var instanceProvider = Get<IInstanceProvider>();
            var scriptGraphInstanceProvider = Get<IScriptGraphInstanceProvider>();
            var rng = Get<Rng>();
            
            if (processor == null)
            {
                processor = new ScriptGraphProcessor();
            }
            
            processor.RngStartState = rng.State();

            // Use the main graph's disposer and don't let the sub graph dispose, so that all the subgraph's stuff
            // gets disposed together with the main graph's stuff.
            processor.Disposer = disposer;
            processor.AutoDispose = false;
            
            processor.InstanceProvider = instanceProvider;
            processor.ScriptGraphInstanceProvider = scriptGraphInstanceProvider;
            processor.IsMultiThreadingEnabled = parentProcessor.IsMultiThreadingEnabled;
            
            processor.ComponentDependencies = parentProcessor.ComponentDependencies;
            
            foreach (var inPort in InPorts)
            {
                var id = subGraph.InputParameters.GetId(inPort.Name);

                if (inPort.IsConnected)
                {
                    // If it's connected, assign it's value to the sub graph's corresponding input parameter.
                    processor.In(id, () => inPort.Get());
                }
                else
                {
                    // Flag this port as unconnected, so even though, the underlying input nodes in the sub graph are
                    // connected, they are treated as unconnected.
                    processor.FlagInUnconnected(id); 
                }
            }
        }

        protected override void OnProcess()
        {
            var result = processor.Process(subGraph, parentProcessor.CustomDependencies);

            if (processor.IsAborted && !MainThread.IsCurrent)
            {
                throw new Exception("Graph processor was aborted. Please check the error log.");
            }

            foreach (var outPort in OutPorts)
            {
                outPort.Set(() => result[outPort.Name]);
            }
        }
        
        private void RegisterSubGraph()
        { 
            if (subGraph == null) return;
            
            var inputParams = subGraph.InputParameters;
            inputParams.OnAdded += OnInputParameterAdded;
            inputParams.OnRemoved += OnInputParameterRemoved;
            inputParams.OnRenamed += OnInputParameterRenamed;
            inputParams.OnMoved += OnInputParameterMoved;
            
            var outParams = subGraph.OutputParameters;
            outParams.OnAdded += OnOutputParameterAdded;
            outParams.OnRemoved += OnOutputParameterRemoved;
            outParams.OnRenamed += OnOutputParameterRenamed;
            outParams.OnMoved += OnOutputParameterMoved;
        }

        private void UnregisterSubGraph()
        {
            ClearPorts();
            
            if (subGraph == null) return; 
            
            var inputParams = subGraph.InputParameters;
            inputParams.OnAdded -= OnInputParameterAdded;
            inputParams.OnRemoved -= OnInputParameterRemoved;
            inputParams.OnRenamed -= OnInputParameterRenamed;
            inputParams.OnMoved -= OnInputParameterMoved;
            
            var outParams = subGraph.OutputParameters;
            outParams.OnAdded -= OnOutputParameterAdded;
            outParams.OnRemoved -= OnOutputParameterRemoved;
            outParams.OnRenamed -= OnOutputParameterRenamed;
            outParams.OnMoved -= OnOutputParameterMoved;
        }

        private void OnInputParameterAdded(string id)
        {
            var inParams = subGraph.InputParameters;
            var paramName = inParams.GetName(id);
            var paramType = inParams.GetType(id);
            AddIn(paramName, paramType);
        }

        private void OnInputParameterRemoved(string id, string name) => RemoveIn(name);
        private void OnInputParameterRenamed(string id, string oldName, string newName) => RenameIn(oldName, newName);
        private void OnInputParameterMoved(int oldIndex, int newIndex) => MoveIn(oldIndex, newIndex);
        
        private void OnOutputParameterAdded(string id)
        {
            var outParams = subGraph.OutputParameters;
            var paramName = outParams.GetName(id);
            var paramType = outParams.GetType(id);
            AddOut(paramName, paramType);
        }
        
        private void OnOutputParameterRemoved(string id, string name) => RemoveOut(name);
        private void OnOutputParameterRenamed(string id, string oldName, string newName) => RenameOut(oldName, newName);
        private void OnOutputParameterMoved(int oldIndex, int newIndex) => MoveOut(oldIndex, newIndex);

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            RegisterSubGraph();
        }
    }
}