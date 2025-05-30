using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace SoftKitty.PCW.Demo
{
    public class PostProcessingControl : MonoBehaviour
    {
        public PostProcessVolume _volumn;
        public Transform _cam;
        DepthOfField _depth;
        void Start()
        {
            _volumn.profile.TryGetSettings<DepthOfField>(out _depth);
        }


        void Update()
        {
            _depth.aperture.value = 1.5F + CameraControl.ApertureAdd;
            _depth.focusDistance.value = Mathf.Abs(_cam.localPosition.z);
        }
    }
}