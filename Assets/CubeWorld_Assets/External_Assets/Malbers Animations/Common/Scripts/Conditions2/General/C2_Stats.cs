using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("General/Stats")]
    public class C_Stats2 : ConditionCore//<Stats>
    {
        [Hide(nameof(LocalTarget))]
        public Stats Target;

        public StatCondition Condition;

        public StatID stat;
        [Hide(nameof(Condition), (int)StatCondition.MinValue, (int)StatCondition.MaxValue, (int)StatCondition.Value, (int)StatCondition.ValueNormalized)]
        public ComparerInt Compare;
        [Hide(nameof(Condition), (int)StatCondition.MinValue, (int)StatCondition.MaxValue, (int)StatCondition.Value, (int)StatCondition.ValueNormalized)]
        public FloatReference Value;
        private Stat st;
        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);

        protected override bool _Evaluate()
        {
            if (!Target) return false;
            st ??= Target.Stat_Get(stat); //Get the Stat from the Target
            if (st == null) return false;

            return Condition switch
            {
                StatCondition.Enabled => st.Active,
                StatCondition.HasStat => st != null,
                StatCondition.Regenerating => st.IsRegenerating,
                StatCondition.Degenerating => st.IsDegenerating,
                StatCondition.Inmune => st.IsImmune,
                StatCondition.Full => st.IsFull,
                StatCondition.Empty => st.IsEmpty,
                StatCondition.Value => st.Value.CompareFloat(Value.Value, Compare),
                StatCondition.ValueNormalized => st.NormalizedValue.CompareFloat(Value.Value, Compare),
                StatCondition.MaxValue => st.MaxValue.CompareFloat(Value.Value, Compare),
                StatCondition.MinValue => st.MinValue.CompareFloat(Value.Value, Compare),
                _ => false,
            };
        }
    }
}