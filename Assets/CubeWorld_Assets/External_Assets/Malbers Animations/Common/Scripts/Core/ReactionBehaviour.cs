using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    public class ReactionBehaviour : StateMachineBehaviour
    {
        [Tooltip("List of reactions to send to the animator")]
        public List<ReactionB> reactions = new();

        override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var item in reactions)
            {
                item.sent = false;
                item.target = item.reaction.VerifyComponent(anim);

                if (item.Time == 0) item.React();
                GetIgnoreTransitionHashs(item);
            }
        }



        override public void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var NextAnim = anim.GetNextAnimatorStateInfo(layer).shortNameHash;
            var InTransition = anim.IsInTransition(layer) && state.shortNameHash != NextAnim; //Check only the Exit Transition not the Start Transsition
            var time = state.normalizedTime % 1;


            foreach (var e in reactions)
            {
                if (e.sent) continue; //If the effect was already sent keep looking for the next one

                if (InTransition)
                {
                    if (e.IgnoreInTransitionHash.Contains(NextAnim))
                    {
                        e.sent = true;
                    }
                    else if (e.Time == 1 && e.ExitInTransition) //If is a quick exit transition
                    {
                        e.React();
                        return;
                    }
                }

                //Regular Update Check for the Effect
                if (time >= e.Time)
                {
                    e.React();
                }
            }
        }

        override public void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; //means is transitioning to it self

            foreach (var reaction in reactions)
            {
                if (reaction.Time == 1 && !reaction.sent)
                {
                    reaction.React();
                }
            }
        }

        private void GetIgnoreTransitionHashs(ReactionB item)
        {
            //Gather all the hashes the first time only
            if (item.IgnoreInTransitionHash == null)
            {
                item.IgnoreInTransitionHash = new List<int>();

                if (item.IgnoreInTransition != null && item.IgnoreInTransition.Count > 0)
                {
                    foreach (var hash in item.IgnoreInTransition)
                    {
                        item.IgnoreInTransitionHash.Add(Animator.StringToHash(hash));
                    }
                }
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < reactions.Count; i++)
            {
                var reac = reactions[i];
                reac.display = $"[{(reac.reaction != null ? reac.reaction.ReactionType.Name : "<NONE>")}]";

                if (reac.Time == 0)
                    reac.display += $"  -  [On Enter]";
                else if (reac.Time == 1)
                    reac.display += $"  -  [On Exit]";
                else
                    reac.display += $"  -  [OnTime] ({reac.Time:F2})";

                if (reac.ExitInTransition && reac.Time == 1) reac.display += "[In Transition]";

                reac.showExecute = reac.Time != 1 && reac.Time != 0;
                reac.showExitInTransition = reac.Time == 1;
            }
        }
    }

    [System.Serializable]
    public class ReactionB
    {
        [HideInInspector]
        public string display;
        [HideInInspector] public bool showExecute;
        [HideInInspector] public bool showExitInTransition;

        [Range(0, 1)]
        public float Time;
        [SubclassSelector, SerializeReference]
        public Reaction reaction;

        public bool sent { get; set; }
        public Component target { get; set; }

        [Tooltip("If the animation was interrupted by a transition and the Time has not played yet, execute the Reaction anyways")]
        [Hide(nameof(showExecute))]
        public bool ExecuteOnExit = true;

        [Tooltip("If the animation is interrupted, Execute the Reaction as soon as it start transition to another Animation State")]
        [Hide(nameof(showExitInTransition))]
        public bool ExitInTransition = true;

        [Tooltip("Ignore the Reaction if Execute is called in a transition for the next anim state. included in any of these State List")]
        public List<string> IgnoreInTransition = new();
        public List<int> IgnoreInTransitionHash { get; set; }


        public void React()
        {
            reaction.React(target);
            sent = true;
        }
    }
}
