using UnityEngine;

namespace MalbersAnimations.Utilities
{
    public class FPSTarget : MonoBehaviour
    {
        public enum CustomFixedTimeStep
        {
            Default,
            [InspectorName("30 FPS")]
            FPS30,
            [InspectorName("60 FPS")]
            FPS60,
            [InspectorName("75 FPS")]
            FPS75,
            [InspectorName("90 FPS")]
            FPS90,
            [InspectorName("120 FPS")]
            FPS120,
            [InspectorName("144 FPS")]
            FPS144
        };

        [Tooltip("Set the FixedTimeStep to match the FPS of your Game, \nEx: If your game aims to run at 30fps, select FPS30 to match the FixedUpdate Physics")]
        public CustomFixedTimeStep customFixedTimeStep = CustomFixedTimeStep.FPS60;

        void Awake()
        {
            SetCustomFixedTimeStep();
        }
        public virtual void SetCustomFixedTimeStep()
        {
            switch (customFixedTimeStep)
            {
                case CustomFixedTimeStep.Default:
                    break;
                case CustomFixedTimeStep.FPS30:
                    Time.fixedDeltaTime = 0.03333334f;
                    break;
                case CustomFixedTimeStep.FPS60:
                    Time.fixedDeltaTime = 0.01666667f;
                    break;
                case CustomFixedTimeStep.FPS75:
                    Time.fixedDeltaTime = 0.01333333f;
                    break;
                case CustomFixedTimeStep.FPS90:
                    Time.fixedDeltaTime = 0.01111111f;
                    break;
                case CustomFixedTimeStep.FPS120:
                    Time.fixedDeltaTime = 0.008333334f;
                    break;
                case CustomFixedTimeStep.FPS144:
                    Time.fixedDeltaTime = 0.006944444f;
                    break;
            }
        }
    }

}