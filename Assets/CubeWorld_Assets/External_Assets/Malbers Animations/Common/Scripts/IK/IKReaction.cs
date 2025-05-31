using MalbersAnimations.Reactions;
using System;
using UnityEngine;

namespace MalbersAnimations.IK
{

    [AddTypeMenu("Malbers/IK")]
    public class IKReaction : Reaction
    {
        public override Type ReactionType => typeof(IIKSource);
        public enum IKReactionType { Activate, Deactivate, SetTargets, ClearTargets }

        public string IKSet = "IKSetName";
        public IKReactionType action = IKReactionType.Activate;

        public Transform[] targets;

        protected override bool _TryReact(Component reactor)
        {
            IIKSource source = reactor as IIKSource;

            switch (action)
            {
                case IKReactionType.Activate:
                    source.Set_Enable(IKSet);
                    break;
                case IKReactionType.Deactivate:
                    source.Set_Disable(IKSet);
                    break;
                case IKReactionType.SetTargets:
                    source.Target_Set(IKSet, targets);
                    break;
                case IKReactionType.ClearTargets:
                    source.Target_Clear(IKSet);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}