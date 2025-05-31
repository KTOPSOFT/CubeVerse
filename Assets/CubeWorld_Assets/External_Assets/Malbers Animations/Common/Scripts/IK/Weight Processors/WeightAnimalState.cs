using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking the Look At Angle of the Animator / </summary>
    [System.Serializable]
    [AddTypeMenu("Animal/Weight State")]
    public class WeightAnimalState : WeightProcessor
    {
        [Tooltip("States to check if the animal is on any of them Weight will be set to 1")]
        public List<StateID> States = new();
        [Tooltip("Profile to check on the State, by default is zero. Ignore if is -1")]
        public IntReference Profile = new();

        [Tooltip("Exclude these states. Meaning if the Character is NOT on these states then the weight is set to 1")]
        public bool exclude = false;


        private MAnimal character;

        private float StateWeight = 0;
        public override void OnEnable(IKSet set, Animator anim)
        {
            if (anim.TryGetComponent(out character))
            {
                character.OnState += OnState;
            }
            else
            {
                Active = false;
                Debug.LogWarning("The State weight processor requires an Animal Controller. Disabling it");
            }
        }

        public override void OnDisable(IKSet set, Animator anim)
        {
            if (character != null) character.OnState -= OnState;
        }

        private void OnState(int newState)
        {
            StateWeight = States.Contains(character.ActiveStateID) ? 1 : 0;
            if (exclude) StateWeight = 1 - StateWeight;

            //Check also the profile of the state
            if (Profile >= 0) StateWeight *= (character.activeState.StateProfile == Profile) ? 1 : 0;
        }

        public override float Process(IKSet set, float weight) => weight * StateWeight;
    }
}
