using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Is Target too High",order = -100)]
    public class TargetTooHighDecision : MAIDecision
    {
        public override string DisplayName => "Movement/Is Target too High";

        public override bool Decide(MAnimalBrain brain, int index)
        {
            if (brain.AIControl.Target != null)
            {
                return (brain.AIControl as MAnimalAIControl).TargetTooHigh;
            }
            return false;
        }
    }
}