using UnityEngine;

namespace MalbersAnimations.HAP
{
    public class DismountBehavior : StateMachineBehaviour
    {
        private MRider rider;
        private Transform MountPoint;
        private TransformAnimation Fix;
        private Vector3 LastRelativeRiderPosition;
        private float ScaleFactor;

        [Range(0f, 1f)]
        [Tooltip("Search for the ground Layer After this Normalized Time of the Dismount Animation")]
        public float findGroundTime = 0.6f;


        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            MTools.ResetFloatParameters(animator);

            rider = animator.FindComponent<MRider>();

            rider.SetMountSide(0);                                          //Remove the side of the mounted **IMPORTANT*** otherwise it will keep trying to dismount
            ScaleFactor = rider.Montura.Animal.ScaleFactor;                 //Get the scale Factor from the Montura
            MountPoint = rider.Montura.MountPoint;

            Fix = rider.MountTrigger.Adjustment;                            //Get the Fix fo the Dismount

            rider.Start_Dismounting();

            LastRelativeRiderPosition = MountPoint.InverseTransformPoint(rider.transform.position); //Get the Relative position of the Rider Position
        }



        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var transition = animator.GetAnimatorTransitionInfo(layerIndex);
            float deltaTime = animator.updateMode == AnimatorUpdateMode.Fixed ? Time.fixedDeltaTime : Time.deltaTime;

            var TargetRot = animator.rootRotation;

            var TargetPos = MountPoint.TransformPoint(LastRelativeRiderPosition);              //Parent Position without Parenting

            TargetPos += ((Fix ? Fix.delay : 1) * deltaTime * ScaleFactor * animator.velocity); //Position Root Motion Animation (Delta)

            if (rider.Montura)  //Stop the Mountura from walking forward when Dismounting
            {
                if (findGroundTime < stateInfo.normalizedTime &&
                    Physics.Raycast(rider.transform.position + rider.transform.up, -rider.transform.up, out var hit, 1.5f, rider.Montura.Animal.GroundLayer))
                {
                    if (TargetPos.y < hit.point.y)
                        TargetPos = new Vector3(TargetPos.x, hit.point.y, TargetPos.z);
                }

                TargetRot *= rider.Montura.Animal.AdditiveRotation; //Keep Inertia

                //if the Rider is in the Last Transition Put him Up Right 
                if (stateInfo.normalizedTime > 0.5f && animator.IsInTransition(layerIndex))
                {
                    TargetRot = Quaternion.Lerp(TargetRot, Quaternion.FromToRotation(rider.transform.up, Vector3.up) * TargetRot, transition.normalizedTime); //Rotate him UpRight
                }
            }

            LastRelativeRiderPosition = MountPoint.InverseTransformPoint(TargetPos); //Keep the Relative Position of the Last Mounting

            rider.Mount_TargetTransform(TargetPos, TargetRot);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { rider.End_Dismounting(); }
    }
}