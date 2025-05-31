#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Camera/Third Person Follow Zoom (Cinemachine)")]
    [DefaultExecutionOrder(121)]
    public class ThirdPersonFollowZoom : MonoBehaviour
    {
        [Tooltip("Update mode for the Aim Logic")]
        public UpdateType updateMode = UpdateType.FixedUpdate;
        [Tooltip("The Camera can rotate independent of the Game Time")]
        public BoolReference unscaledTime = new(true);

        [Tooltip("Zoom In Min Value")]
        public FloatReference ZoomMin = new(1);

        [Tooltip("Zoom Out Max Value")]
        public FloatReference ZoomMax = new(12);

        [Tooltip("Zoom step changes")]
        public FloatReference ZoomStep = new(1);

        [Tooltip("Zoom smooth value to change between steps")]
        public FloatReference ZoomLerp = new(5);

        /// <summary> Current Target Zoom </summary>
        private float TargetZoom { get; set; }


#if UNITY_6000_0_OR_NEWER
        private CinemachineThirdPersonFollow TPF;
#else
        private Cinemachine3rdPersonFollow TPF;
#endif

        public bool UnScaledTime { get => unscaledTime; set => unscaledTime.Value = value; }

        private void Start()
        {

#if UNITY_6000_0_OR_NEWER
            TPF = this.FindComponent<CinemachineThirdPersonFollow>();
#else
            TPF = this.FindComponent<Cinemachine3rdPersonFollow>();
#endif

            if (TryGetComponent<ThirdPersonFollowTarget>(out var follow))
            {
                TargetZoom = follow.CameraDistance;
            }
        }


        public void ZoomIn()
        {
            if (TPF != null && enabled)
                TargetZoom = Mathf.Clamp(TargetZoom - ZoomStep, ZoomMin, ZoomMax);
        }

        public void ZoomOut()
        {
            if (TPF != null && enabled)
                TargetZoom = Mathf.Clamp(TargetZoom + ZoomStep, ZoomMin, ZoomMax);
        }


        public void SetZoom(bool zoom)
        {
            if (zoom)
            {
                ZoomOut();
            }
            else
            {
                ZoomIn();
            }
        }

        public void SetZoom(float zoom) => SetZoom(zoom < 0);


        private void FixedUpdate()
        {
            if (updateMode == UpdateType.FixedUpdate)
            {
                CalculateZoom(UnScaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            if (updateMode == UpdateType.LateUpdate)
            {
                CalculateZoom(UnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            }
        }

        private void CalculateZoom(float deltaTime)
        {
            if (TPF)
                TPF.CameraDistance = Mathf.Lerp(TPF.CameraDistance, TargetZoom, ZoomLerp * deltaTime);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            var scroll = gameObject.AddComponent<MMouseScroll>();


            UnityEditor.Events.UnityEventTools.AddPersistentListener(scroll.OnScrollDown, ZoomOut);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(scroll.OnScrollUp, ZoomIn);

        }
#endif
    }
}
