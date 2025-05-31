using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Stats/Stat Modify")]

    public class StatReaction : Reaction
    {
        public List<StatModifier> modifiers = new() { new StatModifier() };

        public override System.Type ReactionType => typeof(Stats);

        protected override bool _TryReact(Component reactor)
        {
            var stats = reactor as Stats;

            foreach (var modifier in modifiers)
            {
                modifier.ModifyStat(stats);
            }

            return true;
        }
    }
}
