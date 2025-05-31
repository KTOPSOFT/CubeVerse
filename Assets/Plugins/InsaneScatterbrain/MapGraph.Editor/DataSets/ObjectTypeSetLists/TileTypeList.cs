using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Editor list for displaying and editing tile types.
    /// </summary>
    public class TileTypeList : ObjectTypeSetList<TileType, TileTypeEntry, TileBase>
    {
        private readonly TileTypeEntryList tileTypeEntryList;
        protected override ObjectTypeEntryList<TileType, TileTypeEntry, TileBase> ObjectTypeEntryList => tileTypeEntryList;
        protected override string DefaultName => "New Tile Type";
        protected override string LabelText => "Tile Types";
        
        public TileTypeList(Tileset tileset) : base(tileset)
        {
            tileTypeEntryList = new TileTypeEntryList(this);
        }
        
        protected override TileType New(string name)
        {
            return new TileType(name);
        }
    }
}