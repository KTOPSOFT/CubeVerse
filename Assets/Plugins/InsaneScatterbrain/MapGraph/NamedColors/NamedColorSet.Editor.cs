#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InsaneScatterbrain.MapGraph
{
    public partial class NamedColorSet
    {
        private readonly Dictionary<IDataSet, Action<string>> onAddedActions = new Dictionary<IDataSet, Action<string>>();
        private readonly Dictionary<IDataSet, Action<string, string>> onRemovedActions = new Dictionary<IDataSet, Action<string, string>>();
        private readonly Dictionary<IDataSet, Action<string, string, string>> onRenamedActions = new Dictionary<IDataSet, Action<string, string, string>>();
        
        private readonly List<IDataSet> linkedSets = new List<IDataSet>();
        public ReadOnlyCollection<IDataSet> LinkedSets => linkedSets.AsReadOnly();
        
        private Dictionary<string, List<IDataSet>> linkedSetsPerColorId = new Dictionary<string, List<IDataSet>>();
        
        [SerializeField] private List<string> manuallyCreatedColorIds = new List<string>(); 
        
        public void MarkManuallyCreated(string colorId)
        {
            manuallyCreatedColorIds.Add(colorId);
        }
        
        public void UnmarkManuallyCreated(string colorId)
        {
            manuallyCreatedColorIds.Add(colorId);
        }
        
        public bool IsManuallyCreated(string colorId)
        {
            return manuallyCreatedColorIds.Contains(colorId);
        }
        
        private void OnEnable()
        {
            linkedSetsPerColorId = new Dictionary<string, List<IDataSet>>();
            
            foreach (var set in linkedSets)
            {
                InitializeSetLink(set);
            }
        }

        private void InitializeSetLink(IDataSet set)
        {
            foreach (var itemId in set.OrderedIds)
            {
                LinkedSetItemAdded(itemId, set);
            }

            // Store the actions in a dictionary, so we can use those to unsubscribe when unlinking.
            onAddedActions.Add(set, itemId => LinkedSetItemAdded(itemId, set)); 
            onRemovedActions.Add(set, (itemId, removedItemName) => LinkedSetItemRemoved(removedItemName, set));
            onRenamedActions.Add(set, (itemId, oldItemName, newItemName) => LinkedSetItemRenamed(itemId, oldItemName, newItemName, set));
            
            set.OnAdded += onAddedActions[set];
            set.OnRemoved += onRemovedActions[set];
            set.OnRenamed += onRenamedActions[set]; 
        }
        
        /// <summary>
        /// Gets whether the named color set is linked to the given dataset.
        /// </summary>
        /// <param name="set">The dataset.</param>
        /// <returns>True if they're linked, false otherwise.</returns>
        public bool IsLinked(IDataSet set)
        {
            return linkedSets.Contains(set);
        }
        
        /// <summary>
        /// Links the named color set to the given dataset.
        /// </summary>
        /// <param name="set"></param>
        public void Link(IDataSet set)
        {
            linkedSets.Add(set);
            InitializeSetLink(set);
        }
        
        /// <summary>
        /// Unlinks the named color set from the given dataset.
        /// </summary>
        /// <param name="set"></param>
        public void Unlink(IDataSet set)
        {
            foreach (var itemName in set.Names)
            {
                var colorId = GetId(itemName);
                UnlinkColor(colorId, set); 
            }
            
            linkedSets.Remove(set); 
                
            set.OnAdded -= onAddedActions[set];
            set.OnRemoved -= onRemovedActions[set];
            set.OnRenamed -= onRenamedActions[set];

            onAddedActions.Remove(set);
            onRemovedActions.Remove(set);
            onRenamedActions.Remove(set);
        }

        /// <summary>
        /// Callback for whenever a new item is added to a linked set.
        /// </summary>
        /// <param name="itemId">The new item's ID.</param>
        /// <param name="set">The linked set.</param>
        private void LinkedSetItemAdded(string itemId, IDataSet set)
        {
            if (this == null)
            {
                // This set has been destroyed. So we can't update it anymore.
                return;
            }
            
            var newColorName = set.GetName(itemId);

            if (ContainsName(newColorName))
            {
                // A color with this name already exists in the named color set. So we only need to add a link
                // to the given set.
                var colorId = GetId(newColorName);
                LinkColor(colorId, set); 
                return;
            }

            // Otherwise generate a unique color for this name.
            Color32 color;
            do
            {
                color = Random.ColorHSV();
            } 
            while (ContainsColor(color));

            // Pair the name with the color, add it to the named color set and store the link with the dataset.
            var namedColor = new NamedColor(newColorName, color);
            Add(namedColor);
            LinkColor(namedColor.Id, set);
            
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Callback for whenever an item gets removed from a linked set.
        /// </summary>
        /// <param name="removedName">The removed item's name.</param>
        /// <param name="set">The linked set.</param>
        private void LinkedSetItemRemoved(string removedName, IDataSet set)
        {
            if (this == null)
            {
                // This set has been destroyed. So we can't update it anymore.
                return;
            }
            
            var colorId = GetId(removedName);
            UnlinkColor(colorId, set);
            
            if (!linkedSetsPerColorId.ContainsKey(colorId) && !IsManuallyCreated(colorId))
            {
                // The color is not linked with any other sets than the given one. So it can be safely removed,
                // unless it was manually created.
                Remove(colorId);
            }
            
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Callback for whenever an item gets renamed in a linked set.
        /// </summary>
        /// <param name="itemId">The renamed item's name.</param>
        /// <param name="oldName">The item's previous name.</param>
        /// <param name="newName">The item's new name.</param>
        /// <param name="set">The linked set.</param>
        private void LinkedSetItemRenamed(string itemId, string oldName, string newName, IDataSet set)
        {
            if (this == null)
            {
                // This set has been destroyed. So we can't update it anymore.
                return;
            }
            
            var colorId = GetId(oldName);
            
            if (linkedSetsPerColorId[colorId].Count > 1)
            {
                // There are other linked sets that contain an item that has the same (old) name. So instead of renaming,
                // a new entry should be added under the new name, as not to break the color link with the other linked sets.
                UnlinkColor(colorId, set);
                LinkedSetItemAdded(itemId, set);
            }
            else if (ContainsName(newName))
            {
                // If the new name is already in use, we can't rename the color. Instead, we link the set entry to the existing color.
                var newColorId = GetId(newName);
                LinkColor(newColorId, set);
                
                // And remove the old color.
                LinkedSetItemRemoved(oldName, set);
            }
            else
            {
                // Otherwise we can just rename it.
                Rename(colorId, newName);
            }
            
            EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// Returns whether the given color ID is linked to any datasets.
        /// </summary>
        /// <param name="colorId">The color's ID.</param>
        /// <returns>True if the color is linked to one or more datasets, false otherwise.</returns>
        public bool IsColorLinked(string colorId)
        {
            return linkedSetsPerColorId.ContainsKey(colorId);
        }
        
        /// <summary>
        /// Gets all the datasets that the given color ID is linked to.
        /// </summary>
        /// <param name="colorId">The color ID.</param>
        /// <returns>Readonly set of linked datasets. Null if there are none.</returns>
        public IEnumerable<IDataSet> GetColorLinkedSets(string colorId)
        {
            if (!linkedSetsPerColorId.ContainsKey(colorId)) return null;
            
            return linkedSetsPerColorId[colorId].AsReadOnly();
        }
        
        /// <summary>
        /// Links the color ID to the given dataset.
        /// </summary>
        /// <param name="colorId">The color's ID.</param>
        /// <param name="set">The set to link to.</param>
        private void LinkColor(string colorId, IDataSet set)
        {
            if (!linkedSetsPerColorId.ContainsKey(colorId))
            {
                // If this color is not linked to any sets yet, create a list for it first.
                linkedSetsPerColorId.Add(colorId, new List<IDataSet>());
            }

            linkedSetsPerColorId[colorId].Add(set);
        }

        /// <summary>
        /// Unlinks the color ID from the given dataset.
        /// </summary>
        /// <param name="colorId">The color's ID.</param>
        /// <param name="set">The linked set.</param>
        private void UnlinkColor(string colorId, IDataSet set)
        {
            linkedSetsPerColorId[colorId].Remove(set);

            if (linkedSetsPerColorId[colorId].Count == 0)
            {
                // If this color is no longer linked to any sets, the list can be removed.
                linkedSetsPerColorId.Remove(colorId);
            }
        }
        
        /// <summary>
        /// Unlinks any sets that no longer exist. 
        /// </summary>
        public void Update()
        {
            var destroyedSets = linkedSets.Where(set => set.Equals(null)).ToList();

            foreach (var destroyedSet in destroyedSets)
            {
                Unlink(destroyedSet);
            }
        }

        #region Serialization

        [SerializeReference] private List<Object> serializedLinkedSets = new List<Object>();
        
        private void OnBeforeSerializeEditor()
        {
            // Store the linked sets as Objects so Unity can serialize them.
            serializedLinkedSets.Clear();
            foreach (Object obj in linkedSets)
            {
                serializedLinkedSets.Add(obj);
            }
        }

        private void OnAfterDeserializeEditor()
        {
            // Convert serialized linked lists objects back to datasets.
            linkedSets.Clear();
            foreach (IDataSet obj in serializedLinkedSets)
            {
                linkedSets.Add(obj);
            }
        }

        #endregion
    }
}

#endif