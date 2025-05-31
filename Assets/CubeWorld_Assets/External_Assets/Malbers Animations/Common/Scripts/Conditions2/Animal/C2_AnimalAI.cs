using MalbersAnimations.Controller.AI;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddComponentMenu("Malbers/Animal Controller/Conditions/Animal AI")]
    [AddTypeMenu("Animal/Animal AI")]
    public class C2_AnimalAI : ConditionCore//<MAnimalAIControl>
    {
        [RequiredField, Hide(nameof(LocalTarget))]
        public MAnimalAIControl Target;

        public enum AnimalAICondition { enabled, HasTarget, HasNextTarget, Arrived, Waiting, InOffMesh, CurrentTarget, NextTarget }
        public AnimalAICondition Condition;
        [Hide("Condition", 6, 7)]
        public TransformReference Value;

        protected override bool _Evaluate()
        {
            if (Target)
            {
                switch (Condition)
                {
                    case AnimalAICondition.enabled: return Target.enabled;
                    case AnimalAICondition.HasTarget: return Target.Target != null;
                    case AnimalAICondition.HasNextTarget: return Target.NextTarget != null;
                    case AnimalAICondition.Arrived: return Target.HasArrived;
                    case AnimalAICondition.InOffMesh: return Target.InOffMeshLink;
                    case AnimalAICondition.CurrentTarget: return Target.Target == Value.Value;
                    case AnimalAICondition.Waiting: return Target.IsWaiting;
                    case AnimalAICondition.NextTarget: return Target.NextTarget == Value.Value;
                }
            }
            return false;
        }

        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);

    }
}
