using System;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Stats/Stat (Enable-Disable)")]
    public class StatEnableReaction : Reaction
    {
        public IDEnable<StatID>[] stats;

        public override Type ReactionType => typeof(Stats);

        protected override bool _TryReact(Component component)
        {
            var statM = component as Stats;

            foreach (var id in stats)
            {
                statM.Stat_Get(id.ID)?.SetActive(id.enable);
            }
            return true;
        }
    }
}
