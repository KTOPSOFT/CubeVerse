using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// https://x.com/AlexStrook/status/1732240011431383275?s=20
    /// Testing Framerates   </summary>
    [AddComponentMenu("Malbers/Utilities/Frame Rate")]
    public class FrameRateChecker : MonoBehaviour
    {
        [HelpBox]
        public string Description = "Shorcuts to limit your game framerate (Shift+F1/F2/F3/F4/F5/F6  for 10/20/30/60/120/Reset fps)";

        void Update()
        {
#if UNITY_EDITOR

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    Application.targetFrameRate = 10;
                if (Input.GetKeyDown(KeyCode.F2))
                    Application.targetFrameRate = 20;
                if (Input.GetKeyDown(KeyCode.F3))
                    Application.targetFrameRate = 30;
                if (Input.GetKeyDown(KeyCode.F4))
                    Application.targetFrameRate = 60;
                if (Input.GetKeyDown(KeyCode.F5))
                    Application.targetFrameRate = 120;
                if (Input.GetKeyDown(KeyCode.F6))
                    Application.targetFrameRate = 0;
            }
#endif
        }
    }
}
