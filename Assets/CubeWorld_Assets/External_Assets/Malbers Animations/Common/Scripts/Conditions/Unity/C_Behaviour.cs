using UnityEngine;

namespace MalbersAnimations.Conditions
{
    public enum ComponentCondition { Enabled, ActiveAndEnabled }

    [System.Serializable]
    [AddTypeMenu("Unity/Behavior")]
    public class C_Behaviour : MCondition
    {
        //public override string DisplayName => "Unity/Behavior";

        [Tooltip("Target to check for the condition ")]
        [RequiredField] public Behaviour Target;
        [Tooltip("Conditions types")]
        public ComponentCondition Condition;


        public override bool _Evaluate()
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

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target);

        private void Reset() => Name = "New Behaviour Condition";
    }
}
