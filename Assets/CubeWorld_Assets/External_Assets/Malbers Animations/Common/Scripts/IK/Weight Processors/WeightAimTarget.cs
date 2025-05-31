using UnityEngine;

namespace MalbersAnimations.IK
{
    /// <summary>  Process the weight by checking  if the Aimer component has a target/ </summary>
    [System.Serializable]
    [AddTypeMenu("Aimer has Target")]
    public class WeightAimTarget : WeightProcessor
    {
        [Tooltip("If the Aimer component does not have a target then set the weight to 1")]
        public bool invert = false;

        public override float Process(IKSet set, float weight)
        {
            var newWeight = weight * (set.aimer != null && set.aimer.AimTarget ? 1f : 0f);

            if (invert) newWeight = 1 - newWeight;

            return newWeight;
        }
    }
}
