using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check InZone", order = 1)]
    public class CheckInZone : MAIDecision
    {
        public override string DisplayName => "Animal/Check in Zone";

        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target)")]
        public Affected check = Affected.Self;
       
        public override bool Decide(MAnimalBrain brain,int index)
        {
            return check switch
            {
                Affected.Self => brain.Animal.InZone,
                Affected.Target => brain.TargetAnimal != null && brain.TargetAnimal.InZone,
                _ => false,
            };
        } 
    }
}