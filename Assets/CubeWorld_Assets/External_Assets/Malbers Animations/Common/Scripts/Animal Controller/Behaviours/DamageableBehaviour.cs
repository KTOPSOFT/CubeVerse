using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class DamageableBehaviour : StateMachineBehaviour
    {
        private IMDamage damageable;
        public List<DamageableProfile> DamageProfile;

        override public void OnStateEnter(Animator anim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (damageable == null) damageable = anim.GetComponent<IMDamage>();

            foreach (var item in DamageProfile)
            {
                item.isOff = false;
                item.isOn = false;

                if (item.ProfileActivation.minValue == 0)
                {
                    damageable.Profile_Set(item.Profile);
                    item.isOn = true;
                }
            }
        }

        override public void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;

            foreach (var d in DamageProfile)
            {
                if (!d.isOn && (time >= d.ProfileActivation.minValue))
                {
                    damageable.Profile_Set(d.Profile);
                    d.isOn = true;
                    continue;
                }

                if (!d.isOff && (time >= d.ProfileActivation.maxValue))
                {
                    //means is transitioning to it self so do not send 
                    if (anim.IsInTransition(layer) && anim.GetNextAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; 
                    
                    d.isOff = true;
                    damageable.Profile_Restore();
                }
            }
        }

        override public void OnStateExit(Animator anim, AnimatorStateInfo state, int layer)
        {
            if (anim.GetCurrentAnimatorStateInfo(layer).fullPathHash == state.fullPathHash) return; //means is transitioning to it self

            foreach (var p in DamageProfile)
            {
                if (!p.isOff)
                    damageable.Profile_Restore();

                p.isOn = p.isOff = false;                                               //Reset the ON/OFF variables
            }
        }


        private void OnValidate()
        {
            foreach (var p in DamageProfile)
            {
                p.display = $"Profile [{p.Profile.Value}] → ({p.ProfileActivation.minValue}) - ({p.ProfileActivation.maxValue})";
            }
        }

        private void Reset()
        {
            DamageProfile = new List<DamageableProfile>() 
            { 
                new DamageableProfile()
                {  Profile = new StringReference("Default"), ProfileActivation = new RangedFloat(0.3f,0.6f)}
            };

            OnValidate();
        }
    }


    

    [System.Serializable]
    public class DamageableProfile
    {
        [HideInInspector] public string display;

        [Tooltip("Profile to activate on the Damager")]
        public StringReference Profile =  new();

        [Tooltip("Range on the Animation that the Attack Trigger will be Active")]
        [MinMaxRange(0, 1)]
        public RangedFloat ProfileActivation;
        public bool isOn { get; set; }
        public bool isOff { get; set; }
    }
}