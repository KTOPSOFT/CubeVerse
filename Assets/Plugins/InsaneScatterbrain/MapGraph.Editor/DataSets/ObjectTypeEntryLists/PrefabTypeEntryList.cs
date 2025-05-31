using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing prefab type entries.
    /// </summary>
    public class PrefabTypeEntryList : ObjectTypeEntryList<PrefabType, PrefabTypeEntry, GameObject>
    {
        protected override string EntryTypeNameSingular => "Prefab";
        protected override string EntryTypeNamePlural => "Prefabs";

        public PrefabTypeEntryList(PrefabTypeList parentList) : base(parentList) { }
    }
}