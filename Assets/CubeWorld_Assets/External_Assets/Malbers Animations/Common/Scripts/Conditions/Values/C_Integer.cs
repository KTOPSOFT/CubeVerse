using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/Integer")]
    public class C_Integer : MCondition
    {
        //public override string DisplayName => "Values/Integer";

        public IntReference Target;
        public ComparerInt Condition;
        public IntReference Value;

        public void SetTarget(int targ) => Target.Value = targ;
        public void SetValue(int targ) => Value.Value = targ;

        public void SetTarget(IntVar targ) => Target.Value = targ.Value;
        public void SetValue(IntVar targ) => Value.Value = targ.Value;

        public override bool _Evaluate() => Target.Value.CompareInt(Value.Value, Condition);

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target.Variable);


        private void Reset() => Name = "New Integer Comparer";
    }
}
