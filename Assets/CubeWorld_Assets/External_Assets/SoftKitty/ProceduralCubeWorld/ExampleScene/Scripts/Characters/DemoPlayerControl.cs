using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SoftKitty.PCW.Demo
{
    public class DemoPlayerControl : MonoBehaviour
    {
        #region public settings
        public LayerMask GroundLayer;
        public AudioClip [] FootstepSound;
        public AudioClip LandSound;
        public AudioClip JumpSound;
        public ParticleSystem FootstepPar;
        public ParticleSystem WaterPar;
        #endregion

        #region private variables
        private float Speed = 0F;
        private AudioSource mAudio;
        private CharacterController mController;
        private Animator mAni;
        private Camera mCam;
        private Vector3 velocity = Vector3.zero;
        private float v = 0F;
        private float h = 0F;
        private float MotionSpeed = 1F;
        private float Friction = 1F;
        private float SpeedWithFriction = 0F;
        private bool FreeFall = false;
        private bool Jump = false;
        private bool Grounded = false;
        private float Gravity = 0F;
        private Quaternion Rot;
        private RaycastHit GroundHit;
        private AnimatorStateInfo st;
        private Vector3 CamForward;
        private BlockInfo StandingCube;
        private bool inWater = false;
        #endregion 

        #region monoBehaviours
        void Start()
        {
            Init();
        }

        void Update()
        {
            StateUpdate();
            InputControl();
            CameraEffect();
            WaterCheck();
            AniSet();
            
        }

        void FixedUpdate()
        {
            Movement();
            Rotation();
        }
        #endregion

        #region public variables
        public float GetSpeed()
        {
            return SpeedWithFriction;
        }

        public bool isInWater()
        {
            return inWater;
        }
        #endregion

        #region internal functions
        void Init()
        {
            mAni = GetComponent<Animator>();
            mAudio = GetComponent<AudioSource>();
            mController = GetComponent<CharacterController>();
            mCam = GetComponentInChildren<Camera>();
            mCam.GetComponentInParent<CameraControl>().transform.SetParent(null);
            mCam.GetComponentInParent<CameraControl>().FollwingTarget = transform;
        }
        void StateUpdate()
        {
            if (Time.frameCount % 30 == 0 && Grounded) StandingCube = BlockGenerator.instance.GetCubeByPosition(transform.position);
            st = mAni.GetCurrentAnimatorStateInfo(0);
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
            if (st.IsTag("Jumpping")) Jump = false;
            SpeedWithFriction = Mathf.MoveTowards(SpeedWithFriction, Speed, Time.deltaTime * 60F * Friction * Friction * Friction);
            Gravity = Mathf.MoveTowards(Gravity, -9.8F, 20F * Time.deltaTime);
        }

        void CameraEffect()
        {
            if (st.IsTag("Jumpping"))
            {
                CameraControl.FovAdd = Mathf.Lerp(CameraControl.FovAdd, 15F, Time.deltaTime * 3F);
                CameraControl.ApertureAdd = Mathf.Lerp(CameraControl.ApertureAdd, 1.5F, Time.deltaTime * 3F);
            }
            else if (Speed > 4.5F)
            {
                CameraControl.FovAdd = Mathf.Lerp(CameraControl.FovAdd, 10F, Time.deltaTime * 2F);
                CameraControl.ApertureAdd = Mathf.Lerp(CameraControl.ApertureAdd, 1.5F, Time.deltaTime * 2F);
            }
        }

        void WaterCheck()
        {
            if ((inWater && transform.position.y > 0F) || (!inWater && transform.position.y <= 0F))
            {
                Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/WaterSpray"), new Vector3(transform.position.x, 0F, transform.position.z), Quaternion.identity);
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Water"), transform.position);
            }
            inWater = transform.position.y <= 0F;
            if (transform.position.y <= -0.4F)
            {
                if (!WaterPar.isPlaying) WaterPar.Play();
            }
            else
            {
                if (WaterPar.isPlaying) WaterPar.Stop();
            }
        }
       

        void InputControl()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                h = Input.GetAxis("Horizontal");
                v = Input.GetAxis("Vertical");
                if (Input.GetKeyDown(KeyCode.Space) && Grounded && Gravity < 0F)
                {
                    DoJump();
                }
            }
            else
            {
                h = 0F;
                v = 0F;
            }

            if (v != 0F || h != 0F)
            {
                Speed = Mathf.MoveTowards(Speed, Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)) * (!Grounded ? 4F : 6F) * MotionSpeed, Time.deltaTime * (!Grounded ? 1F : 5F));
            }
            else
            {
                Speed = Mathf.MoveTowards(Speed, 0F, Time.deltaTime * 10F);
            }
            
        }

        void DoJump()
        {
            Gravity = 12F;
            Jump = true;
            EmitFootstepParticle();
            mAudio.PlayOneShot(JumpSound);
        }

        void Rotation()
        {
            if (v != 0F || h != 0F)
            {
                Vector3 _camForward = mCam.transform.forward;
                _camForward.y = 0F;
                CamForward = Vector3.MoveTowards(CamForward, _camForward.normalized, Time.fixedDeltaTime * 3F);
                Vector3 _forward = CamForward * v + Vector3.Cross(Vector3.up, CamForward) * h;
                _forward.y = 0F;
                Rot = Quaternion.LookRotation(_forward.normalized, Vector3.up);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Rot, Time.fixedDeltaTime * 500F);
        }



        void Movement()
        {
            if (StandingCube != null && Grounded)
            {
                MotionSpeed = inWater ? BlockGenerator.instance.MoveSpeedInWater : StandingCube._walkableSpeed * 0.01F;
                Friction = StandingCube._friction * 0.01F;
            }
            else
            {
                MotionSpeed = 1F;
                Friction = 1F;
            }
            velocity = transform.forward * SpeedWithFriction;
            velocity.y = Gravity;
            mController.Move(velocity * Time.fixedDeltaTime);
        }

        void AniSet()
        {
            mAni.SetFloat("Speed", Speed);
            mAni.SetFloat("SpeedOffset", Mathf.Clamp01(SpeedWithFriction-Speed-0.1F));
            mAni.SetBool("Jump", Jump);
            mAni.SetBool("Grounded", Grounded);
            mAni.SetBool("FreeFall", FreeFall);
        }

        void OnLand()//called by animation events
        {
            mAudio.PlayOneShot(LandSound);
            EmitFootstepParticle();
        }

        void OnFootstep()//called by animation events
        {
            mAudio.PlayOneShot(FootstepSound[Random.Range(0,FootstepSound.Length)]);
            if (Friction >= 1F) EmitFootstepParticle();
            
        }
        void EmitFootstepParticle()
        {
            if (inWater) return;
            if (StandingCube != null)
            {
                FootstepPar.startColor = Color.Lerp(Color.white, StandingCube._topColor, 0.8F);
            }
            FootstepPar.Emit(Random.Range(2, 5));
        }
        #endregion
    }
}
