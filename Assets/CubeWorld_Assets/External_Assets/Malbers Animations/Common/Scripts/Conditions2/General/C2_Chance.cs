using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("General/Try Chance (0:1)")]
    [ConditionDescription("Random Chance to send true [0..1]")]
    public class C2_Weight : ConditionCore
    {
        protected override void _SetTarget(Object target) { }
        [Tooltip("Chance for checking a condition Set the value from  0 to 1")]
        public FloatReference Weight = new();

        protected override bool _Evaluate()
        {
            float prob = Random.Range(0f, 1f);
            if (prob <= Weight)
            {
                Debugging($"Chance [{prob:F1}] is in the Weight Range [{Weight.Value:F1}]", true, null);
                return true; //Do not Activate the Zone with low Probability.
            }

            Debugging($"Chance [{prob:F1}] is NOT the Weight Range [{Weight.Value:F1}]", false, null);
            return false;
        }
    }
}
