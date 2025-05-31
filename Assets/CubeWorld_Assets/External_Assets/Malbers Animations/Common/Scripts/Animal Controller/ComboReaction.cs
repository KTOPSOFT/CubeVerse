using MalbersAnimations.Controller;
using System;
using UnityEngine;


namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Damage/Play Combo")]
    public class ComboReaction : Reaction
    {
        [Tooltip("Branch to Play on the Combo")]
        public int Branch;

        public override Type ReactionType => typeof(ComboManager);

        protected override bool _TryReact(Component component)
        {
            (component as ComboManager).Play(Branch);

            return true;
        }
    }
}
