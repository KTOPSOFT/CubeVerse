using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing tilemap type entries.
    /// </summary>
    public class TilemapTypeEntryList : ObjectTypeEntryList<TilemapType, TilemapTypeEntry, Tilemap>
    {
        protected override string EntryTypeNameSingular => "Tilemap";
        protected override string EntryTypeNamePlural => "Tilemaps";

        public TilemapTypeEntryList(TilemapTypeList parentList) : base(parentList) { }
    }
}