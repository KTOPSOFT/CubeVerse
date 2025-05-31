using System;
using System.Collections;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if MAP_GRAPH_EDITORCOROUTINES
using Unity.EditorCoroutines.Editor;
#endif

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Base class for reorderable data sets list in the editor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataSetListBase<T> : ListBase where T : IDataSetItem
    {
        /// <summary>
        /// Event triggered whenever a new item has been selected.
        /// </summary>
        public event Action<string> OnSelect;
        
        /// <summary>
        /// Event triggered whenever an item has been removed.
        /// </summary>
        public event Action<string> OnRemove;

        protected event Action<string> OnRegisterUndo;
        
        /// <summary>
        /// Gets the default name for a new item.
        /// </summary>
        protected abstract string DefaultName { get; }
        
        /// <summary>
        /// Gets the title text displayed in the list's label.
        /// </summary>
        protected abstract string LabelText { get; }
        
        /// <summary>
        /// Gets the width in pixel available for any additional fields.
        /// </summary>
        protected abstract int AdditionalFieldsWidth { get; }

        /// <summary>
        /// Gets the data set the list represents.
        /// </summary>
        protected IDataSet<T> DataSet { get; }
        
        /// <summary>
        /// Gets/sets the scriptable object that needs to be saved whenever the data set is changed.
        /// </summary>
        public ScriptableObject DataObject { get; set; }
        
        /// <summary>
        /// Draw any additional item fields.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="id">The item ID.</param>
        protected abstract void DrawAdditionalFields(Rect rect, string id);

        /// <summary>
        /// Gets the reorderable list element.
        /// </summary>
        protected override ReorderableList ReorderableList { get; }

        private readonly List<string> orderedIds;

        /// <inheritdoc cref="IDataSet{T}.OrderedIds"/>
        public IList<string> OrderedIds => DataSet.OrderedIds;

        /// <summary>
        /// Draws list.
        /// </summary>
        public virtual void DoLayoutList()
        {
            EditorGUILayout.LabelField(LabelText, DefaultHeaderStyle);
            ReorderableList.DoLayoutList();
        }

        /// <summary>
        /// Gets the currently selected index.
        /// </summary>
        public int SelectedIndex => ReorderableList.index;

        /// <summary>
        /// Draws a list item's name field.
        /// </summary>
        /// <param name="rect">The rect where to draw the field.</param>
        /// <param name="itemName">The item's current name.</param>
        /// <returns>The item's new name.</returns>
        protected virtual string NameField(Rect rect, string itemName)
        {
            return EditorGUI.DelayedTextField(
                new Rect(rect.x, rect.y, rect.width - 5 - AdditionalFieldsWidth, EditorGUIUtility.singleLineHeight),
                itemName);
        }

        protected DataSetListBase(IDataSet<T> dataSet)
        {
            Undo.undoRedoPerformed += UpdateList;
            
            DataSet = dataSet;
            
            // By default the data object is the object itself, if it's a scriptable object.
            DataObject = dataSet as ScriptableObject;

            orderedIds = new List<string>(dataSet.OrderedIds);

            ReorderableList = new ReorderableList(orderedIds, typeof(string), true, false, true, true)
            {
                headerHeight = 4,
                drawElementCallback = (rect, i, active, focused) =>
                {
                    var id = OrderedIds[i];
                    var itemName = dataSet.GetName(id);
                    
                    EditorGUI.BeginChangeCheck();
                    
                    // Give the text field a unique ID, consisting of the its index and the list's ID, so that it can be used
                    // to see if it's the currently focused control and select the associated item in the list. 
                    SetNextListItemControlName(i);

                    var newItemName = NameField(rect, itemName);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        try
                        {
                            dataSet.Rename(id, newItemName);
                            SaveAsset();
                        }
                        catch (ArgumentException)
                        {
                            EditorUtility.DisplayDialog("", "Name already exists.", "OK");
                        }
                    }

                    DrawAdditionalFields(rect, id);
                    UpdateSelectedIndex();
                },
                onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    var id = dataSet.OrderedIds[oldIndex];

                    dataSet.Move(id, newIndex);
                    SaveAsset();
                },
                onRemoveCallback = list =>
                {
                    // Make sure there's no focus on the name field, as keeping having it focused will unintentionally
                    // put the deleted item's name into the name field of the item selected afterwards, causing
                    // unintentional renames.
                    GUI.FocusControl(null);

#if MAP_GRAPH_EDITORCOROUTINES
                    EditorCoroutineUtility.StartCoroutineOwnerless(DelayedDelete(list));
#endif
                },
                onSelectCallback = list =>
                {
                    if (list.index >= dataSet.OrderedIds.Count)
                    {
                        list.index = dataSet.OrderedIds.Count - 1;
                    }
                    
                    var idSelected = dataSet.OrderedIds[list.index];
                    OnSelect?.Invoke(idSelected);
                },
                index = -1
            };
        }
        
        private IEnumerator DelayedDelete(ReorderableList list)
        {
            // Wait a frame to avoid accidental renaming of the newly selected item after deletion.
            yield return null;
            
            if (DataObject != null)
            {
                var undoName = $"Remove {LabelText}";
                Undo.RegisterCompleteObjectUndo(DataObject, undoName);
                OnRegisterUndo?.Invoke(undoName);
            }

            var idToRemove = DataSet.OrderedIds[list.index];

            DataSet.Remove(idToRemove);
            UpdateList();
            SaveAsset();

            var newIndex = list.index - 1;
            list.index = newIndex;
                    
            OnRemove?.Invoke(idToRemove);

            // Only of something is selected, we call OnSelect.
            if (list.index > -1)
            {
                OnSelect?.Invoke(DataSet.OrderedIds[list.index]);
            }
        }

        public virtual void Dispose()
        {
            Undo.undoRedoPerformed -= UpdateList;
        }

        /// <summary>
        /// Loads the entries in the reorderable list.
        /// </summary>
        public void UpdateList()
        {
            orderedIds.Clear();
            orderedIds.AddRange(DataSet.OrderedIds);
            ReorderableList.list = orderedIds;
        }

        protected void SaveAsset()
        {
            if (DataObject == null) return;     // No serialized object to save.
            
            EditorUtility.SetDirty(DataObject);
        }
    }
}