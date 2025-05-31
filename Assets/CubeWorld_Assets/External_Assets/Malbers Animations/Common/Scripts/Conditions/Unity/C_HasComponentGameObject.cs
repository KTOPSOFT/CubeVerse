using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Has Component [GameObject]")]
    public class C_HasComponentGameObject : MCondition
    {
        // public override string DisplayName => "Unity/Has Component [GameObject]";

        [Tooltip("Target to check for the condition ")]
        [RequiredField] public GameObject Target;
        [Tooltip("Name of the Component script. (Type Name)")]
        public string componentName;


        public override bool _Evaluate()
        {
            return Target != null && Target.GetComponent(componentName) != null;
        }

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target);

        private void Reset() => Name = "Does the GameObject has this component?";
    }
}
