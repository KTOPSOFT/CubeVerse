using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.Extensions;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// The base node for any node that consumes (takes) data. In other words, any node that has one more in ports.
    /// </summary>
    [Serializable]
    public abstract class ConsumerNode : ScriptNode, IConsumerNode
    {
        [SerializeReference] private List<InPort> inPorts = new List<InPort>();

        private ReadOnlyCollection<InPort> readOnlyInPorts;

        /// <inheritdoc cref="IConsumerNode.InPorts"/>
        public ReadOnlyCollection<InPort> InPorts => readOnlyInPorts ?? (readOnlyInPorts = inPorts.AsReadOnly());

        /// <inheritdoc cref="IConsumerNode.OnInPortAdded"/>
        public event Action<InPort> OnInPortAdded;
        
        /// <inheritdoc cref="IConsumerNode.OnInPortRemoved"/>
        public event Action<InPort> OnInPortRemoved;
        
        /// <inheritdoc cref="IConsumerNode.OnInPortRemoved"/>
        public event Action<InPort, string, string> OnInPortRenamed;

        /// <inheritdoc cref="IConsumerNode.OnInPortMoved"/>
        public event Action<InPort, int> OnInPortMoved;

        /// <inheritdoc cref="IConsumerNode.GetInPort"/>
        public InPort GetInPort(string name)
        {
            var inPort = inPorts.Find(p => p.Name == name);
            if (inPort == null)
            {
                throw new PortNotFoundException(name);
            }

            return inPort;
        }

        /// <inheritdoc cref="IConsumerNode.HasInPort"/>
        public bool HasInPort(string name)
        {
            return inPorts.Any(p => p.Name == name);
        }

        /// <summary>
        /// Adds an in port with the given name of the given type.
        /// </summary>
        /// <param name="name">The port's name.</param>
        /// <param name="type">The port's data type.</param>
        /// <param name="owner">
        /// The node that is marked as the owner. If this is null that will be this node
        /// (which is probably what you want).
        /// </param>
        /// <param name="port">The existing port.</param>
        /// <returns>The new in port.</returns>
        public InPort AddIn(string name, Type type, IScriptNode owner = null, InPort port = null)
        {
            if (port != null && port.Name != name)
            {
                port.Name = name;
            }
            
            var existingPort = inPorts.Find(p => p.Name == name);
            if (existingPort != null)
            {
                if (existingPort.Type == type)
                {
                    // Port already exists and the type matches. Re-add it to the list, to make sure the order is
                    // the same as how they are defined in OnLoadInputPorts.
                    inPorts.Remove(existingPort);
                    inPorts.Add(existingPort);
                    
                    // By default the port shouldn't require a connection.
                    existingPort.IsConnectionRequired = false;
                    
                    OnInPortAdded?.Invoke(existingPort);
                    return existingPort;
                }

                // Port already exists, but the type doesn't match, remove it, so a new one with the correct type can be added.
                inPorts.Remove(existingPort);
            }
            
            if (owner == null) owner = Node;
            
            var inPort = InPort.Create(name, type, owner); 
            inPorts.Add(inPort);

            OnInPortAdded?.Invoke(inPort);
            
            return inPort; 
        }
        
        /// <inheritdoc cref="AddIn"/>
        public InPort AddIn<T>(string name, IScriptNode owner = null)
        {
            return AddIn(name, typeof(T), owner);
        }
        
        /// <inheritdoc cref="IConsumerNode.RemoveIn"/>
        public void RemoveIn(string name)
        {
            var inPort = inPorts.Find(p => p.Name == name);
            if (inPort == null)
            {
                return;
            }
            
            inPort.Disconnect();
            inPorts.Remove(inPort);
            OnInPortRemoved?.Invoke(inPort);
        }

        /// <inheritdoc cref="IConsumerNode.RenameIn"/>
        public void RenameIn(string oldName, string newName)
        {
            var inPort = inPorts.Find(p => p.Name == oldName);
            inPort.Name = newName;
            OnInPortRenamed?.Invoke(inPort, oldName, newName);
        }

        /// <inheritdoc cref="IConsumerNode.MoveIn"/>
        public void MoveIn(int oldIndex, int newIndex)
        {
            var inPort = inPorts[oldIndex];
            inPorts.RemoveAt(oldIndex);
            inPorts.Insert(newIndex, inPort);
            OnInPortMoved?.Invoke(inPort, newIndex);
        }
        
        public void ClearPorts()
        {
            for (var i = inPorts.Count - 1; i >= 0; --i)
            {
                var inPort = inPorts[inPorts.Count];
                RemoveIn(inPort.Name);
            }
        }

        /// <inheritdoc cref="IConsumerNode.OnLoadInputPorts"/>
        public virtual void OnLoadInputPorts()
        {
            var inPortFields = Node.GetType().GetAllPrivateFields()
                .Where(field => typeof(InPort).IsAssignableFrom(field.FieldType));
            
            foreach (var field in inPortFields)
            {
                var inPortAttribute = field.GetAttribute<InPortAttribute>();
                
                if (inPortAttribute == null) continue;

                var port = field.GetValue(Node) as InPort;
                
                port = AddIn(inPortAttribute.Name, inPortAttribute.Type, port: port);
                port.IsConnectionRequired = inPortAttribute.IsConnectionRequired; 
                field.SetValue(Node, port);  
            }
        }

        /// <summary>
        /// In case of proxy classes (such as ProcessorNode uses) it's useful to explicitly set the node's type.
        /// </summary>
        protected virtual IConsumerNode Node => this;
    }
}