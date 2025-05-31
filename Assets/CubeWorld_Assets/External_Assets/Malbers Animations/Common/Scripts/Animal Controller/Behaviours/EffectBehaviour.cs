using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Effects")]
    public class EffectBehaviour : StateMachineBehaviour
    {
        [Tooltip("Re-check If there any new Effect Manager on the Character (Use this when the Weapons are added or Removed and you want to add effect to them")]
        public bool DynamicManagers = false;
        public List<EffectItem> effects = new();
        private EffectManager[] effectManagers = null;
        private bool HasEfM = false;

        override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (effectManagers == null || DynamicManagers)
            {
                HasEfM = false;
                effectManagers = anim.GetComponentsInChildren<EffectManager>();

                if (effectManagers != null && effectManagers.Length > 0) HasEfM = true;
            }

            //Reset that the messages were sent. Always on State Enter
            if (HasEfM)
            {
                foreach (var item in effects)
                {
                    item.sent = false;


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
            }
        }

        override public void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (HasEfM)
            {
                var NextAnim = anim.GetNextAnimatorStateInfo(layer).shortNameHash;
                var InTransition = anim.IsInTransition(layer) && state.shortNameHash != NextAnim; //Check only the Exit Transition not the Start Transsition
                var time = state.normalizedTime % 1;

                foreach (var e in effects)
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
                            Execute(e);
                            return;
                        }
                    }

                    //Regular Update Check for the Effect
                    if (time >= e.Time)
                    {
                        Execute(e);
                    }
                }
            }
        }



        override public void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (HasEfM)
            {
                //means is transitioning to it self
                if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return;

                foreach (var e in effects)
                {
                    if (e.sent) continue; //If the effect was already sent keep looking for the next one

                    //Ignore the Effect action if the Next Animation is on the list
                    if (e.IgnoreInTransitionHash.Contains(anim.GetCurrentAnimatorStateInfo(layer).shortNameHash))
                    {
                        return;
                    }


                    if (e.Time == 1 || (e.ExecuteOnExit && !e.sent))
                    {
                        Execute(e);
                    }
                }
            }
        }


        private void Execute(EffectItem e)
        {
            foreach (var EM in effectManagers)
            {
                if (e.action == EffectOption.Play)
                    EM.PlayEffect(e.ID);
                else
                    EM.StopEffect(e.ID);
            }
            e.sent = true;
        }

        //Fake Inspector
        private void OnValidate()
        {
            foreach (var item in effects)
            {
                item.name = $"{item.action} Effect [{item.ID}]";

                if (item.Time == 0)
                    item.name += $"  -  [On Enter]";
                else if (item.Time == 1)
                    item.name += $"  -  [On Exit]";
                else
                    item.name += $"  -  [OnTime] ({item.Time:F2})";

                if (item.ExitInTransition && item.Time == 1) item.name += " - [In Transition]";

                if (item.IgnoreInTransition.Count > 0) item.name += $" - [Ignore: {item.IgnoreInTransition.Count}]";

                item.showExecute = item.Time != 1 && item.Time != 0;
                item.showExitInTransition = item.Time == 1;
            }
        }
    }




    [System.Serializable]
    public class EffectItem
    {
        [HideInInspector] public string name;
        [HideInInspector] public bool showExecute;
        [HideInInspector] public bool showExitInTransition;
        [Tooltip("ID of the Effect")]
        public int ID = 1;                           //ID of the Attack Trigger to Enable/Disable during the Attack Animation
        public EffectOption action = EffectOption.Play;
        [Range(0, 1)]
        public float Time = 0;

        [Tooltip("If the animation is interrupted by a transition and the time has not played yet, execute the Effect anyways")]
        [Hide(nameof(showExecute))]
        public bool ExecuteOnExit = true;

        [Tooltip("If the animation is interrupted, Execute the Effect as soon as it start transition to another Animation State")]
        [Hide(nameof(showExitInTransition))]
        public bool ExitInTransition = true;

        [Tooltip("Ignore the effect if execute is called in Transition and the next transition is this list. Use the name of the Animation State")]
        public List<string> IgnoreInTransition = new List<string>();
        public List<int> IgnoreInTransitionHash { get; set; }

        public bool sent { get; set; }
    }

    public enum EffectOption { Play, Stop }
}