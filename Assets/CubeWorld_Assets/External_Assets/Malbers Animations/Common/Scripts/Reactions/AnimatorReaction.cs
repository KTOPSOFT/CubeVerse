using MalbersAnimations.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]

    [AddTypeMenu("Unity/Animator SetParameter")]

    public class AnimatorReaction : Reaction
    {
        public override System.Type ReactionType => typeof(Animator);

        public List<MAnimatorParameter> parameters = new();

        public void Set(Animator anim)
        {
            foreach (var param in parameters)
                param.Set(anim);
        }

        protected override bool _TryReact(Component component)
        {
            Set(component as Animator);
            return true;
        }
    }
}
