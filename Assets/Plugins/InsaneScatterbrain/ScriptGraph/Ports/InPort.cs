using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// In ports can be added to nodes as a means to input data into the node.
    /// </summary>
    [Serializable]
    public class InPort : ScriptGraphPort
    {
        [SerializeReference] private OutPort connectedOut;  
        [SerializeField] private bool isConnectionRequired = false;
        
        /// <summary>
        /// Gets/sets whether a connection to this port is required for the node to function properly. False by default.
        /// </summary>
        public bool IsConnectionRequired
        {
            get => isConnectionRequired;
            set => isConnectionRequired = value;
        }
        
        /// <summary>
        /// Gets the out port that this in port is connected to. Null if it's not connected.
        /// </summary>
        public OutPort ConnectedOut => connectedOut;
        
        /// <summary>
        /// Return true if this node is connected to another node, false otherwise.
        /// </summary>
        public bool IsConnected => connectedOut != null && connectedOut.IsConnected;

        /// <summary>
        /// Pulls the data from the connected source and returns it. If the port is unconnected, it returns the default
        /// value for the type of port. 
        /// </summary>
        /// <typeparam name="T">The type of the data to return.</typeparam>
        /// <returns>The data, or default value if unconnected.</returns>
        public T Get<T>()
        {
            if (typeof(T) != Type)
            {
                // If the data type requested doesn't match the data type of the port, an error is thrown.
                Debug.LogError($"Invalid type getting port result: {typeof(T)} given, but expected {Type}.");
            }

            return connectedOut == null ? default : connectedOut.Get<T>();
        }

        /// <summary>
        /// Pulls the data from the connected source and returns it. If the port is unconnected, it returns null.
        /// </summary>
        /// <returns>The data, or null if unconnected.</returns>
        public object Get()
        {
            return connectedOut?.Get();
        }
        
        /// <summary>
        /// Connects this in port to the given out port.
        /// </summary>
        /// <param name="outPort">The out port.</param>
        public void Connect(OutPort outPort)
        {
            if (!Type.IsAssignableFrom(outPort.Type))
            {
                // If the in port and out port types don't match. Throw an error.
                Debug.LogError($"Invalid type connecting ports. InPort is of type {Type}, but OutPort is of type {outPort.Type}");
            }
            outPort.Connect(this);
            connectedOut = outPort;
        }

        
        /// <summary>
        /// Disconnect this in port from the connected out port.
        /// </summary>
        public void Disconnect()
        {
            if (connectedOut == null) return;
            
            connectedOut.Disconnect(this);
            connectedOut = null; 
        }
        
        /// <summary>
        /// Factory method to create a new in port.
        /// </summary>
        /// <param name="name">The port's name.</param>
        /// <param name="type">The port's type.</param>
        /// <param name="owner">The port's owning node. Usually the node it's part of.</param>
        /// <returns>The new in node.</returns>
        public static InPort Create(string name, Type type, IScriptNode owner)
        {
            return ScriptGraphPort.Create<InPort>(name, type, owner);
        }
    }
}