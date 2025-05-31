using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/Boolean")]
    public class C_Bool : MCondition
    {
        // public override string DisplayName => "Values/Boolean";

        public BoolReference Target = new();
        public BoolReference Value = new();

        public void SetTarget(bool targ) => Target.Value = targ;
        public void SetValue(bool targ) => Value.Value = targ;

        public void SetTarget(BoolVar targ) => Target.Value = targ.Value;
        public void SetValue(BoolVar targ) => Value.Value = targ.Value;

        public override bool _Evaluate() => Target.Value == Value.Value;

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target.Variable);


        // private void Reset() => Name = "New Bool Comparer";
    }
}
