
using System;

namespace MalbersAnimations.Reactions
{
    /// <summary> Animal Parent Reaction Class  </summary>
    [System.Serializable]
    public abstract class MReaction : Reaction
    {
        public override Type ReactionType => typeof(Controller.MAnimal);
    }
}