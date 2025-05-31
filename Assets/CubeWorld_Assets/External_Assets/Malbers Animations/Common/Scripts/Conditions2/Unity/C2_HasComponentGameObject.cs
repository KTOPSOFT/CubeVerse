using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Has Component [GameObject]")]
    [ConditionDescription("Does [GameObject] has [Component] attached")]
    public class C2_HasComponentGameObject : ConditionCore
    {
        [Tooltip("Target to check for the condition "), Hide(nameof(LocalTarget))]
        public GameObjectReference Target = new();

        [Tooltip("Type of Script-Component attached to the GameObject")]
        public StringReference component = new();

        protected override bool _Evaluate()
        {
            return Target.Value != null && Target.Value.GetComponent(component) != null;
        }

        protected override void _SetTarget(Object target) => Target.Value = MTools.VerifyComponent(target, Target.Value);
    }
}
