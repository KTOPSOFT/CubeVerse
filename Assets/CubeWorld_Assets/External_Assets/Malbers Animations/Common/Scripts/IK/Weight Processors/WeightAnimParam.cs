using UnityEngine;

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking the Look At Angle of the Animator / </summary>
    [System.Serializable, AddTypeMenu("Animator/Paramameter (Float)")]

    public class WeightAnimParam : WeightProcessor
    {
        [Tooltip("Name of the Animator Parameter to check")]
        [AnimatorParam(AnimatorControllerParameterType.Float)]
        public string Parameter;
        [Tooltip("Normalize the weight by this value")]
        public float normalizedBy = 1;

        [HideInInspector] public int AnimParamHash;

        [Tooltip("Inverth the value of the Animation Curve (One Minus) 1-Value")]
        public bool invert = false;


        public override float Process(IKSet set, float weight)
        {
            if (AnimParamHash == 0)
                AnimParamHash = Animator.StringToHash(Parameter);

            var animWeight = 1f;

            if (AnimParamHash != 0)
            {
                animWeight = set.Animator.GetFloat(AnimParamHash) / normalizedBy;
                if (invert) animWeight = 1 - animWeight;
            }
            return Mathf.Min(weight, animWeight);
        }
    }
}
