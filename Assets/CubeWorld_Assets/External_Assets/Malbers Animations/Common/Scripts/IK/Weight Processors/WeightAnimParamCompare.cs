using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking the Look At Angle of the Animator / </summary>
    [System.Serializable, AddTypeMenu("Animator/Paramameter Compare")]
    public class WeightAnimParamCompare : WeightProcessor
    {
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

        [HideInInspector] public int AnimParamHash;


        public override float Process(IKSet set, float weight)
        {
            if (AnimParamHash == 0)
                AnimParamHash = Animator.StringToHash(parameter.Value);

            bool Result = false;
            switch (parameterType)
            {
                case AnimatorType.Float:
                    var Float = set.Animator.GetFloat(AnimParamHash);
                    Result = Float.CompareFloat(m_Value, compare);
                    break;
                case AnimatorType.Int:
                    var Int = set.Animator.GetInteger(AnimParamHash);
                    Result = Int.CompareInt(value, compare); break;
                case AnimatorType.Bool:
                    var Bool = set.Animator.GetBool(AnimParamHash);
                    Result = Bool == m_isTrue.Value; break;
                default: break;
            }
            return Result ? weight : 0;
        }
    }
}
