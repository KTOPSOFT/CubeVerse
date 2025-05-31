using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/RandomChance", order = 7)]
    public class RandomChanceDecision : MAIDecision
    {
        [Tooltip("Chance at which the decision will apply")]
        [Range(0, 1)]
        public float chance = 0.5f;

        public override string DisplayName => "General/Random Chance";

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            return Random.value < chance;
        }


        private void Reset()
        {
            Description = "Evaluate the chance to apply the decision. Use the Interval value to delay the decision";
        }
    }
}