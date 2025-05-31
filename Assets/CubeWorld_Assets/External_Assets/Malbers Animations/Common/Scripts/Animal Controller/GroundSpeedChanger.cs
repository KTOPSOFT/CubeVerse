using MalbersAnimations.Reactions;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/Ground Speed Changer")]
    public class GroundSpeedChanger : MonoBehaviour
    {
        [Tooltip("This will make the ground slippery if the value is very low.\nSet it to Zero to ignore the Sliding Speed")]
        [Min(0)] public float Lerp = 2f;


        [Tooltip("Adittional Position added to the Movement on the Floor")]
        [Hide(nameof(Lerp), true)]
        public float Position;

        [Tooltip("Slide Override on the Animal Controller. When the Animal gets to a Slide on the ground because of a Slope, This is the value for slide down with gravity")]
        [Hide(nameof(Lerp), true)]
        public float SlideAmount = 0.25f;

        [Tooltip("Slide activation using the Max Slope Limit if the slope angle is greater than this value, then the sliding will start.")]
        [Hide(nameof(Lerp), true)]
        public float SlideThreshold = 30f;

        [Tooltip("Lerp value to smoothly slide down the ramp")]
        [Hide(nameof(Lerp), true)]
        public float SlideDamp = 20f;

        [Tooltip("Values used on the [Slide] State")]
        public SlideData SlideData;

        [SubclassSelector, SerializeReference]
        public Reaction OnEnter;
        [SubclassSelector, SerializeReference]
        public Reaction OnExit;

        private void Reset()
        {
            SlideData = new SlideData() { ActivationAngle = 180 };
        }
    }

    [System.Serializable]
    public struct SlideData
    {
        [Tooltip("If is set to true then this Ground Changer can activate the Slide State on the Animal")]
        public bool Slide;

        [Tooltip("If true, then the rotation of the character will be ignored during the Slide State")]
        public bool IgnoreRotation;

        [Tooltip("Minimum  Slope Direction Angle to activate the Slide State")]
        [Min(0)] public float MinAngle;

        [Tooltip("Slide activation angle to activate the state. The character needs to be looking/align at the Slope, Default value 180")]
        public float ActivationAngle;


        [Tooltip("Additive Value to add to the Speed of the Slide State")]
        public float AdditiveForwardSpeed;

        [Tooltip("Additive Value to add to the Speed of the Slide State")]
        public float AdditiveHorizontalSpeed;
    }
}
