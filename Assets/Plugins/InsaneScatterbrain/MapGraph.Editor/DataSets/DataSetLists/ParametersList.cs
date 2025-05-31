using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing script graph parameters.
    /// </summary>
    public abstract class ParametersList : DataSetListBase<ScriptGraphParameter>
    {
        /// <summary>
        /// Gets the types available to create parameters for.
        /// </summary>
        protected abstract IEnumerable<Type> Types { get; }
        
        /// <inheritdoc cref="DataSetListBase{T}.AdditionalFieldsWidth"/>
        protected override int AdditionalFieldsWidth => 150;
        
        private readonly ScriptGraphParameters parameters;

        protected ParametersList(ScriptGraphParameters parameters, ScriptableObject dataObject) : base(parameters)
        {
            OnRegisterUndo += undoName =>
            {
                var graphs = Assets.Find<ScriptGraphGraph>();
                foreach (var graph in graphs)
                {
                    Undo.RegisterCompleteObjectUndo(graph, undoName);
                }
            };
            
            this.parameters = parameters;
            ReorderableList.onAddDropdownCallback = (rect, list) =>
            {
                var menu = new GenericMenu();

                foreach (var type in Types)
                {
                    menu.AddItem(new GUIContent(type.GetFriendlyName()), false, AddNewParameter, type);
                }
                
                menu.ShowAsContext();
            };

            DataObject = dataObject;
        }

        /// <inheritdoc cref="DataSetListBase{T}.DrawAdditionalFields"/>
        protected override void DrawAdditionalFields(Rect rect, string id)
        {
            var parameterType = parameters.GetType(id);
            
            EditorGUI.LabelField(new Rect(
                rect.x + rect.width - AdditionalFieldsWidth, 
                rect.y, AdditionalFieldsWidth, 
                EditorGUIUtility.singleLineHeight), parameterType.GetFriendlyName());
        }

        private void AddNewParameter(object userData)
        {
            var type = (Type) userData;
            AddNewParameter(type);
        }
        
        private void AddNewParameter(Type type)
        {
            if (DataObject != null)
            {
                var undoName = $"Add {LabelText}";
                Undo.RegisterCompleteObjectUndo(DataObject, undoName);
                var graphs = Assets.Find<ScriptGraphGraph>();
                foreach (var graph in graphs)
                {
                    Undo.RegisterCompleteObjectUndo(graph, undoName);
                }
            }
            
            string newName;
            var newNameTrailingNumber = 0;
            do
            {
                newName = DefaultName;
                if (newNameTrailingNumber > 0)
                {
                    newName += $" {newNameTrailingNumber}";
                }

                newNameTrailingNumber++;
            } 
            while (DataSet.ContainsName(newName));
            
            parameters.Add(new ScriptGraphParameter(newName, type));

            SaveAsset();
            UpdateList();
        }
        
        protected static IEnumerable<TAttribute> GetFieldAttributes<TObject, TAttribute>() where TAttribute : Attribute
        {
            var attributes = new List<TAttribute>();
            var types = Services.Types.ChildrenOf<TObject>();
            
            foreach (var type in types)
            {
                var typeAttributes = type.GetAllPrivateFields()
                    .Select(fieldInfo => fieldInfo.GetAttribute<TAttribute>()).Where(attribute => attribute != null); 
                
                attributes.AddRange(typeAttributes);
            }

            return attributes;
        }
        
        protected static IEnumerable<TAttribute> GetClassAttributes<TObject, TAttribute>() where TAttribute : Attribute
        {
            var types = Services.Types.ChildrenOf<TObject>();

            return types.Select(type => type.GetAttribute<TAttribute>()).Where(attribute => attribute != null);
        }

        protected static IEnumerable<Type> SortTypes(IEnumerable<Type> types)
        {
            return types
                .OrderBy(type => type.GetFriendlyName().ToLower() != type.GetFriendlyName())
                .ThenBy(type => type.GetFriendlyName());
        }
    }
}