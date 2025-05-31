using System;
using System.Collections.ObjectModel;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Interface for any node that provides (gives) data. In other words, any node that has one more out ports.
    /// </summary>
    public interface IProviderNode : IScriptNode
    {
        /// <summary>
        /// Event is triggered everytime an out port is added to the node.
        /// </summary>
        event Action<OutPort> OnOutPortAdded;
        
        /// <summary>
        /// Event is triggered everytime an out port is removed from the node.
        /// </summary>
        event Action<OutPort> OnOutPortRemoved;
        
        /// <summary>
        /// Event is triggered everytime an out port is renamed.
        /// </summary>
        event Action<OutPort, string, string> OnOutPortRenamed;
        
        /// <summary>
        /// Event is triggered everytime an out port is moved.
        /// </summary>
        event Action<OutPort, int> OnOutPortMoved;
        
        /// <summary>
        /// Gets all the out ports.
        /// </summary>
        ReadOnlyCollection<OutPort> OutPorts { get; }
        
        /// <summary>
        /// Returns the out port with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The out port.</returns>
        OutPort GetOutPort(string name);
        
        /// <summary>
        /// This method should contain all the out port initialization code. In other words, it should contain all
        /// the AddOut calls for the node.
        /// </summary>
        void OnLoadOutputPorts();
        
        /// <summary>
        /// Removes the out port with the given name from the node.
        /// </summary>
        /// <param name="name">The port's name.</param>
        void RemoveOut(string name);
                
        /// <summary>
        /// Renames the out port.
        /// </summary>
        /// <param name="oldName">The port's old name.</param>
        /// <param name="newName">The port's new name.</param>
        void RenameOut(string oldName, string newName);
        
        /// <summary>
        /// Move the out port to a different index
        /// </summary>
        /// <param name="oldIndex">The port's old index.</param>
        /// <param name="newIndex">The port's new index.</param>
        void MoveOut(int oldIndex, int newIndex);

        /// <summary>
        /// Returns whether the out port with the given name exists on the node.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>True if the port exists, false otherwise.</returns>
        bool HasOutPort(string name);
    }
}