using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing prefab types.
    /// </summary>
    public class PrefabTypeList : ObjectTypeSetList<PrefabType, PrefabTypeEntry, GameObject>
    {
        private readonly PrefabTypeEntryList entryList;
        protected override ObjectTypeEntryList<PrefabType, PrefabTypeEntry, GameObject> ObjectTypeEntryList => entryList;
        protected override string DefaultName => "New Prefab Type";
        protected override string LabelText => "Prefab Types";
        
        public PrefabTypeList(PrefabSet prefabSet) : base(prefabSet)
        {
            entryList = new PrefabTypeEntryList(this);
        }
        
        protected override PrefabType New(string name)
        {
            return new PrefabType(name);
        }
    }
}