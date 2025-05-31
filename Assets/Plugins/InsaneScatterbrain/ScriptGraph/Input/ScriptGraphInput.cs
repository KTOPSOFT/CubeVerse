using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Component for easily assigning values to input parameters in the Unity editor. For example, you can
    /// reference this component in a UI component's OnChange UnityEvent to update an input parameter's value.
    /// </summary>
    public abstract class ScriptGraphInput : ScriptGraphMonoBehaviour
    {
        [SerializeField] private ScriptGraphRunner runner = default;
        
        [FormerlySerializedAs("parameter")] 
        [SerializeField] private string parameterId;

        /// <summary>
        /// Gets the input parameter's type.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Gets the script graph runner.
        /// </summary>
        public ScriptGraphRunner Runner => runner;
        
        /// <summary>
        /// Gets/sets the parameter ID.
        /// </summary>
        public string ParameterId
        {
            get => parameterId;
            set => parameterId = value;
        }
    }
    
    /// <inheritdoc cref="ScriptGraphInput"/>
    public abstract class ScriptGraphInput<T> : ScriptGraphInput
    {
        [SerializeField] private T defaultValue = default;
        
        /// <inheritdoc cref="ScriptGraphInput.Type"/>
        public override Type Type => typeof(T);

        private void OnEnable()
        {
            if (Runner == null)
            {
                Debug.LogWarning("No runner assigned to input.", gameObject);
                return;
            }

            if (string.IsNullOrEmpty(ParameterId))
            {
                Debug.LogWarning("No parameter selected for input.", gameObject);
                return;
            }
            
            Set(defaultValue);
        }

        /// <summary>
        /// Sets the input parameter's value.
        /// </summary>
        /// <param name="value">The new value.</param>
        public void Set(T value)
        {
            Runner.SetInById(ParameterId, value);
        }
    }
}