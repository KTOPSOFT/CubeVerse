using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.IK
{
    public class IKBehavior : StateMachineBehaviour
    {
        private IIKSource source;

        [Tooltip("The name of the IK Set to be enabled or disabled.")]
        public StringReference IKSet = new();

        [Tooltip("Enable or disable the IK Set when entering the state.")]
        public bool OnEnter = true;

        [Hide(nameof(OnEnter))]

        [Tooltip("Specifies whether the IK Set should be enabled or disabled on entering the state.")]
        public bool enable = true;

        [Space]
        [Tooltip("Enable or disable the IK Set when exiting the state.")]
        public bool OnExit = false;

        [Hide(nameof(OnExit))]
        [Tooltip("Specifies whether the IK Set should be enabled or disabled on exiting the state.")]
        public bool m_enable = true;


        [Space]
        [Tooltip("Enable or disable the IK Set at a specific time during the animation.")]
        public bool OnTime = false;

        [Hide(nameof(OnTime))]
        [MinMaxRange(0, 1), Tooltip("The specific normalized time in the animation (0 to 1) when the IK Set should be toggled.")]
        public RangedFloat timeThreshold = new(0.4f, 0.8f);
        [Hide(nameof(OnTime))]
        [Tooltip("Specifies whether the IK Set should be enabled or disabled at the specified time.")]
        public bool _enable = true;

        private bool hasTriggered;// Prevent multiple triggers in the same loop.
        private bool originalEnableState;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state.
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            source ??= animator.GetComponent<IIKSource>();
            hasTriggered = false;
            originalEnableState = !_enable; // Save the original enable state.

            if (OnEnter && !OnTime)
                source?.Set_Enable(IKSet.Value, enable);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit.
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnTime && !hasTriggered)
            {
                float currentNormalizedTime = stateInfo.normalizedTime % 1;

                if (timeThreshold.IsInRange(currentNormalizedTime))
                {
                    source?.Set_Enable(IKSet.Value, _enable);
                    hasTriggered = true;
                }
            }

            // Reset to original state after the time threshold
            if (OnTime && hasTriggered)
            {
                float currentNormalizedTime = stateInfo.normalizedTime % 1;

                if (currentNormalizedTime >= timeThreshold.maxValue)
                {
                    source?.Set_Enable(IKSet.Value, originalEnableState);
                    hasTriggered = false; // Reset for the next cycle.
                }
            }
        }

        // OnStateExit is called when a transition ends, and the state machine finishes evaluating this state.
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnExit)
                source?.Set_Enable(IKSet.Value, m_enable);
            else if (OnTime)
                source?.Set_Enable(IKSet.Value, originalEnableState); // Reset to the original state on exit.
        }
    }
}