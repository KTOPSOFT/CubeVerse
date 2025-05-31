using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using InsaneScatterbrain.Dependencies;
using InsaneScatterbrain.RandomNumberGeneration;
using InsaneScatterbrain.Services;
using InsaneScatterbrain.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Rng = InsaneScatterbrain.Services.Rng;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class processes the processor nodes in the correct order.
    /// </summary>
    [Serializable]
    public class ScriptGraphProcessor
    {
        private static IDependencyContainer _globalDependencies;
        private static IDependencyContainer GlobalDependencies => _globalDependencies = _globalDependencies ?? new DependencyContainer();
        
        public static void RegisterGlobalDependency<T>(Func<T> dependency) where T : class
        {
            GlobalDependencies.Register(dependency);
        }
        
        public static void RegisterGlobalDependency(Type type, Func<object> dependency)
        {
            GlobalDependencies.Register(type, dependency);
        }
        
        public static T Get<T>() where T : class
        {
            return GlobalDependencies.Get<T>();
        }
        
        private readonly Dictionary<string, Func<object>> inParams = new Dictionary<string, Func<object>>();

        private readonly HashSet<string> unconnectedInParams = new HashSet<string>();
        
        private readonly ExecutionGraphBuilder executionGraphBuilder = new ExecutionGraphBuilder();
        private readonly ExecutionGraph executionGraph = new ExecutionGraph();

        private readonly ConcurrentDictionary<IProcessorNode, bool> queuedNodes =
            new ConcurrentDictionary<IProcessorNode, bool>();
        
        private readonly ConcurrentDictionary<IProcessorNode, ConcurrentDictionary<IProcessorNode, bool>> nextNodesWaitingForCompletion =
            new ConcurrentDictionary<IProcessorNode, ConcurrentDictionary<IProcessorNode, bool>>();

        private readonly ConcurrentDictionary<IProcessorNode, bool> nodesBeingProcessed =
            new ConcurrentDictionary<IProcessorNode, bool>();
        
        private readonly Dictionary<IScriptNode, RngState> rngStates = new Dictionary<IScriptNode, RngState>();
        
        /// <summary>
        /// Global event that's called each time a script graph is about to be processed by any script graph processor.
        /// </summary>
        public static event Action<ScriptGraphProcessor> OnProcess;

#if UNITY_EDITOR
        /// <summary>
        /// Global event that's called each time processing a node fails.
        /// </summary>
        public static event Action<ScriptGraphProcessor, IScriptNode> OnProcessFailed;
#endif
        
        public event Action<int, int> OnProgress;

        private int numNodesProcessed;
        
        [SerializeField] private ScriptGraphGraph graph;
        
        [SerializeField] private List<ComponentDependency> componentDependencies = new List<ComponentDependency>();

        public List<ComponentDependency> ComponentDependencies
        {
            get => componentDependencies;
            set => componentDependencies = value;
        }
        
        /// <summary>
        /// Gets/sets the graph to process.
        /// </summary>
        public ScriptGraphGraph Graph
        {
            get => graph;
            set => graph = value;
        }

        [SerializeField] private SeedType seedType = SeedType.Int;
        [SerializeField] private int seed;
        [SerializeField] private string seedGuid;

        public SeedType SeedType
        {
            get => seedType;
            set => seedType = value;
        }
        
        /// <summary>
        /// Gets/sets the RNG seed.
        /// </summary>
        public int Seed
        {
            get => seed;
            set => seed = value;
        }
        
        public string SeedGuid
        {
            get => seedGuid;
            set => seedGuid = value;
        }

        [SerializeField] private bool isSeedRandom = true;
        
        /// <summary>
        /// Gets/sets whether a random RNG seed is used or the one set in seed property.
        /// </summary>
        public bool IsSeedRandom
        {
            get => isSeedRandom;
            set => isSeedRandom = value;
        }
        
#if UNITY_WEBGL && UNITY_EDITOR
        // Store the Application.IsPlaying value here, as it can only be accessed on the main thread.
        public bool IsPlaying { get; set; }
        
        private bool isMultiThreadingEnabled;
        public bool IsMultiThreadingEnabled
        {
            get
            {
                // If this set to a WebGL build and we're running in play mode in the editor. This should emulate the
                // standalone player runtime behaviour as much as possible. Meaning that multi-threading is not possible.
                if (IsPlaying) return false;
                
                // However, in the edit mode, we want to be able to run graphs on different threads, so we respect
                // the settings.
                return isMultiThreadingEnabled;
            }
            set => isMultiThreadingEnabled = value;
        }
#elif UNITY_WEBGL
        // WebGL doesn't support multi-threading and thus must always be disabled.
        public bool IsMultiThreadingEnabled
        {
            get { return false; }
            set {}
        }
#else 
        public bool IsMultiThreadingEnabled { get; set; }
#endif

        private Rng rng = new Rng();
        /// <summary>
        /// Gets the RNG.
        /// </summary>
        [Obsolete("Will be removed in version 2.0. Check the manual page about custom nodes to see how to access dependencies, such as the Rng object.")]
        public Rng Rng => rng;

        /// <summary>
        /// Gets the disposer.
        /// </summary>
        public Disposer Disposer { get; set; } = new Disposer();
        public IInstanceProvider InstanceProvider { get; set; }
        public IScriptGraphInstanceProvider ScriptGraphInstanceProvider { get; set; }

        public bool AutoDispose { get; set; } = true;

        [NonSerialized] private bool isProcessing;
        
        /// <summary>
        /// Gets whether the processor is currently processing a graph or not.
        /// </summary>
        public bool IsProcessing => isProcessing;

        public RngState? RngStartState { get; set; } = null;
        
#if UNITY_EDITOR
        private Stopwatch executionTimer;

        /// <inheritdoc cref="IProcessorNode.LatestExecutionTime"/>
        public long LatestExecutionTime => executionTimer?.ElapsedMilliseconds ?? -1;
#endif

        public bool IsAborted { get; private set; }
        
        public DependencyContainer CustomDependencies { get; private set; }
        
        private bool isCancelled;

        private static int numGraphsProcessing;
        
        /// <summary>
        /// Prepares the graph's nodes for processing by making sure the ports are loaded.
        /// </summary>
        private void InitializeGraph(DependencyContainer container)
        {
            foreach (var node in graphInstance.Nodes)
            {
                if (node is IConsumerNode consumerNode)
                {
                    consumerNode.OnLoadInputPorts();
                }

                if (node is IProviderNode providerNode)
                {
                    providerNode.OnLoadOutputPorts();
                }
                node.LoadDependencies(container);
                node.Initialize();
                rngStates.Add(node, rng.State());

                node.OnSaveDependenciesState += SaveNodeDependenciesState;
                node.OnRestoreDependenciesState += RestoreNodeDependenciesState;
            }
        }
        
        private void PrepareNextNodes(IProcessorNode node)
        {
            // Add next nodes.
            var nextNodes = executionGraph.Next(node);

            if (nextNodes == null) return;
            
            foreach (var nextNode in nextNodes)
            {
                if (nextNodesWaitingForCompletion.ContainsKey(nextNode)) continue;

                nextNodesWaitingForCompletion[nextNode] = InstanceProvider.Get<ConcurrentDictionary<IProcessorNode, bool>>();
                var nodesToCompleteBeforeProcessingNext = executionGraph.Previous(nextNode);

                foreach (var waitForNode in nodesToCompleteBeforeProcessingNext)
                {
                    nextNodesWaitingForCompletion[nextNode].TryAdd(waitForNode, true);
                }
            }
        }

        private void QueueWaitingNodes()
        {
            while (IsThreadReady())
            {
                if (!waitingNodes.TryDequeue(out var node)) break;
                StartProcessingNode(node);
            }
        }

        private void QueueReadyNextNodes(IProcessorNode node)
        {
            lock (nextNodesWaitingForCompletion)
            {
                var nextNodes = executionGraph.Next(node);
            
                if (nextNodes == null) return;
            
                // Check if next nodes are ready for processing.
                foreach (var nextNode in nextNodes)
                {
                    nextNodesWaitingForCompletion[nextNode].TryRemove(node, out _);

                    if (nextNodesWaitingForCompletion[nextNode].Count > 0) continue;

                    QueueNode(nextNode);

                    nextNodesWaitingForCompletion.TryRemove(nextNode, out _);
                }
            }
        }

        private void CleanUp()
        {
            isCancelled = false;
            while (waitingNodes.Count > 0) waitingNodes.TryDequeue(out _);
            graphInstance = null;
            nextNodesWaitingForCompletion.Clear();
            queuedNodes.Clear();
            nodesBeingProcessed.Clear();
            numNodesProcessing = 0;
            CustomDependencies = null;
            
            Interlocked.Decrement(ref numGraphsProcessing);

            if (numGraphsProcessing == 0)
            {
                // Dispose of all disposable objects and return all instances to their pools. But only do so
                // once all graphs have been processed, as at the moment there is no distinction between
                // which object is in use by which graph/processor.

                if (AutoDispose)
                {
                    Disposer.Dispose();
                    InstanceProvider?.ReturnAll();
                    ScriptGraphPoolManagerSingleton.Instance.ReturnAll();

#if UNITY_EDITOR
                    // Clear script graph pools in the editor, so we don't have any outdated instances if a graph is changed.
                    ScriptGraphPoolManagerSingleton.Instance.ClearAll();
#endif
                }
                
                // Destroy the temporary components. Needs to be done on the main thread, because it requires the Unity API.
                MainThread.Execute(ClearComponentsCommand, false);
            }

            isProcessing = false;

#if UNITY_EDITOR
            executionTimer.Stop();
#endif
         }
         
        private void ProcessNode(IProcessorNode node)
        {
            if (isCancelled)
            {
                return;
            }
            
            try
            {
                PrepareNextNodes(node);

                nodesBeingProcessed.TryAdd(node, true);

#if UNITY_EDITOR
                try
                {
                    // Process node.
                    node.Process();
                }
                catch (Exception)
                {
                    IsAborted = true;
                    MainThread.Execute(new MainThreadCommand(() => OnProcessFailed?.Invoke(this, node)));
                    throw;
                }
#else
                node.Process();
#endif
                numNodesProcessed++;
                var totalNumNodes = graphInstance.ProcessorNodes.Count;
                OnProgress?.Invoke(numNodesProcessed, totalNumNodes);

                QueueWaitingNodes();
                QueueReadyNextNodes(node);
            }
            finally
            {
                Interlocked.Decrement(ref numNodesProcessing);
            }
        }

        private int numNodesProcessing;

        private void StartProcessing()
        {
            numNodesProcessed = 0;
            foreach (var firstNode in executionGraph.FirstNodes)
            {
                QueueNode(firstNode);
            }
        }

        private void WaitForProcessingToFinish()
        {
            while (numNodesProcessing > 0)
            {
                Thread.Sleep(10);
            }
        }

        private ConcurrentQueue<IProcessorNode> waitingNodes = new ConcurrentQueue<IProcessorNode>();

        // Don't process more nodes at the same time than there are logical cores (-1 to account for the Unity's
        // main thread) on the system. It will only increase processing time, due to the additional overhead.
        private bool IsThreadReady() => !IsMultiThreadingEnabled || numNodesProcessing < Environment.ProcessorCount - 1;

        private void QueueNode(IProcessorNode node)
        {
            if (queuedNodes.ContainsKey(node)) return;

            queuedNodes.TryAdd(node, true);
            
            if (!IsThreadReady())
            {
                waitingNodes.Enqueue(node);
                return;
            }

            StartProcessingNode(node);
        }

        private void StartProcessingNode(IProcessorNode node)
        {
            Interlocked.Increment(ref numNodesProcessing);

            if (IsMultiThreadingEnabled)
            {
                ThreadPool.QueueUserWorkItem(data =>
                {
                    try
                    {
                        ProcessNode(node);
                    }
                    catch (Exception e)
                    {
                        IsAborted = true;
                        // Explicitly write the exception to log, because if an exception is thrown on a non-main thread
                        // this doesn't happen automatically.
                        Debug.LogException(e);
                        throw;
                    }
                });
            }
            else
            {
                ProcessNode(node);
            }
        }
        
        private ScriptGraphGraph graphInstance;

        private DependencyContainer nodeDependencies = new DependencyContainer();

        private MainThreadCommand getGraphInstanceCommand;
        private MainThreadCommand GetGraphInstanceCommand =>
            getGraphInstanceCommand ?? (getGraphInstanceCommand = new MainThreadCommand(GetGraphInstance));

        private void GetGraphInstance()
        {
            graphInstance = ScriptGraphInstanceProvider.Get(graph);
        }

        private MainThreadCommand clearComponentsCommand;
        private MainThreadCommand ClearComponentsCommand =>
            clearComponentsCommand ??
            (clearComponentsCommand = new MainThreadCommand(ScriptGraphComponents.Clear));
        
        private MainThreadCommand invokeOnProcessCommand;
        private MainThreadCommand InvokeOnProcessCommand =>
            invokeOnProcessCommand ??
            (invokeOnProcessCommand = new MainThreadCommand(() => OnProcess?.Invoke(this)));

        public bool IsMainThreadTimePerFrameLimitEnabled { get; set; }
        
        private void SaveNodeDependenciesState(IScriptNode node, IDependencyContainer dependencyContainer)
        {
            var random = dependencyContainer.Get<Rng>();
            rngStates[node] = random.GetState();
        } 
        
        private void RestoreNodeDependenciesState(IScriptNode node, IDependencyContainer dependencyContainer)
        {
            var random = dependencyContainer.Get<Rng>();
            random.SetState(rngStates[node]);
        } 

        /// <summary>
        /// Processes the given graph.
        /// </summary>
        /// <param name="graphToProcess">The graph to process.</param>
        /// <param name="customDependencies">
        /// Optional parameter to add optional dependencies that custom nodes may depend on.
        /// </param>
        /// <returns>The values for each output parameter.</returns>
        /// <exception cref="InvalidOperationException">
        /// InvalidOperationException is thrown if the graph is already processing.
        /// </exception>
        public Dictionary<string, object> Process(
            ScriptGraphGraph graphToProcess = null, 
            DependencyContainer customDependencies = null)
        {
            if (IsProcessing)
            {
                throw new InvalidOperationException("Graph processor is already processing.");
            }
            
            isProcessing = true;
            Interlocked.Increment(ref numGraphsProcessing);
            
            if (graphToProcess != null)
            {
                graph = graphToProcess;
            }
            
            Prepare();
            
#if UNITY_EDITOR
            if (executionTimer == null)
            {
                executionTimer = new Stopwatch();
            }
            
            executionTimer.Restart();
#endif

            IsAborted = false;

            while (waitingNodes.Count > 0) waitingNodes.TryDequeue(out _);
            nodesBeingProcessed.Clear();
            rngStates.Clear();
            
            try
            {
                queuedNodes.Clear();
                nextNodesWaitingForCompletion.Clear();

                if (InstanceProvider == null)
                {
                    InstanceProvider = new NewInstanceProvider();
                }

                // Sort the nodes so the processor nodes get processed in the right order.
                executionGraphBuilder.Build(graphInstance, executionGraph);

                if (RngStartState != null)
                {
                    rng.SetState(RngStartState.Value);
                }
                else if (isSeedRandom)
                {
                    rng.SetState(RngState.New());
                }
                else switch (seedType)
                {
                    case SeedType.Int:
                    {
                        rng.SetState(RngState.FromInt(seed));
                        break;
                    }
                    case SeedType.Guid:
                    {
                        if (!Guid.TryParse(seedGuid, out var guid))
                        {
                            throw new ArgumentException("Seed is not a valid GUID.");
                        }
                        rng.SetState(RngState.FromBytes(guid.ToByteArray()));
                        break;
                    }
                }

                nodeDependencies.Clear();
                nodeDependencies.Register(() => this);
                nodeDependencies.Register(() => InstanceProvider);
                nodeDependencies.Register(() => ScriptGraphInstanceProvider);
                nodeDependencies.Register(() => rng);
                nodeDependencies.Register(() => Disposer);
                
                graphInstance.RegisterDependencies(nodeDependencies);
                
                // We need to register the dependencies in the right order, so they are overridden correctly.
                
                // First we register the global dependencies.
                if (_globalDependencies != null)
                {
                    nodeDependencies.Register(_globalDependencies);
                }

                // Add the graph's dependencies.
                nodeDependencies.Register(graphInstance.DependencyContainer);

                // Secondly the dependencies that are set on the runner as a component.
                foreach (var dependency in componentDependencies)
                {
                    var gameObject = dependency.GameObject;
                    var component = dependency.Component;
                    
                    if (gameObject == null)
                    {
                        Debug.LogWarning("Component dependency object is null.");
                        continue;
                    }
                    
                    if (component == null)
                    {
                        Debug.LogWarning($"Component dependency for object \"{gameObject.name}\" is null.");
                        continue;
                    }

                    // Get the type that the component is registered under.
                    var type = dependency.Type;
                    nodeDependencies.Register(type, () => dependency.Component);
                }

                // Finally the dependencies passed in as a parameter.
                if (customDependencies != null)
                {
                    nodeDependencies.Register(customDependencies);
                }

                CustomDependencies = customDependencies;
                
                // Initialize the graph.
                InitializeGraph(nodeDependencies);

                // Bind the input parameters to their respective out ports.
                PrepareInputParams();

                MainThread.Execute(InvokeOnProcessCommand);

                StartProcessing();

                WaitForProcessingToFinish();

                // Get the values for the output parameters from the output nodes.
                var output = PrepareOutput();

                return output;
            }
            finally
            {
                CleanUp();
            }
        }

        /// <summary>
        /// Binds the input parameters to their respective out ports.
        /// </summary>
        private void PrepareInputParams()
        {
            foreach (var input in graphInstance.InputNodes)
            {
                if (unconnectedInParams.Contains(input.InputParameterId))
                {
                    input.OutPort.Set(null);
                    input.OutPort.IsFlaggedUnconnected = true;
                    continue;
                }
                
                if (!inParams.ContainsKey(input.InputParameterId))
                {
                    var inputParameterName = graphInstance.InputParameters.GetName(input.InputParameterId);
                    Debug.LogWarning($"No value bound to input: {inputParameterName}");
                    continue;
                }
                
                input.OutPort.Set(inParams[input.InputParameterId]);
                input.OutPort.IsFlaggedUnconnected = false;
            }
        }

        /// <summary>
        /// Get the values for the output parameters from the output nodes.
        /// </summary>
        /// <returns>The output parameter values.</returns>
        private Dictionary<string, object> PrepareOutput()
        {
            var outputValues = new Dictionary<string, object>();
            
            foreach (var output in graphInstance.OutputNodes)
            {
                foreach (var inPort in output.InPorts)
                {
                    var name = graphInstance.OutputParameters.GetName(output.OutputParameterId);
                    outputValues[name] = inPort.Get();
                }
            }

            return outputValues;
        }

        /// <inheritdoc cref="In"/>
        public void In<T>(string name, Func<T> value)
        {
            inParams[name] = () => value;
        }

        /// <summary>
        /// Set input providers for the given input parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">Input provider.</param>
        public void In(string name, Func<object> value)
        {
            inParams[name] = value;
            unconnectedInParams.Remove(name);
        }

        /// <summary>
        /// Flags the given in parameter as unconnected.
        /// </summary>
        /// <param name="id">The input parameters ID.</param>
        public void FlagInUnconnected(string id)
        {
            unconnectedInParams.Add(id);
        }
        
        public void SetGraph(ScriptGraphGraph setGraph)
        {
            graph = setGraph;
        }

        private void Prepare()
        {
            if (graphInstance != null)
            {
                return;
            }
            
            if (graph == null)
            {
                throw new InvalidOperationException("Graph is not set.");
            }
            
            if (ScriptGraphInstanceProvider == null)
            {
                ScriptGraphInstanceProvider = new NewScriptGraphInstanceProvider();
            }

            MainThread.Execute(GetGraphInstanceCommand);
        }

        public bool ContainsInParameterId(string id)
        {
            Prepare();
            
            return graphInstance.InputParameters.ContainsId(id);
        }

        public string GetInParameterId(string inName)
        {
            Prepare();
            
            var inputParameters = graphInstance.InputParameters;
            if (!inputParameters.ContainsName(inName))
            {
                Debug.LogError($"Cannot get input parameter \"{inName}\": not defined.");
                return default;
            }
            
            return inputParameters.GetId(inName);
        }

        public string GetInParameterName(string id)
        {
            Prepare();
            
            var inputParameters = graphInstance.InputParameters;
            if (!inputParameters.ContainsId(id))
            {
                Debug.LogError($"Cannot get input parameter \"{id}\": not defined.");
                return default;
            }
            
            return inputParameters.GetName(id);
        }

        public Type GetInParameterType(string id)
        {
            Prepare();
            
            var inputParameters = graphInstance.InputParameters;
            if (!inputParameters.ContainsId(id))
            {
                Debug.LogError($"Cannot get input parameter \"{id}\": not defined.");
                return default;
            }
            
            return inputParameters.GetType(id);
        }

        public IEnumerable<string> GetInParameterIds()
        {
            Prepare();

            return graphInstance.InputParameters.OrderedIds;
        }

        public IEnumerable<IConsumerNode> GetConsumerNodes()
        {
            Prepare();

            return graphInstance.ConsumerNodes;
        }

        public void Cancel()
        {
            isCancelled = true;
        }

        public void Unprepare()
        {
            graphInstance = null;
        }
    }
}