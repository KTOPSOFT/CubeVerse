using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Base class for editor list for displaying and editing object type entry lists.
    /// </summary>
    /// <typeparam name="TType">The type of object type.</typeparam>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <typeparam name="TObject">The entry's object type.</typeparam>
    public abstract class ObjectTypeEntryList<TType, TEntry, TObject> : ListBase
        where TType : IObjectType<TEntry, TObject>
        where TObject : Object
        where TEntry : IObjectTypeEntry<TObject>
    {
        private readonly ObjectTypeSetList<TType, TEntry, TObject> parentList;
        private readonly ReorderableList reorderableList;

        protected abstract string EntryTypeNamePlural { get; }
        protected abstract string EntryTypeNameSingular { get; }

        protected override ReorderableList ReorderableList => reorderableList;
        
        const int WeightFieldWidth = 75;

        /// <summary>
        /// Draws list.
        /// </summary>
        public void DoLayoutList()
        {
            reorderableList.displayAdd = parentList.SelectedIndex > -1;
            reorderableList.displayRemove = parentList.SelectedIndex > -1;

            if (parentList.SelectedIndex > -1)
            {
                var typeId = parentList.OrderedIds[parentList.SelectedIndex];
                var typeName = parentList.ObjectTypeSet.GetName(typeId);

                EditorGUILayout.LabelField($"{typeName} {EntryTypeNamePlural}", DefaultHeaderStyle);
                
                // The header is drawn here, instead of part as the list, as that doesn't seem to work for
                // all Unity versions.
                DrawHeader();
            }
            
            reorderableList.DoLayoutList();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(ReorderableList.defaultBehaviours.headerBackground);
            
            var typeIds = parentList.OrderedIds;

            if (reorderableList.count == 0 || typeIds.Count == 0 || parentList.SelectedIndex < 0)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            var typeId = parentList.OrderedIds[parentList.SelectedIndex];
                    
            var isWeightEnabled = parentList.ObjectTypeSet.IsWeightEnabled(typeId);
            
            EditorGUILayout.LabelField(EntryTypeNameSingular);

            EditorGUI.BeginChangeCheck();
                    
            var weightContent = new GUIContent("Weight", "Enable weights for this type. Entries with higher weights are more likely to be selected.\n\nEnabling this has a slight performance impact.");
                    
            isWeightEnabled = EditorGUILayout.ToggleLeft(weightContent, isWeightEnabled, GUILayout.Width(WeightFieldWidth));
            if (EditorGUI.EndChangeCheck())
            {
                parentList.ObjectTypeSet.SetWeightEnabled(typeId, isWeightEnabled);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Refreshes list entries.
        /// </summary>
        public void RefreshList()
        {
            reorderableList.index = -1;
            reorderableList.list = new int[GetEntryCount(parentList.SelectedIndex)];
        }

        private void ApplyChanges()
        {
            parentList.ApplyChanges();
        }

        protected ObjectTypeEntryList(ObjectTypeSetList<TType, TEntry, TObject> parentList)
        {
            Undo.undoRedoPerformed += RefreshList;
            
            this.parentList = parentList;
            
            var typeIds = parentList.OrderedIds;
            var dataSet = parentList.ObjectTypeSet;

            reorderableList = new ReorderableList(Array.Empty<int>(), typeof(TEntry), true, false, false, false)
            {
                headerHeight = 4,
                drawElementCallback = (rect, i, active, focused) =>
                {
                    var entryObject = GetObject(i);
                    var weight = GetWeight(i);
                    var typeId = parentList.OrderedIds[parentList.SelectedIndex];

                    EditorGUI.BeginChangeCheck();
                    
                    // Give the text field a unique ID, consisting of the its index and the list's ID, so that it can be used
                    // to see if it's the currently focused control and select the associated item in the list. 
                    SetNextListItemControlName(i);
                    
                    // The rect for the entry object field, which is the entire row minus the weight field.
                    var entryObjectRect = new Rect(rect.x, rect.y, rect.width - WeightFieldWidth, rect.height);
                    
                    // The rect for the weight field.
                    var weightRect = new Rect(rect.x + rect.width - WeightFieldWidth, rect.y, WeightFieldWidth, rect.height);
                    
                    var newEntryObject = (TObject) EditorGUI.ObjectField(entryObjectRect, entryObject, typeof(TObject), false);

                    if (!parentList.ObjectTypeSet.IsWeightEnabled(typeId))
                    {
                        GUI.enabled = false;
                    }
                    
                    var newWeight = EditorGUI.FloatField(weightRect, weight);
                    GUI.enabled = true;
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetObject(i, newEntryObject);
                        SetWeight(i, newWeight);
                        ApplyChanges();
                    }
                    
                    UpdateSelectedIndex();
                },
                drawNoneElementCallback = rect =>
                {
                    if (parentList.SelectedIndex < 0)
                    {
                        var style = new GUIStyle(EditorStyles.label) {alignment = TextAnchor.MiddleCenter};
                        EditorGUI.LabelField(rect, $"Select a {EntryTypeNameSingular} Type", style);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, "List is Empty");
                    }
                },
                onAddCallback = list =>
                {
                    var typeId = typeIds[parentList.SelectedIndex];
                    dataSet.AddNewEntry(typeId);
                    ApplyChanges();
                    RefreshList();
                },
                onRemoveCallback = list =>
                {
                    if (parentList.DataObject != null)
                    {
                        Undo.RegisterCompleteObjectUndo(parentList.DataObject, $"Remove {EntryTypeNameSingular}");
                    }

                    var typeId = typeIds[parentList.SelectedIndex];
                    dataSet.RemoveEntry(typeId, reorderableList.index);
                    ApplyChanges();
                    RefreshList();
                },
                onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    var typeId = typeIds[parentList.SelectedIndex];
                    dataSet.MoveEntry(typeId, oldIndex, newIndex);
                    ApplyChanges();
                }
            };
        }
        
        public void Dispose()
        {
            Undo.undoRedoPerformed -= RefreshList;
        }
        
        private TObject GetObject(int index)
        {
            var orderedIds = parentList.OrderedIds;
            var typeId = orderedIds[parentList.SelectedIndex];

            return parentList.ObjectTypeSet.GetObject(typeId, index);
        }
        
        private float GetWeight(int index)
        {
            var orderedIds = parentList.OrderedIds;
            var typeId = orderedIds[parentList.SelectedIndex];

            return parentList.ObjectTypeSet.GetWeight(typeId, index);
        }

        private void SetObject(int index, TObject newObject)
        {
            var orderedIds = parentList.OrderedIds;
            var typeId = orderedIds[parentList.SelectedIndex];

            parentList.ObjectTypeSet.SetObject(typeId, index, newObject);
        }
        
        private void SetWeight(int index, float weight)
        {
            var orderedIds = parentList.OrderedIds;
            var typeId = orderedIds[parentList.SelectedIndex];

            parentList.ObjectTypeSet.SetWeight(typeId, index, weight);
        }

        private int GetEntryCount(int index)
        {
            var id = parentList.OrderedIds[index];

            return parentList.ObjectTypeSet.GetEntryCount(id);
        }
    }
}