using MalbersAnimations.Controller.AI;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddComponentMenu("Malbers/Animal Controller/Conditions/Animal AI")]
    [AddTypeMenu("Animal/Animal AI")]
    public class C_AnimalAI : MCondition
    {
        [RequiredField] public MAnimalAIControl AI;
        public enum AnimalAICondition { enabled, HasTarget, HasNextTarget, Arrived, Waiting, InOffMesh, CurrentTarget, NextTarget }
        public AnimalAICondition Condition;
        [Hide("Condition", 6, 7)]
        public TransformReference Target;

        public override bool _Evaluate()
        {
            if (AI)
            {
                switch (Condition)
                {
                    case AnimalAICondition.enabled: return AI.enabled;
                    case AnimalAICondition.HasTarget: return AI.Target != null;
                    case AnimalAICondition.HasNextTarget: return AI.NextTarget != null;
                    case AnimalAICondition.Arrived: return AI.HasArrived;
                    case AnimalAICondition.InOffMesh: return AI.InOffMeshLink;
                    case AnimalAICondition.CurrentTarget: return AI.Target == Target.Value;
                    case AnimalAICondition.Waiting: return AI.IsWaiting;
                    case AnimalAICondition.NextTarget: return AI.NextTarget == Target.Value;
                }
            }
            return false;
        }

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref AI);

    }
}
