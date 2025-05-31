using System.Collections.Generic;
using InsaneScatterbrain.Editor.Updates;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Updater to perform all update actions for Map Graph.
    /// </summary>
    public class MapGraphUpdater : Updater
    {
        protected override IEnumerable<UpdateAction> UpdateActions => new UpdateAction[]
        {
            new UpdateAction_1_2(this),
            new UpdateAction_1_10(this),
            new UpdateAction_1_14(this),
            new UpdateAction_1_15(this),
            new UpdateAction_1_26_1(this)
        };

        protected override string ProgressBarTitle => "Map Graph Update";
        protected override string ProgressBarText => "Map Graph is applying changes. This may take a while.";
    }
}