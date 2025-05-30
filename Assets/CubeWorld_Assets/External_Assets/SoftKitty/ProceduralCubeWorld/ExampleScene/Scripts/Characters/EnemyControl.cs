using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SoftKitty.PCW.Demo
{
    public class EnemyControl : MonoBehaviour
    {
        #region Variables
        public LayerMask GroundLayer;

        private CharacterController mController;
        private Animator mAni;
        private PathFinding mPathFinding;
        private Vector3 velocity = Vector3.zero;
        private float v = 0F;
        private float h = 0F;
        private float MotionSpeed = 1F;
        private float Speed = 0F;
        private bool FreeFall = false;
        private bool Jump = false;
        private bool Grounded = false;
        private float Gravity = 0F;
        private Quaternion Rot;
        private RaycastHit GroundHit;
        private AnimatorStateInfo st;
        private MapIcon MiniMapIcon;
        #endregion

        #region MonoBehaviour
        void Start()
        {
            Init();
        }

        void Update()
        {
            StateUpdate();
            AiInput();
            AniSet();
            
        }

        void FixedUpdate()
        {
            Movement();
            Rotation();
        }
        private void OnDestroy()
        {
            if (MiniMapIcon != null) MiniMapIcon.Destroy();
        }
        #endregion

        #region internal functions

        void Init()
        {
            mAni = GetComponent<Animator>();
            mPathFinding = GetComponent<PathFinding>();
            mController = GetComponent<CharacterController>();
            MiniMapIcon = MinimapUI.instance.CreateIcon(transform.position, Resources.Load<Texture>("ProceduralCubeWorld/EnemyIcon"));
        }
        void StateUpdate()
        {
            mPathFinding.movingSpeed = Speed;//It is important to let PathFinding script to know the current moving speed.

            st = mAni.GetCurrentAnimatorStateInfo(0);
            if (MiniMapIcon != null) MiniMapIcon.UpdatePos(transform.position);//Update mini map icon position
            if (Physics.SphereCast(transform.position + Vector3.up * 0.5F, mController.radius, Vector3.down, out GroundHit, 10F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                FreeFall = GroundHit.distance > 0.5F + 0.5F;
                Grounded = GroundHit.distance < 0.1F + 0.5F;
            }
            else
            {
                FreeFall = true;
                Grounded = false;
            }
            if (v != 0F || h != 0F)
            {
                Speed = Mathf.MoveTowards(Speed, Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)) * (!Grounded ? 4F : 6F) * MotionSpeed, Time.deltaTime * (!Grounded ? 1F : 5F));
            }
            else
            {
                Speed = Mathf.MoveTowards(Speed, 0F, Time.deltaTime * 10F);
            }
            if (st.IsTag("Jumpping")) Jump = false;
            Gravity = Mathf.MoveTowards(Gravity, -9.8F, 20F * Time.deltaTime);
        }

        void AiInput()
        {
            if (!mPathFinding.hasTarget() && BlockGenerator.instance.Player != null)
            {
                mPathFinding.SetTargetTransform(BlockGenerator.instance.Player.transform);//Set player as moving target
            }

            if (BuildControl.BuildMode) {
               if(mPathFinding.isRunning())mPathFinding.Stop(); //Stop running when in build mode
            }
            else if (!mPathFinding.isRunning())
            {
                mPathFinding.Run(); //Start to run
            }

            if (mPathFinding.movingState == PathFinding.PathFindingState.Idle)
            {
                h = 0F;
                v = 0F;
            }else{
                Vector3 _dir = mPathFinding.desiredPosition - transform.position; //Use desiredPosition to calculate the direction enemy should run to
                _dir.y = 0F;
                _dir.Normalize();
                h = _dir.x;
                v = _dir.z;

                if (mPathFinding.movingState== PathFinding.PathFindingState.Jumping)
                {
                    Gravity = 8F;
                    Jump = true;
                }
            }
           
        }

        void Rotation()
        {
            if (v != 0F || h != 0F)
            {
                Vector3 _forward = Vector3.forward * v + Vector3.right * h;
                _forward.y = 0F;
                Rot = Quaternion.LookRotation(_forward.normalized, Vector3.up);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Rot, Time.fixedDeltaTime * 1000F);
        }

        void Movement()
        {
            if ( Grounded)
            {
                MotionSpeed = mPathFinding.terrainMovingSpeed;
            }
            else
            {
                MotionSpeed = 1F;
            }
            velocity = transform.forward * Speed;
            velocity.y = Gravity;
            mController.Move(velocity * Time.fixedDeltaTime);

        }

        void AniSet()
        {
            mAni.SetFloat("Speed", Speed);
            mAni.SetFloat("SpeedOffset", 0F);
            mAni.SetBool("Jump", Jump);
            mAni.SetBool("Grounded", Grounded);
            mAni.SetBool("FreeFall", FreeFall);
        }

        void OnLand()//called by animation events
        {

        }

        void OnFootstep()//called by animation events
        {

        }
        #endregion
    }
}
