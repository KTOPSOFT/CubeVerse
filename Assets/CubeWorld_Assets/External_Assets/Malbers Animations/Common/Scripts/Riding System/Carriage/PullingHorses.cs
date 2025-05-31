using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.HAP
{
    [AddComponentMenu("Malbers/Riding/Pulling Horses")]
    public class PullingHorses : MonoBehaviour
    {
        public MAnimal MainAnimal;
        public MAnimal SecondAnimal;

        [Tooltip("Main Animal Rigid Body "),RequiredField]
        public Rigidbody RB;

        public Vector3 PullingDirection { get; set; }          //Calculation for the Animator Velocity converted to RigidBody Velocityble
        public bool CurrentAngleSide { get; set; }               //True if is in the Right Side ... False if is in the Left Side
        public bool CanRotateInPlace { get; set; }


        public bool HasSecondAnimal => SecondAnimal != null && SecondAnimal != MainAnimal;

        public Vector3 RotationOffset;
        Vector3 SecondAnimalOffset;


        // Use this for initialization
        void Start()
        {
            if (!MainAnimal)
            {
                Debug.LogWarning("MainAnimal is Empty, Please set the Main Animal");
                return;
            }
            if (!SecondAnimal) SecondAnimal = MainAnimal;

            RB = MainAnimal.RB;

            //MainAnimal.transform.parent = transform;
            //SecondAnimal.transform.parent = transform;

            // RHorseInitialPos = MainAnimal.transform.localPosition;          //Store the position of the Right Main Horse
            // LHorseInitialPos = SecondAnimal.transform.localPosition;        //Store the position of the Right Main Horse

            //MainAnimal.DisablePosition = true;
            //SecondAnimal.DisablePosition = true;

             MainAnimal.DisableRotation = true;
             SecondAnimal.DisableRotation = true;

            if (HasSecondAnimal)
            {
                SecondAnimal.DisablePosition = true;
                SecondAnimalOffset = MainAnimal.transform.InverseTransformPoint(SecondAnimal.transform.position);
            }


            SecondAnimal.RootMotion = false;
            MainAnimal.RootMotion = false;
        }

        void FixedUpdate()
        {
            var time = Time.fixedDeltaTime;

            if (time > 0)
            {
                var RotationPoint = MainAnimal.transform.TransformPoint(RotationOffset);

                //Rotate around Speed
                MainAnimal.transform.RotateAround(RotationPoint, MainAnimal.UpVector, MainAnimal.HorizontalSmooth * time * MainAnimal.CurrentSpeedModifier.rotation);

                if (SecondAnimal != MainAnimal)
                {
                    SecondAnimal.transform.RotateAround(RotationPoint, MainAnimal.UpVector, MainAnimal.HorizontalSmooth * time * MainAnimal.CurrentSpeedModifier.rotation);

                    var offset = MainAnimal.transform.TransformPoint(SecondAnimalOffset);

                    if (SecondAnimal.Grounded)
                    {
                        offset.y = SecondAnimal.transform.position.y;
                        SecondAnimal.transform.position = offset;

                    }
                    else
                    {
                        SecondAnimal.transform.position = MainAnimal.transform.position + offset;
                    }
                }
            }
        }


        void LateUpdate()
        {
            // MainAnimal.transform.localPosition = new Vector3(RHorseInitialPos.x, MainAnimal.transform.localPosition.y, RHorseInitialPos.z);
            //  SecondAnimal.transform.localPosition = new Vector3(RHorseInitialPos.x, MainAnimal.transform.localPosition.y, RHorseInitialPos.z);


            if (HasSecondAnimal)
            {
                SecondAnimal.RawInputAxis = MainAnimal.RawInputAxis;
                SecondAnimal.UseRawInput = MainAnimal.UseRawInput;
                SecondAnimal.Sprint = MainAnimal.sprint;
                SecondAnimal.Rotate_at_Direction = MainAnimal.Rotate_at_Direction;
                SecondAnimal.CurrentSpeedIndex = MainAnimal.CurrentSpeedIndex;
                SecondAnimal.MovementDetected = MainAnimal.MovementDetected;
                SecondAnimal.MovementAxis = MainAnimal.MovementAxis;
            }
        }

        void OnDrawGizmos()
        {
            var RotationPoint = transform.TransformPoint(RotationOffset);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(RotationPoint, 0.05f);
            Gizmos.DrawSphere(RotationPoint, 0.05f);
        }
    }
}