using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]

    [AddTypeMenu("Tools/Aim")]
    public class AimReaction : Reaction
    {
        [Tooltip("Set a new Target to the Aim Component. If left empty, it will clear the target")]
        public GameObjectReference NewTarget = new();

        public override Type ReactionType => typeof(Aim);

        protected override bool _TryReact(Component reactor)
        {
            if (reactor is Aim aimer)
            {
                if (NewTarget.Value)
                {
                    aimer.SetTarget(NewTarget.Value);   
                }
                else
                {
                    aimer.ClearTarget();
                }
            }
            return true;
        }
    }
}