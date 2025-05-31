using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("General/Malbers Tag")]
    public class C2_MalbersTag : ConditionCore
    {
        public enum MalbersTagCondition { HasTag, HasTagInParent }

        [Tooltip("Target to check for the condition ")]
        [Hide(nameof(LocalTarget))] public GameObjectReference Target = new();

        public MalbersTagCondition Condition;

        public Tag[] tags;


        protected override bool _Evaluate()
        {
            if (Target != null)
            {
                return Condition switch
                {
                    MalbersTagCondition.HasTag => Target.Value.HasMalbersTag(tags),
                    MalbersTagCondition.HasTagInParent => Target.Value.HasMalbersTagInParent(tags),
                    _ => false,
                };
            }
            return false;
        }
        protected override void _SetTarget(Object target) => Target.Value = MTools.VerifyComponent(target, Target.Value);
    }
}
