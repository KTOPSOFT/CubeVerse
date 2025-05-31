using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("General/Check Distance")]
    [ConditionDescription("Check Distance from [Target] to [Target2]")]
    public class C2_Distance : ConditionCore
    {
        protected override void _SetTarget(Object target) => Target.Value = MTools.VerifyComponent(target, Target.Value);

        [Tooltip("Target to check for the condition")]
        [Hide(nameof(LocalTarget))] public GameObjectReference Target = new();

        public GameObjectReference Target2 = new();
        public ComparerInt Condition = ComparerInt.Less;

        public FloatReference Distance = new(5);

        protected override bool _Evaluate()
        {
            if (Target == null) return false;
            return Vector3.Distance(Target.Value.transform.position, Target2.Value.transform.position).CompareFloat(Distance.Value, Condition);

        }
    }
}
