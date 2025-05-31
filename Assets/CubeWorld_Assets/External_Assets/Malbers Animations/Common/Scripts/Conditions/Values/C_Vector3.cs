using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/Vector3")]
    public class C_Vector3 : MCondition
    {
        //public override string DisplayName => "Values/Vector3";

        public Vector3Reference Target;
        public Vector3Reference Value;

        public void SetTarget(Vector3 targ) => Target.Value = targ;
        public void SetValue(Vector3 targ) => Value.Value = targ;

        public void SetTarget(Vector3Var targ) => Target.Value = targ.Value;
        public void SetValue(Vector3Var targ) => Value.Value = targ.Value;

        public override bool _Evaluate() => Target.Value == Value.Value;

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target.Variable);

        private void Reset() => Name = "New Vector3 Comparer";
    }
}
