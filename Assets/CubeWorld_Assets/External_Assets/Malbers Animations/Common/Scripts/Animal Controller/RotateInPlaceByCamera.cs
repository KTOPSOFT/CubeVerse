using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>
    /// This script allows the Character to Rotate towards the camera direction if is on the Idle State doing nothing
    /// </summary>
    [AddComponentMenu("Malbers/Animal Controller/Rotate in Place by Camera")]

    public class RotateInPlaceByCamera : MonoBehaviour
    {
        [Tooltip("Reference for the Animal Controller")]
        [RequiredField] public MAnimal animal;
        [Tooltip("If the angle formed by Camera's forward direction and Character's forward direction greater than this value, then start rotating in place")]
        [Min(15)] public float LimitAngle = 90f;
        [Tooltip("If the angle formed by Camera's forward direction and Character's forward direction greater than this value, then Stop rotating in place")]
        [Min(1)] public float AngleThreshold = 2f;
        [Tooltip("Wait x seconds before rotating in place if the conditions are true")]
        [Min(0)] public float Wait = 1f;
        [Tooltip("Use only RootMotion Movement")]
        public bool RootMotionOnly = true;

        public bool debug = true;

        private void OnEnable()
        {
            animal.PreInput += PreInput;
            animal.PostStateMovement += PostStateMovement;
        }


        private void OnDisable()
        {
            animal.PreInput -= PreInput;
            animal.PostStateMovement -= PostStateMovement;
        }


        //
        private bool RotateInPlace;
        private Vector3 TargetRotation;
        private float angle;
        private float releaseTime;
        private bool waitTime;

        private void PostStateMovement(MAnimal animal)
        {
            //Only do RootMotion, Remove all additive position.
            if (RotateInPlace && RootMotionOnly)
            {
                animal.AdditiveRotation = animal.Anim.deltaRotation;
            }
        }

        private void PreInput(MAnimal animal)
        {
            //Do nothing if movement is detected, locomotion Idle is NOT playing or Strafe is true
            if (animal.RawInputAxis != Vector3.zero || animal.ActiveStateID.ID > 1 || animal.Strafe
                || animal.LockMovement == true || animal.LockUpDownMovement == true)
            {
                RotateInPlace = false;
                animal.Rotate_at_Direction = false;
                releaseTime = Time.time;
                waitTime = false;
                return;
            }

            if (!waitTime && MTools.ElapsedTime(releaseTime, Wait))
            {
                waitTime = true;
            }

            if (waitTime)
            {
                TargetRotation = Vector3.ProjectOnPlane(animal.MainCamera.transform.forward, Vector3.up).normalized;
                angle = Vector3.Angle(animal.transform.forward, TargetRotation);


                if (debug)
                {
                    MDebug.DrawRay(transform.position + Vector3.up * 0.01f, TargetRotation, Color.yellow);
                    MDebug.DrawRay(transform.position + Vector3.up * 0.01f, transform.forward, Color.yellow);
                }


                if (RotateInPlace)
                {
                    animal.RotateAtDirection(TargetRotation);

                    if (angle <= AngleThreshold)
                    {
                        if (debug) Debug.Log($"Stoping Rotate In Place ");

                        RotateInPlace = false;
                        animal.Rotate_at_Direction = false;
                    }
                }
                else
                {
                    if (angle >= LimitAngle)
                    {
                        RotateInPlace = true;
                    }
                }
            }
        }

        private void Reset()
        {
            animal = GetComponent<MAnimal>();
        }



#if UNITY_EDITOR && MALBERS_DEBUG
        private void OnDrawGizmos()
        {
            if (debug && animal)
            {
                UnityEditor.Handles.color = new Color(1, 1, 0, 0.01f);
                UnityEditor.Handles.DrawSolidArc(transform.position, animal.UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireArc(transform.position, animal.UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
            }
        }
#endif
    }
}
