using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
#endif


namespace MalbersAnimations.Conditions
{
    public enum TransformCondition { Null, Equal, ChildOf, ParentOf, IsGrandChildOf, IsGrandParentOf, Name }

    [System.Serializable]
    [AddTypeMenu("Unity/Transform")]
    public class C_Transform : MCondition
    {
        //public override string DisplayName => "Unity/Transform";
        public string CheckName { get => checkName.Value; set => checkName.Value = value; }

        [Tooltip("Target to check for the condition ")]
        public TransformReference Target;
        [Tooltip("Conditions types")]
        public TransformCondition Condition;
        [Tooltip("Transform Value to compare with")]
        [Hide(nameof(Condition), true, 0, 6)]
        public TransformReference Value;
        [Tooltip("Name to compare"), SerializeField]
        [Hide(nameof(Condition), 6)]
        private StringReference checkName;

        public override bool _Evaluate()
        {
            if (Condition == TransformCondition.Null) return Target.Value == null;

            if (Target.Value != null)
            {
                return Condition switch
                {
                    TransformCondition.Name => Target.Value.name.Contains(CheckName),
                    TransformCondition.ChildOf => Target.Value.IsChildOf(Value.Value),
                    TransformCondition.Equal => Target.Value == Value.Value,
                    TransformCondition.ParentOf => Value.Value.IsChildOf(Target.Value),
                    TransformCondition.IsGrandChildOf => Target.Value.SameHierarchy(Value.Value),
                    TransformCondition.IsGrandParentOf => Value.Value.SameHierarchy(Target.Value),
                    _ => false,
                };
            }

            return false;
        }

        protected override void _SetTarget(Object target)
        {
            var Tar = Target.Value;
            VerifyTarget(target, ref Tar);
            Target.Value = Tar;
        }

        public void SetValue(Object target) => _SetTarget(target);
    }
}
