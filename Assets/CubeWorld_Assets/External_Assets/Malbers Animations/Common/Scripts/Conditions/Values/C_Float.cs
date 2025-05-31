using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/Float")]
    public class C_Float : MCondition
    {
        //public override string DisplayName => "Values/Float";

        public FloatReference Target;
        public ComparerInt Condition;
        public FloatReference Value;

        public void SetTarget(float targ) => Target.Value = targ;
        public void SetValue(float targ) => Value.Value = targ;

        public void SetTarget(FloatVar targ) => Target.Value = targ.Value;
        public void SetValue(FloatVar targ) => Value.Value = targ.Value;

        public override bool _Evaluate() => Target.Value.CompareFloat(Value.Value, Condition);

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target.Variable);


        private void Reset() => Name = "New Float Comparer";
    }
}
