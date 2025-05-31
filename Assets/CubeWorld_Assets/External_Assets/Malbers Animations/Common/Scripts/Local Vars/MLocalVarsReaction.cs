using System;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Variables/Local Variable")]
    public class MLocalVarsReaction : Reactions.Reaction
    {
        [Header("Variable Name")]
        public LocalVar var;
        public override Type ReactionType => typeof(MLocalVars);

        protected override bool _TryReact(Component reactor)
        {
            var m = reactor as MLocalVars;

            if (m.HasVar(var))
            {
                m.SetVar(var);
                return true;
            }
            return false;

        }
    }
}