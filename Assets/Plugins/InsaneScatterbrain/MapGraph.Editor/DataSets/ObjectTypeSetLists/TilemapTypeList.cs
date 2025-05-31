using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing tilemap types.
    /// </summary>
    public class TilemapTypeList : ObjectTypeSetList<TilemapType, TilemapTypeEntry, Tilemap>
    {
        private readonly TilemapTypeEntryList objectTypeEntryList;
        protected override ObjectTypeEntryList<TilemapType, TilemapTypeEntry, Tilemap> ObjectTypeEntryList => objectTypeEntryList;
        protected override string DefaultName => "New Tilemap Type";
        protected override string LabelText => "Tilemap Types";
        
        public TilemapTypeList(TilemapSet tilemapSet) : base(tilemapSet)
        {
            objectTypeEntryList = new TilemapTypeEntryList(this);
        }
        
        protected override TilemapType New(string name)
        {
            return new TilemapType(name);
        }
    }
}