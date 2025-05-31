using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Unity/Animator Parameter")]
    public class C_AnimatorParameter : MCondition
    {
        [Tooltip("Target to check for the condition ")]
        [RequiredField] public Animator Target;

        [Tooltip("Paramerter to check in the animator ")]
        public string parameter = "Parameter Name";

        [Tooltip("Conditions types")]
        public AnimatorType parameterType;

        [Hide(nameof(parameterType), true, 2)]
        public ComparerInt compare = ComparerInt.Equal;

        [Hide(nameof(parameterType), false, 2)]
        public bool m_isTrue;
        [Hide(nameof(parameterType), false, 0)]
        public float m_Value;
        [Hide(nameof(parameterType), false, 1)]
        public int value;
        private int ParameterHash;

        public override bool _Evaluate()
        {
            if (ParameterHash == 0) ParameterHash = Animator.StringToHash(parameter);

            if (Target != null)
            {
                switch (parameterType)
                {
                    case AnimatorType.Float:
                        var Float = Target.GetFloat(ParameterHash);

                        //  Debug.Log($"Param F[{Float:F2}] Value [{m_Value:F2}]; {compare} [{Float.CompareFloat(m_Value, compare)}]");

                        return Float.CompareFloat(m_Value, compare);
                    case AnimatorType.Int:
                        var Int = Target.GetInteger(ParameterHash);

                        //Debug.Log($"Param I[{Int}] Value [{value}]; {compare} [{Int.CompareInt(value, compare)}]");

                        return Int.CompareInt(value, compare);
                    case AnimatorType.Bool:

                        var Bool = Target.GetBool(ParameterHash);

                        // Debug.Log($"Param B[{Bool}] Value [{m_isTrue}]; {compare} [{Bool == m_isTrue}]");

                        return Bool == m_isTrue;
                    default: break;
                }
            }
            return false;
        }
        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target);
    }
}
