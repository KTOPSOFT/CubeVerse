using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing tile type entries.
    /// </summary>
    public class TileTypeEntryList : ObjectTypeEntryList<TileType, TileTypeEntry, TileBase>
    {
        protected override string EntryTypeNameSingular => "Tile";

        protected override string EntryTypeNamePlural => "Tiles";

        public TileTypeEntryList(TileTypeList parentList) : base(parentList) { }
    }
}