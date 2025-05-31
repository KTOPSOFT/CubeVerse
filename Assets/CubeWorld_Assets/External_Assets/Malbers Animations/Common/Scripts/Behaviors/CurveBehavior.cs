using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Sends the Value of a curve to a monobehaviour attached to the Animator</summary>
    public class CurveBehavior : StateMachineBehaviour
    {
        [Tooltip("ID of the Curve")]
        public int ID;

        [Tooltip("Curve to send to the Animator")]
        public AnimationCurve Value = new(new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, 1) });

        [Tooltip("Enable or disable updating the animator parameter")]
        public bool driveAnimParameter = true;

        [Hide("driveAnimParameter", false), Tooltip("The name of the animator parameter to drive")]
        public string animatorParameterName;

        private List<IAnimatorCurve> curves;
        private bool hasICurves;

        override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if (!hasICurves)
            {
                curves = animator.GetComponentsInChildren<IAnimatorCurve>().ToList();
                if (curves != null) hasICurves = true;
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Normalize the time so that it loops within the curve range (0 to 1)
            float normalizedTime = stateInfo.normalizedTime % 1f;
            float curveValue = Value.Evaluate(normalizedTime);

            // Set animator parameter if option is enabled
            if (driveAnimParameter && !string.IsNullOrEmpty(animatorParameterName))
            {
                animator.SetFloat(animatorParameterName, curveValue);
            }

            if (hasICurves)
            {
                for (int i = 0; i < curves.Count; i++)
                {
                    curves[i].AnimatorCurve(ID, curveValue);
                }
            }
        }
    }

}
