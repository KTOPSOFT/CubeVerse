using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/String")]
    public class C_String : MCondition
    {
        public enum stringCondition { Equal, Contains }

        // public override string DisplayName => "Values/String";

        public StringReference Target;
        public stringCondition Condition;
        public StringReference Value;

        public void SetTarget(string targ) => Target.Value = targ;
        public void SetValue(string targ) => Value.Value = targ;


        public override bool _Evaluate()
        {
            switch (Condition)
            {
                case stringCondition.Equal: return Target.Value == Value.Value;
                case stringCondition.Contains: return Target.Value.Contains(Value.Value);
                default:
                    break;
            }
            return false;
        }

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target.Variable);

        //  private void Reset() => Name = "New String Comparer";
    }
}
