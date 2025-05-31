using MalbersAnimations.Conditions;
using UnityEngine;

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking the Look At Angle of the Animator / </summary>
    [System.Serializable]
    [AddTypeMenu("Check Conditions2")]
    public class WeightConditions2 : WeightProcessor
    {
        [Tooltip("Evaluate conditions. If true then it will set the weight to 1 if false it will set to 0")]
        public Conditions2 CheckIf = new();

        public override float Process(IKSet set, float weight) => weight * (CheckIf.Evaluate(set.Animator) ? 1 : 0);
    }
}
