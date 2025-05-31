using System;
using System.Collections.ObjectModel;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Interface for any node that consumes (takes) data. In other words, any node that has one more in ports.
    /// </summary>
    public interface IConsumerNode : IScriptNode
    {
        /// <summary>
        /// Event is triggered everytime an in port is added to the node.
        /// </summary>
        event Action<InPort> OnInPortAdded;
        
        /// <summary>
        /// Event is triggered everytime an in port is removed from the node.
        /// </summary>
        event Action<InPort> OnInPortRemoved;
        
        /// <summary>
        /// Event is triggered everytime an in port is renamed.
        /// </summary>
        event Action<InPort, string, string> OnInPortRenamed;
        
        /// <summary>
        /// Event is triggered everytime an in port is moved.
        /// </summary>
        event Action<InPort, int> OnInPortMoved;
        
        /// <summary>
        /// Gets all the in ports.
        /// </summary>
        ReadOnlyCollection<InPort> InPorts { get; }
        
        /// <summary>
        /// Returns the in port with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The in port.</returns>
        InPort GetInPort(string name);
        
        /// <summary>
        /// Returns whether the in port with the given name exists on the node.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if the port exists, false otherwise.</returns>
        bool HasInPort(string name);
        
        /// <summary>
        /// This method should contain all the in port initialization code. In other words, it should contain all
        /// the AddIn calls for the node.
        /// </summary>
        void OnLoadInputPorts();
        
        /// <summary>
        /// Removes the in port with the given name from the node.
        /// </summary>
        /// <param name="name">The port's name.</param>
        void RemoveIn(string name);
        
        /// <summary>
        /// Renames the in port.
        /// </summary>
        /// <param name="oldName">The port's old name.</param>
        /// <param name="newName">The port's new name.</param>
        void RenameIn(string oldName, string newName);
        
        /// <summary>
        /// Move the in port to a different index
        /// </summary>
        /// <param name="oldIndex">The port's old index.</param>
        /// <param name="newIndex">The port's new index.</param>
        void MoveIn(int oldIndex, int newIndex);
    }
}