using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Base class for the in and out ports that can be added to each script graph node.
    /// </summary>
    [Serializable]
    public abstract class ScriptGraphPort : ISerializationCallbackReceiver
    {
        [SerializeField] private string name;

        /// <summary>
        /// Gets the node's name.
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        private Type type;

        /// <summary>
        /// Gets the node's type.
        /// </summary>
        public Type Type => type;

        [SerializeReference] private IScriptNode owner;
        
        /// <summary>
        /// Gets the node's owner, usually the node that it's been added to.
        /// </summary>
        public IScriptNode Owner => owner;

        /// <summary>
        /// Factory method to create a new port of type.
        /// </summary>
        /// <param name="name">The port's name.</param>
        /// <param name="owner">The owning node.</param>
        /// <typeparam name="TPort">The port's type. Probably want this to be either InPort or OutPort.</typeparam>
        /// <typeparam name="TValue">The type of value the port either provides or consumes.</typeparam>
        /// <returns>The new port.</returns>
        public static TPort Create<TPort, TValue>(string name, IScriptNode owner) where TPort : ScriptGraphPort, new()
        {
            return new TPort
            {
                name = name,
                type = typeof(TValue),
                owner = owner
            };
        }
        
        /// <inheritdoc cref="Create{TPort,TValue}"/>
        protected static TPort Create<TPort>(string name, Type type, IScriptNode owner) where TPort : ScriptGraphPort, new()
        { 
            return new TPort
            {
                name = name,
                type = type,
                owner = owner
            };
        }

        #region Serialization
        
        [SerializeField] private string typeName;
        
        public void OnBeforeSerialize()
        {
            if (type == null) return;
            
            typeName = type.AssemblyQualifiedName; 
        }

        public void OnAfterDeserialize()
        {
            type = Type.GetType(typeName);
        }
        
        #endregion
    }
}