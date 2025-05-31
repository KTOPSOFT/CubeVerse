using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Out ports can be added to nodes as a means to output data from the node.
    /// </summary>
    [Serializable] 
    public class OutPort : ScriptGraphPort
    {
        [SerializeReference] private List<InPort> connectedInPorts = new List<InPort>();

        private ReadOnlyCollection<InPort> readOnlyConnectedInPorts;

        /// <summary>
        /// Gets the in ports that this out port is connected to. Returns an empty list if it's unconnected.
        /// </summary>
        public ReadOnlyCollection<InPort> ConnectedInPorts =>
            readOnlyConnectedInPorts ?? (readOnlyConnectedInPorts = connectedInPorts.AsReadOnly());
        
        /// <summary>
        /// Return true if this node is connected to another node, false otherwise.
        /// </summary>
        public bool IsConnected => !IsFlaggedUnconnected && ConnectedInPorts.Count > 0;

        /// <summary>
        /// Gets/sets the unconnected flag. If this flag is set to true, the port will be considered
        /// to be unconnected, even if it does actually have a connection with another port.
        /// </summary>
        public bool IsFlaggedUnconnected { get; set; }

        private Func<object> resultProvider;

        /// <summary>
        /// Gets the data from this port's result provider, which is set through Set. If no result provider has been
        /// assigned, the default value for the port's type will be returned.
        /// </summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <returns>The data, or the default if no result provider is set.</returns>
        public T Get<T>()
        {
            if (!typeof(T).IsAssignableFrom(Type))
            {
                // If the data type requested doesn't match the data type of the port, an error is thrown.
                throw new IncompatiblePortTypeException(Name, typeof(T), Type);
            }

            var obj = resultProvider?.Invoke();

            // If there's not result provider set. Return a default value.
            if (obj == null) return default;

            if (obj is T value) return value;
            
            // If the type of the port and request doesn't match, try to convert it.
            return (T)Convert.ChangeType(obj, typeof(T));
        }
        
        /// <summary>
        /// Gets the data from this port's result provider, which is set through Set. If no result provider has been
        /// assigned, null is returned.
        /// </summary>
        /// <returns>The data, or null if no result provider is set.</returns>
        public object Get()
        {
            var result = resultProvider?.Invoke();

            if (result != null && result.GetType() != Type)
            {
                // If the data type requested doesn't match the data type of the port, an error is thrown.
                Debug.LogError($"Invalid type getting port result: {result.GetType()} given, but expected {Type}.");
            }

            return result;
        }
        
        /// <summary>
        /// Sets the result provider, which provides the result for the Get calls. 
        /// </summary>
        /// <param name="result">The result provider.</param>
        /// <typeparam name="T">The type the result provider returns.</typeparam>
        public void Set<T>(Func<T> result)
        {
            if (typeof(T) != Type)
            {
                // If the data type for the result provider doesn't match the data type of the port, an error is thrown.
                Debug.LogError($"Invalid type setting port result provider: {typeof(T)} given, but expected {Type}.");
            }
            resultProvider = () => result();
        }

        /// <inheritdoc cref="Set{T}"/>
        public void Set(Func<object> result)
        {
            resultProvider = result;
        }

        /// <summary>
        /// Connects this out port to the given in port.
        /// </summary>
        /// <param name="inPort">The in port to connect to.</param>
        public void Connect(InPort inPort)
        {
            if (!inPort.Type.IsAssignableFrom(Type))
            {
                // If the in port and out port types don't match. Throw an error.
                Debug.LogError($"Invalid type connecting ports. InPort is of type {inPort.Type}, but OutPort is of type {Type}");
            }
            connectedInPorts.Add(inPort);
        }

        /// <summary>
        /// Disconnect this out port from the given in port.
        /// </summary>
        /// <param name="inPort">The in port to disconnect from.</param>
        public void Disconnect(InPort inPort)
        {
            connectedInPorts.Remove(inPort);
        }
        
        /// <summary>
        /// Disconnect all connected in ports from this out port.
        /// </summary>
        public void DisconnectAll()
        {
            var inPortsToDisconnect = new List<InPort>(ConnectedInPorts);
            foreach (var inPort in inPortsToDisconnect)
            {
                inPort.Disconnect();
            }
        }
        
        /// <summary>
        /// Factory method to create an out port.
        /// </summary>
        /// <param name="name">The out port's name.</param>
        /// <param name="type">The out port's type.</param>
        /// <param name="owner">The owning node. Usually the node that it's part of.</param>
        /// <returns>The new out port.</returns>
        public static OutPort Create(string name, Type type, IScriptNode owner)
        {
            return ScriptGraphPort.Create<OutPort>(name, type, owner);
        }
    }
}