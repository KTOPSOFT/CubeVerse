using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SoftKitty.PCW.Demo
{
    public class CameraControl : MonoBehaviour
    {
        public static float ApertureAdd = 0F;
        public static float FovAdd = 0F;
        public static bool Active = true;
        public LayerMask GroundLayer;
        public Transform RotY;
        public Transform RotX;
        public Transform OffsetY;
        public Camera Cam;
        public Transform FollwingTarget;
        public Volume PostVolume;
        private float AngleY;
        private float AngleX =30F;
        private float Scroll = 6F;
        private float Clipping = 6F;
        private bool Clipped = false;
        private RaycastHit GroundHit;
        private bool MouseButtonDown =false;
        private DepthOfField DepthScript;

        void Start()
        {
             PostVolume.profile.TryGet<DepthOfField>(out DepthScript);
        }

        void Update()
        {
            Active = !BlockGenerator.instance.isTeleporting();
            if(!BuildControl.BuildMode && !EventSystem.current.IsPointerOverGameObject()) Scroll = Mathf.Clamp(Scroll - Input.GetAxis("Mouse ScrollWheel")*7F, 3F, 8F);
            FovAdd = Mathf.Lerp(FovAdd,0F,Time.deltaTime*1F);
            ApertureAdd = Mathf.Lerp(ApertureAdd, 0F, Time.deltaTime * 1F);
            Cam.fieldOfView = 60F + FovAdd;
            if (DepthScript != null) DepthScript.aperture.value =1F+ApertureAdd;

            if (!Active) return;
            if (Input.GetMouseButtonDown(1)) MouseButtonDown = true;
            if (Input.GetMouseButtonUp(1)) MouseButtonDown = false;

            if (MouseButtonDown)
            {
                Cursor.lockState =  CursorLockMode.Locked;
                Cursor.visible = false;
                AngleY += Input.GetAxis("Mouse X")*Time.deltaTime*100F;
                AngleX = Mathf.Clamp(AngleX- Input.GetAxis("Mouse Y") * Time.deltaTime * 50F, -30F,80F);
                AngleY = AngleY % 360F;
                if (AngleY > 180F) AngleY -= 360F;
                if (AngleY < -180F) AngleY += 360F;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            
        }
        void LateUpdate()
        {
            if (FollwingTarget == null || !Active) return;
            transform.position = FollwingTarget.position;
            Cam.transform.localPosition = new Vector3(0F,0F, Mathf.Lerp(Cam.transform.localPosition.z, Mathf.Max(-Clipping, -Scroll),Time.deltaTime* (Clipped ? 100F:2F)));
            if (DepthScript != null) DepthScript.focusDistance.value = -Cam.transform.localPosition.z;
        }

        private void FixedUpdate()
        {
            if (!Active) return;
            RotY.localEulerAngles = new Vector3(0F, AngleY, 0F);
            RotX.localEulerAngles = new Vector3(AngleX, 0F, 0F);
            if (Physics.Linecast(RotX.transform.position, Cam.transform.position- Cam.transform.forward*2F, out GroundHit, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                Clipping = Mathf.Min(Scroll,GroundHit.distance-0.5F);
                Clipped = true;
            }
            else
            {
                Clipping = Scroll;
                Clipped = false;
            }

        }
    }
}
