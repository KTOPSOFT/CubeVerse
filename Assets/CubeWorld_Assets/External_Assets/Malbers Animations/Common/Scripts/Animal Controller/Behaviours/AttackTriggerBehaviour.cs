using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Attack Trigger")]
    public class AttackTriggerBehaviour : StateMachineBehaviour
    {
        [Tooltip("0: Disable All Attack Triggers\n-1: Enable All Attack Triggers\nx: Enable the Attack Trigger by its index")]
        public int AttackTrigger = 1;                           //ID of the Attack Trigger to Enable/Disable during the Attack Animation
        [Tooltip("Profile to activate on the Damager")]
        [Min(0)]public int Profile = 0;                           

        [Tooltip("Range on the Animation that the Attack Trigger will be Active")]
        [MinMaxRange(0, 1)]
        public RangedFloat AttackActivation = new(0.3f, 0.6f);
       
        private bool isOn, isOff;
        private IMDamagerSet[] damagers;


        override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            isOn = isOff = false;                                   //Reset the ON/OFF parameters (to be used on the Range of the animation

            if (damagers == null) damagers = anim.GetComponents<IMDamagerSet>();

            //Let know everybody that an attack trigger animation has started (Needed for the Weapon Manager)
            if (damagers != null)
                foreach (var d in damagers)
                    d.DamagerAnimationStart(stateInfo.fullPathHash);
        }

        override public void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;

            if (!isOn && (time >= AttackActivation.minValue))
            {
                foreach (var d in damagers) d.ActivateDamager(AttackTrigger,Profile);
                isOn = true;
            }

            if (!isOff && (time >= AttackActivation.maxValue))
            {
                //means is transitioning to it self so do skip sending off the 
                if (anim.IsInTransition(layer) && anim.GetNextAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return;

                foreach (var d in damagers) d.ActivateDamager(0, Profile);
                isOff = true;
            }
        }

        override public void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; //means is transitioning to it self

            if (!isOff)
                foreach (var d in damagers) d.ActivateDamager(0, Profile);  //Double check that the Trigger is OFF


            isOn = isOff = false;                                               //Reset the ON/OFF variables

            //Let know everybody that an attack trigger animation has started (Needed for the Weapon Manager)
            if (damagers != null)
                foreach (var d in damagers)
                    d.DamagerAnimationEnd(state.fullPathHash);
        }
    }
}