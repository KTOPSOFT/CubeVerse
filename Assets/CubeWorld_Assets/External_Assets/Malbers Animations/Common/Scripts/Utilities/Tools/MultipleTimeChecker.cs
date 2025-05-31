using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/multiple-time-checker")]
    [AddComponentMenu("Malbers/Utilities/Multiple Time Checker")]
    public class MultipleTimeChecker : MonoBehaviour
    {
        [Tooltip("Amount of taps/clicks/checks needed to trigger the event.")]
        [Min(1)] public int MaxChecks = 2;
        [Min(0.1f)] public float interval = 0.6f;
        public bool debug;

        public int CurrentCheck { get; private set; }
        public float CurrentTime { get; private set; }

        public IntEvent CheckStep = new();
        public UnityEvent CheckSuccessful = new();


        [MButton("Check", "Test", true)]
        public bool TestButton;

        public void Check()
        {
            if (MTools.ElapsedTime(CurrentTime, interval))
            {
                // Reset if the interval is exceeded
                ResetCheck();
            }

            // Increase check count
            CurrentCheck++;

            if (debug) Debug.Log($"Check [{CurrentCheck}] at time: {Time.time}");

            CheckStep.Invoke(CurrentCheck);

            if (CurrentCheck >= MaxChecks)
            {
                if (debug) Debug.Log("Max Checks Successful!");
                CheckSuccessful.Invoke();
                ResetCheck();
            }
            else
            {
                // Store the current time for interval tracking
                CurrentTime = Time.time;
            }
        }

        void ResetCheck()
        {
            CurrentCheck = 0; // Start fresh from zero
            CurrentTime = Time.time;
        }
    }
}