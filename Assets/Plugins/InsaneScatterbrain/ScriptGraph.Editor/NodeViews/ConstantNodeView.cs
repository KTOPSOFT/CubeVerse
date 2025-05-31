using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// This is the base class for all constant node views.
    /// </summary>
    public abstract class ConstantNodeView : ScriptNodeView
    {
        private ConstantNode constantNode;
        
        /// <summary>
        /// Gets the constant node associated with this node view.
        /// </summary>
        public ConstantNode ConstantNode => constantNode;
        
        public ConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            constantNode = node;
            
            AddToClassList("constant-node-view");
        }

        /// <summary>
        /// Adds a default field to assign the constant value with.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <typeparam name="TValue">The value's type.</typeparam>
        /// <typeparam name="TField">The type of field to assign the value to.</typeparam>
        /// <returns>The field.</returns>
        protected TField AddDefaultField<TValue, TField>(TValue value) where TField : VisualElement, INotifyValueChanged<TValue>, new()
        {
            var valueField = new TField(); 
            valueField.AddToClassList("const-input-field");
            valueField.SetValueWithoutNotify(value);
            valueField.RegisterValueChangedCallback(e =>
            {
                constantNode.Value = e.newValue;
                EditorUtility.SetDirty(Graph);
            });
            inputContainer.Add(valueField);

            return valueField;
        }
        
        /// <summary>
        /// Adds a default object field to assign the constant value with. This is useful for any type that inherits
        /// from Unity's Object class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The object field.</returns>
        protected ObjectField AddDefaultObjectField<TValue>(TValue value) where TValue : Object
        {
            var objField = AddDefaultField<Object, ObjectField>(value); 
            objField.objectType = typeof(TValue);
            objField.allowSceneObjects = false;
            return objField;
        }
    }
}