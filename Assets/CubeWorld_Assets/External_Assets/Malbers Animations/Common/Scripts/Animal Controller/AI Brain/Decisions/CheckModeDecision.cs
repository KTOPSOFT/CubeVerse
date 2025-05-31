using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    public enum PlayingModeState { Enter, Exit, PlayingMode }

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Mode", order = 2)]
    public class CheckModeDecision : MAIDecision
    {

        public override string DisplayName => "Animal/Check Mode";

        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected checkOn = Affected.Self;
        [Tooltip("Check if the Mode is Entering or Exiting")]
        public PlayingModeState ModeState = PlayingModeState.Enter;

        [Hide("ModeState", true, (int)PlayingModeState.PlayingMode)]
        public ModeID ModeID;
        [Tooltip("Which ability is playing in the Mode. If is set to less or equal to zero; then it will return true if the Mode Playing")]
        public IntReference Ability = new();


        //public override void PrepareDecision(MAnimalBrain brain, int Index)
        //{
        //    //Store if the animal was playing a mode
        //    if (ModeState == PlayingModeState.Exit)
        //    {
        //        var wasPlayingMode = checkOn == Affected.Target && brain.TargetAnimal != null ? brain.TargetAnimal.IsPlayingMode : brain.Animal.IsPlayingMode;
        //        var LastMode = checkOn == Affected.Target && brain.TargetAnimal != null ? brain.TargetAnimal.LastModeID : brain.Animal.LastModeID;

        //       if (!wasPlayingMode && LastMode == ModeID)

        //        brain.DecisionsVars[Index].intValue = checkOn == Affected.Target && brain.TargetAnimal != null ? brain.TargetAnimal. : brain.Animal.IsPlayingMode;
        //    }
        //}

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            return checkOn switch
            {
                Affected.Self => AnimalMode(brain.Animal),
                Affected.Target => AnimalMode(brain.TargetAnimal),
                _ => false,
            };
        }

        private bool AnimalMode(MAnimal animal)
        {
            if (animal == null) return false;

            return ModeState switch
            {
                PlayingModeState.Enter => OnEnterMode(animal),
                PlayingModeState.Exit => OnExitMode(animal),
                PlayingModeState.PlayingMode => animal.IsPlayingMode,
                _ => false,
            };
        }

        private bool OnEnterMode(MAnimal animal)
        {
            if (animal.ActiveModeID == ModeID)
            {
                if (Ability <= 0)
                    return true; //Means that Is playing a random mode does not mater which one
                else
                    return Ability == (animal.ModeAbility % 1000); //Return if the Ability is playing 
            }
            return false;
        }


        private bool OnExitMode(MAnimal animal)
        {
            if (animal.LastMode != null && animal.LastMode.ID == ModeID)
            {
                if (Ability <= 0)
                    return true; //Means that Is playing a random mode does not mater which one
                else
                    return Ability == (animal.LastMode.AbilityIndex); //Return if the Ability is playing 
            }
            return false;
        }
    }
}
