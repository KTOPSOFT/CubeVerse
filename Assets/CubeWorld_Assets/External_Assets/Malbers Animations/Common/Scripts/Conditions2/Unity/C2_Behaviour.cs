using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("Unity/Behavior")]
    public class C2_Behaviour : ConditionCore
    {
        public enum ComponentCondition { Enabled, ActiveAndEnabled }

        [Tooltip("Target to check for the condition ")]
        [RequiredField, Hide(nameof(LocalTarget))]
        public Behaviour Target;
        [Tooltip("Conditions types")]
        public ComponentCondition Condition;

        protected override bool _Evaluate()
        {
            if (Target != null)
            {
                switch (Condition)
                {
                    case ComponentCondition.Enabled: return Target.enabled;
                    case ComponentCondition.ActiveAndEnabled: return Target.isActiveAndEnabled;
                }
            }
            return false;
        }

        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);
    }
}
