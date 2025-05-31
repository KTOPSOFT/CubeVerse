using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// The default list to use in the editor for object type set lists.
    /// </summary>
    /// <typeparam name="TType">The type of object type.</typeparam>
    /// <typeparam name="TEntry">The entry type.</typeparam>
    /// <typeparam name="TObject">The entry's object type.</typeparam>
    public abstract class ObjectTypeSetList<TType, TEntry, TObject> : DataSetList<TType> 
        where TType : IObjectType<TEntry, TObject>
        where TObject : Object
        where TEntry : IObjectTypeEntry<TObject>
    {
        /// <summary>
        /// Gets the object type set.
        /// </summary>
        public IObjectTypeSet<TType, TObject> ObjectTypeSet { get; }
        
        protected abstract ObjectTypeEntryList<TType, TEntry, TObject> ObjectTypeEntryList { get; }

        protected ObjectTypeSetList(IObjectTypeSet<TType, TObject> objectTypeSet) : base(objectTypeSet)
        {
            ObjectTypeSet = objectTypeSet;
            
            OnSelect += id =>
            {
                if (ReorderableList.index < 0) return;

                ObjectTypeEntryList.RefreshList();
            };

            OnRemove += id =>
            {
                if (ReorderableList.index <= -1) return;

                ObjectTypeEntryList.RefreshList();
            };
        }

        protected override int AdditionalFieldsWidth => 0;

        protected override void DrawAdditionalFields(Rect rect, string id)
        {
            
        }

        /// <summary>
        /// Draws list.
        /// </summary>
        public override void DoLayoutList()
        {
            // If the list is the only element in the editor and there are no entries yet, it will result in an unresponsive
            // text field when creating the first entry, at least until the asset is unselected in the project window
            // and selected again. (Issue #245)
            // Workaround:
            // Adding an invisible field here so that the list is never alone in the editor.
            // The field is always disabled so it can't be tabbed into.
            var enabled = GUI.enabled;
            GUI.enabled = false;
            GUILayout.TextField(null, GUILayout.Height(0), GUILayout.Width(0));
            GUI.enabled = enabled;
            
            base.DoLayoutList();
            
            GUILayout.Space(20);

            ObjectTypeEntryList.DoLayoutList();
        }

        public override void Dispose()
        {
            base.Dispose();
            ObjectTypeEntryList.Dispose();
            ObjectTypeSet.Clean();
        }

        public void ApplyChanges()
        {
            SaveAsset();
        }
    }
}