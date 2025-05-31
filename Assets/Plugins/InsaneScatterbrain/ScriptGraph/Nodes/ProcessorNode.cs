using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using InsaneScatterbrain.Services;
using InsaneScatterbrain.Threading;
using UnityEngine;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Interface for any node that can be used to process something. It can either consume or provide data or both.
    /// </summary>
    [Serializable]
    public abstract class ProcessorNode : ScriptNode, IProcessorNode
    {
#if UNITY_EDITOR
        private Stopwatch nodeTimer;

        /// <inheritdoc cref="IProcessorNode.LatestExecutionTime"/>
        public long LatestExecutionTime => nodeTimer?.ElapsedMilliseconds ?? -1;
#endif
        private event Action processingCompleted;
        
        /// <inheritdoc cref="IProcessorNode.ProcessingCompleted"/>
        [Obsolete("Please use the static event NodeProcessingCompleted instead.")]
        public event Action ProcessingCompleted
        {
            add => processingCompleted += value;
            remove => processingCompleted -= value;
        }
        
        public static event Action<ProcessorNode> NodeProcessingCompleted;

        [SerializeReference] private ProcessorProviderNode providerNode;
        [SerializeReference] private ProcessorConsumerNode consumerNode;

        private Rng rng = null;

        /// <summary>
        /// Gets the instance of random used for processing this node.
        /// </summary>
        [Obsolete(
            "Get the random object by calling Get<Rng>() instead. This property will be removed in version 2.0.")]
        public Rng Rng => rng ?? Get<Rng>();

        [Obsolete("Processor nodes needn't be dependant on a graph anymore. You can use the parameterless constructor instead. This one will be removed with version 2.0.")]
        protected ProcessorNode(ScriptGraphGraph graph) : base(graph)
        {
            InstantiateInnerNodes();
        }
        
        protected ProcessorNode()
        {
            InstantiateInnerNodes();
        }

        private void InstantiateInnerNodes()
        {
            providerNode = new ProcessorProviderNode(this);
            consumerNode = new ProcessorConsumerNode(this);
        }

        /// <summary>
        /// Executes OnProcess.
        /// </summary>
        public void Process()
        {
            foreach (var inPort in InPorts)
            {
                if (inPort.IsConnectionRequired && !inPort.IsConnected)
                {
                    throw new RequiredPortNotConnectedException(inPort);
                }
            }
            
#if UNITY_EDITOR
            if (nodeTimer == null)
            {
                nodeTimer = new Stopwatch();
            }
            
            nodeTimer.Restart();
#endif
            // Make sure this nodes RNG state is used for consistent results.
            RestoreDependencyStates();
            OnProcess();
            // Save the current state of RNG, in case we need it later on for further processing.
            SaveDependencyStates();

            if (onProcessMainThreadCoroutineCommand != null)
            {
                MainThread.Execute(onProcessMainThreadCoroutineCommand);
            }
            else if (onProcessMainThreadCommand != null)
            {
                MainThread.Execute(onProcessMainThreadCommand);
            }

#if UNITY_EDITOR
            nodeTimer.Stop();
#endif
            MainThread.Execute(processingCompletedCommand, false);
        }

        private IMainThreadCommand onProcessMainThreadCommand;
        private IMainThreadCoroutineCommand onProcessMainThreadCoroutineCommand;
        private IMainThreadCommand processingCompletedCommand;
        
        public override void Initialize()
        {
            base.Initialize();
            
            processingCompletedCommand = new MainThreadCommand(() =>
            {
                processingCompleted?.Invoke();
                NodeProcessingCompleted?.Invoke(this);
            });

            var processor = Get<ScriptGraphProcessor>();
            if (processor.IsMainThreadTimePerFrameLimitEnabled)
            {
                // If the limit is enabled prefer the coroutine version.
                if (!AssignMainThreadCoroutine())
                {
                    AssignMainThread();
                }
            }
            else
            {
                // Otherwise we prefer the non-coroutine version.
                if (!AssignMainThread())
                {
                    AssignMainThreadCoroutine();
                }
            }
        }

        private bool AssignMainThread()
        {
            var mainThreadMethod = GetType().GetMethod(nameof(OnProcessMainThread), BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (mainThreadMethod.DeclaringType == typeof(ProcessorNode)) return false;
            
            // Default has been implemented, we're going to use it.
            onProcessMainThreadCommand = new MainThreadCommand(ProcessMainThread);
            return true;
        }

        private void ProcessMainThread()
        {
            // First we restore the RNG state for this node, for consistent results.
            RestoreDependencyStates();
            OnProcessMainThread();
            // Save the current state of RNG, in case we need it later on for further processing.
            SaveDependencyStates();
        }
        
        private bool AssignMainThreadCoroutine()
        {
            var coroutineMethod = GetType().GetMethod(nameof(OnProcessMainThreadCoroutine), BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (coroutineMethod.DeclaringType == typeof(ProcessorNode)) return false;
            
            // Coroutine has been implemented, we're going to use it.
            onProcessMainThreadCoroutineCommand = new MainThreadCoroutineCommand(ProcessMainThreadCoroutine());
            return true;
        }
        
        private IEnumerator ProcessMainThreadCoroutine()
        {
            // First we restore the RNG state for this node, for consistent results.
            RestoreDependencyStates();
            var action = OnProcessMainThreadCoroutine();
            while (action.MoveNext())
            {
                yield return action.Current;
            }
            // Save the current state of RNG, in case we need it later on for further processing.
            SaveDependencyStates();
        }

        /// <inheritdoc cref="IProcessorNode.OutPorts"/>
        public ReadOnlyCollection<OutPort> OutPorts => providerNode.OutPorts;

        /// <inheritdoc cref="IProcessorNode.GetOutPort"/>
        public OutPort GetOutPort(string name)
        {
            return providerNode.GetOutPort(name);
        }

        /// <inheritdoc cref="IProcessorNode.OnInPortAdded"/>
        public event Action<InPort> OnInPortAdded
        {
            add => consumerNode.OnInPortAdded += value;
            remove => consumerNode.OnInPortAdded -= value;
        }
        
        /// <inheritdoc cref="IProcessorNode.OnInPortRemoved"/>
        public event Action<InPort> OnInPortRemoved
        {
            add => consumerNode.OnInPortRemoved += value;
            remove => consumerNode.OnInPortRemoved -= value;
        }
        
        /// <inheritdoc cref="IProcessorNode.OnInPortRenamed"/>
        public event Action<InPort, string, string> OnInPortRenamed
        {
            add => consumerNode.OnInPortRenamed += value;
            remove => consumerNode.OnInPortRenamed -= value;
        }
        
        /// <inheritdoc cref="IProcessorNode.OnInPortMoved"/>
        public event Action<InPort, int> OnInPortMoved
        {
            add => consumerNode.OnInPortMoved += value;
            remove => consumerNode.OnInPortMoved -= value;
        }
        
        /// <inheritdoc cref="IProcessorNode.OnOutPortAdded"/>
        public event Action<OutPort> OnOutPortAdded
        {
            add => providerNode.OnOutPortAdded += value;
            remove => providerNode.OnOutPortAdded -= value;
        }
        
        /// <inheritdoc cref="IProcessorNode.OnInPortRemoved"/>
        public event Action<OutPort> OnOutPortRemoved
        {
            add => providerNode.OnOutPortRemoved += value;
            remove => providerNode.OnOutPortRemoved -= value;
        }

        /// <inheritdoc cref="IProcessorNode.OnOutPortRenamed"/>
        public event Action<OutPort, string, string> OnOutPortRenamed
        {
            add => providerNode.OnOutPortRenamed += value;
            remove => providerNode.OnOutPortRenamed -= value;
        }

        /// <inheritdoc cref="IProcessorNode.OnOutPortMoved"/>
        public event Action<OutPort, int> OnOutPortMoved
        {
            add => providerNode.OnOutPortMoved += value;
            remove => providerNode.OnOutPortMoved -= value;
        }

        /// <inheritdoc cref="IProcessorNode.InPorts"/>
        public ReadOnlyCollection<InPort> InPorts => consumerNode.InPorts;

        /// <inheritdoc cref="IProcessorNode.GetInPort"/>
        public InPort GetInPort(string name)
        {
            return consumerNode.GetInPort(name);
        }

        /// <inheritdoc cref="IProcessorNode.HasInPort"/>
        public bool HasInPort(string name)
        {
            return consumerNode.HasInPort(name);
        }

        /// <inheritdoc cref="ProviderNode.AddOut"/>
        protected OutPort AddOut<T>(string name)
        {
            // Add an out port, but make this node the owner instead of the provider node, as that's just basically a
            // stub to allow for multiple inheritance.
            return providerNode.AddOut<T>(name, this);
        }

        /// <inheritdoc cref="ProviderNode.AddOut"/>
        protected OutPort AddOut(string name, Type type)
        {
            // Add an out port, but make this node the owner instead of the provider node, as that's just basically a
            // stub to allow for multiple inheritance.
            return providerNode.AddOut(name, type, this);
        }
        
        /// <inheritdoc cref="ConsumerNode.AddIn"/>
        protected InPort AddIn<T>(string name)
        {
            // Add an in port, but make this node the owner instead of the consumer node, as that's just basically a
            // stub to allow for multiple inheritance.
            return consumerNode.AddIn<T>(name, this);
        }
        
        /// <inheritdoc cref="ConsumerNode.AddIn"/>
        protected InPort AddIn(string name, Type type)
        {
            // Add an in port, but make this node the owner instead of the consumer node, as that's just basically a
            // stub to allow for multiple inheritance.
            return consumerNode.AddIn(name, type, this);
        }

        /// <inheritdoc cref="IConsumerNode.RemoveIn"/>
        public void RemoveIn(string name) => consumerNode.RemoveIn(name);
        
        /// <inheritdoc cref="IConsumerNode.RenameIn"/>
        public void RenameIn(string oldName, string newName) => consumerNode.RenameIn(oldName, newName);
        
        /// <inheritdoc cref="IConsumerNode.MoveIn"/>
        public void MoveIn(int oldIndex, int newIndex) => consumerNode.MoveIn(oldIndex, newIndex);

        /// <inheritdoc cref="IProviderNode.RemoveOut"/>
        public void RemoveOut(string name) => providerNode.RemoveOut(name);
        
        /// <inheritdoc cref="IProviderNode.RenameOut"/>
        public void RenameOut(string oldName, string newName) => providerNode.RenameOut(oldName, newName);
        
        /// <inheritdoc cref="IProviderNode.MoveOut"/>
        public void MoveOut(int oldIndex, int newIndex) => providerNode.MoveOut(oldIndex, newIndex);

        /// <inheritdoc cref="IProviderNode.HasOutPort"/>
        public bool HasOutPort(string name)
        {
            return providerNode.HasOutPort(name);
        }

        /// <inheritdoc cref="IProcessorNode.OnLoadOutputPorts"/>
        public virtual void OnLoadOutputPorts()
        {
            providerNode.Parent = this;
            providerNode.OnLoadOutputPortsBase();
        }
        
        /// <inheritdoc cref="IProcessorNode.OnLoadInputPorts"/>
        public virtual void OnLoadInputPorts()
        {
            consumerNode.Parent = this;
            consumerNode.OnLoadInputPortsBase();
        }

        public void ClearPorts()
        {
            consumerNode.ClearPorts();
            providerNode.ClearPorts();
        }

        /// <summary>
        /// Contains all the processing logic of this node. Called by Process.
        /// </summary>
        protected virtual void OnProcess() { }

        /// <summary>
        /// Contains all the processing logic of this node that must be executed on the main thread.
        /// Called by Process after OnProcess
        /// </summary>
        protected virtual void OnProcessMainThread() { }
        
        protected virtual IEnumerator OnProcessMainThreadCoroutine() => null;

        #region Subnodes
        /// <summary>
        /// Used to make ProcessorNode basically inherit from both provider node and consumer node.
        /// </summary>
        [Serializable]
        private class ProcessorProviderNode : ProviderNode
        {
            private ProcessorNode parent;
            
            public ProcessorNode Parent
            {
                get => parent;
                set => parent = value;
            }
            
            public ProcessorProviderNode(ProcessorNode parent)
            {
                this.parent = parent;
            }

            public override void OnLoadOutputPorts()
            {
                parent.OnLoadOutputPorts();
            }
            
            public void OnLoadOutputPortsBase()
            {
                base.OnLoadOutputPorts();
            }

            protected override IProviderNode Node => parent; 
        }

        /// <summary>
        /// Used to make ProcessorNode basically inherit from both provider node and consumer node.
        /// </summary>
        [Serializable]
        private class ProcessorConsumerNode : ConsumerNode
        {
            private ProcessorNode parent;

            public ProcessorNode Parent
            {
                get => parent;
                set => parent = value; 
            }

            public ProcessorConsumerNode(ProcessorNode parent)
            {
                this.parent = parent;
            }

            public override void OnLoadInputPorts()
            {
                parent.OnLoadInputPorts();
            }

            public void OnLoadInputPortsBase()
            {
                base.OnLoadInputPorts();
            }

            protected override IConsumerNode Node => parent;
        }
        #endregion
    }
} 