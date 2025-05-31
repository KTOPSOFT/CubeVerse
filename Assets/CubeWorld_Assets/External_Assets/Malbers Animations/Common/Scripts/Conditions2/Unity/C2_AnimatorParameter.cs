using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Animator Parameter")]
    [ConditionDescription("Check an Animator Parameter values")]
    public class C2_AnimatorParameter : ConditionCore
    {
        [Tooltip("Target to check for the condition ")]
        [RequiredField, Hide(nameof(LocalTarget))]
        public Animator Target;

        [Tooltip("Paramerter to check in the animator ")]
        public StringReference parameter = new("Parameter Name");

        [Tooltip("Conditions types")]
        public AnimatorType parameterType;

        [Hide(nameof(parameterType), true, 2)]
        public ComparerInt compare = ComparerInt.Equal;

        [Hide(nameof(parameterType), false, 2)]
        public BoolReference m_isTrue;
        [Hide(nameof(parameterType), false, 0)]
        public FloatReference m_Value;
        [Hide(nameof(parameterType), false, 1)]
        public IntReference value;
        private int ParameterHash;

        protected override bool _Evaluate()
        {
            if (ParameterHash == 0) ParameterHash = Animator.StringToHash(parameter);

            if (Target != null)
            {
                switch (parameterType)
                {
                    case AnimatorType.Float:
                        var Float = Target.GetFloat(ParameterHash);
                        return Float.CompareFloat(m_Value, compare);
                    case AnimatorType.Int:
                        var Int = Target.GetInteger(ParameterHash);
                        return Int.CompareInt(value, compare);
                    case AnimatorType.Bool:
                        var Bool = Target.GetBool(ParameterHash);
                        return Bool == m_isTrue.Value;
                    default: break;
                }
            }
            return false;
        }
        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);
    }
}
