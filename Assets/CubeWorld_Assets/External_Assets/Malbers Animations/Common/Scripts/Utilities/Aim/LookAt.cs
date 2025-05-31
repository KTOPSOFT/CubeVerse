using MalbersAnimations.Scriptables;
using UnityEngine;
using MalbersAnimations.Events;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>Used for Animal that have Animated Physics enabled </summary>
    [DefaultExecutionOrder(500)/*,[RequireComponent(typeof(Aim))*/]
    [AddComponentMenu("Malbers/Utilities/Aiming/Look At")]
    public class LookAt : MonoBehaviour, IAnimatorListener, ILookAtActivation
    {
        [System.Serializable]
        public class BoneRotation
        {
            /// <summary> Transform reference for the Bone </summary>
            [RequiredField] public Transform bone;                                          //The bone
            public Vector3 offset = new(0, -90, -90);               //The offset for the look At
            [Range(0, 1)] public float weight = 1;                          //the Weight of the look a
                                                                            // internal Quaternion nextRotation;
                                                                            //internal Quaternion UpdateRotation;
            internal Quaternion defaultRotation;

            [Tooltip("Is not a bone driven by the Animator")]
            public bool external;
        }

        private const float CloseToZero = 0.005f;

        public BoolReference active = new(true);     //For Activating and Deactivating the HeadTrack

        [Tooltip("Enable this if your Animator uses Animate physics loop")]
        public bool AnimatePhysics = (true);     //For Activating and Deactivating the HeadTrack

        private IGravity a_UpVector;

        [Tooltip("Reference for the Aim Component")]
        [RequiredField] public Aim aimer;


        [Tooltip("Limits the Look At from the Min to Max Value")]
        public RangedFloat LookAtLimit = new(90, 120);

        [Tooltip("Track an animator Paramter to multiply its value to the weight of the Look At")]
        public StringReference TrackParameter = new("LookAt");

        private int TrackParameterHash;

        //[Space, Tooltip("Max Angle to LookAt")]
        //public FloatReference ExitAngle = new(15f);

        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Smoothness between Enabled and Disable")]
        public FloatReference Lerp = new(5f);

        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Use the LookAt only when there's a Force Target on the Aim... use this when the Animal is AI Controlled")]
        [SerializeField] private BoolReference onlyTargets = new(false);

        [Space]
        public BoneRotation[] Bones;      //Bone Chain    
                                          // private  Quaternion[] LocalRot;      //Bone Chain    

        public BoolEvent OnLookAtActive = new();


        public bool debug = true;
        [Hide(nameof(debug))]
        public float GizmoRadius = 1f;
        public float LookAtWeight { get; private set; }
        /// <summary>Angle created between the transform.Forward and the LookAt Point   </summary>
        public float Angle { get; private set; }

        /// <summary>Means there's a camera or a Target to look At</summary>
        public bool HasTarget { get; set; }
        public Vector3 UpVector => a_UpVector != null ? a_UpVector.UpVector : Vector3.up;


        private Transform EndBone;

        /// <summary>Direction Stored on the Aim Script</summary>
        public Vector3 AimDirection => aimer.AimDirection;

        private bool isAiming;
        /// <summary>Check if is on the Angle of Aiming</summary>
        public bool IsAiming
        {
            get
            {
                var check = Active && CameraAndTarget && ActiveByAnimation;

                if (check != isAiming)
                {
                    isAiming = check;
                    OnLookAtActive.Invoke(isAiming);
                }
                return isAiming;
            }
        }



        /// <summary> Enable Disable the Look At </summary>
        public bool Active
        {
            get => active;
            set => active.Value = value;
        }


        //bool activebyAnim;
        /// <summary> Enable/Disable the LookAt by the Animator</summary>
        public Animator Anim { get; set; }
        public bool ActiveByAnimation { get; set; }
        //{
        //    get => activebyAnim;
        //    set 
        //    {
        //        activebyAnim = value;
        //        Debug.Log($"activebyAnim {activebyAnim}");
        //    }

        //}

        /// <summary>The Character is using a Camera to Look?</summary>
        bool CameraAndTarget { get; set; }

        /// <summary>When set to True the Look At will only function with Targets gameobjects only instead of the Camera forward Direction</summary>
        public bool OnlyTargets { get => onlyTargets.Value; set => onlyTargets.Value = value; }

        void Awake()
        {
            a_UpVector = gameObject.FindInterface<IGravity>(); //Get Up Vector

            if (aimer == null)
                aimer = gameObject.FindComponent<Aim>();  //Get the Aim Component

            Anim = gameObject.FindComponent<Animator>();

            if (Anim != null)
            {
                if (MTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, TrackParameter.Value))
                {
                    TrackParameterHash = Animator.StringToHash(TrackParameter.Value); //Cache the Animator parameter to inspect it later on the LateUpdate
                }
            }



            aimer.IgnoreTransform = transform;
            ActiveByAnimation = true;
            EnablePriority = 1;
            foreach (var item in Bones)
            {
                if (item.bone == null)
                {
                    Debug.LogError($"LookAt in [{name}] has missing/empty bones. Please fill the reference. Disabling [LookAt]", this);
                    enabled = false;
                    break;
                }
            }
        }

        void OnEnable()
        {
            if (Bones != null && Bones.Length > 0)
                EndBone = Bones[^1].bone;

            if (aimer.AimOrigin == null || aimer.AimOrigin == EndBone)
                aimer.AimOrigin = Bones[0].bone.parent;

            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].defaultRotation = Bones[i].bone.localRotation; //Save the Local Rotation of the Bone
            }

            if (AnimatePhysics)
                StartCoroutine(SolveLookAt());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator SolveLookAt()
        {
            var fixedUp = new WaitForFixedUpdate();

            while (true)
            {
                yield return fixedUp;
                DoLateUpdateLookAt(Time.fixedDeltaTime);
            }
        }

        private void ResetBoneLocalRot()
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].bone.localRotation = Bones[i].defaultRotation; //Save the Local Rotation of the Bone
            }
        }

        void LateUpdate()
        {
            if (!AnimatePhysics)
                DoLateUpdateLookAt(Time.deltaTime);
        }

        private void DoLateUpdateLookAt(float time)
        {

            // if (Time.time < float.Epsilon || Time.timeScale <= 0) return; //Do not look when the game is paused

            if (!aimer.UseCamera && aimer.AimTarget == null) { CameraAndTarget = false; }
            else
            {
                //If Only Target is true then Disable it because we do not have any target
                if (OnlyTargets)
                {
                    CameraAndTarget = (aimer.AimTarget != null);
                }
                //If Only Target is false and there's no Camera then Disable it because we do not have any target
                else
                {
                    CameraAndTarget = (aimer.MainCamera != null) || !aimer.UseCamera;
                }
            }


            Angle = Vector3.Angle(transform.forward, AimDirection);
            LookAtWeight = Mathf.Lerp(LookAtWeight, IsAiming ? 1 : 0, time * Lerp);

            if (LookAtLimit.maxValue != 0 && LookAtLimit.minValue != 0) //Check the Limit in case there is a limit
                LookAtWeight = Mathf.Min(LookAtWeight, Angle.CalculateRangeWeight(LookAtLimit.minValue, LookAtLimit.maxValue));

            //Multiply the LookAtWeight by the Animator Parameter
            if (TrackParameterHash != 0)
            {
                var track = Anim.GetFloat(TrackParameterHash);
                LookAtWeight *= track;
            }

            if (LookAtWeight == 0) return;

            LookAtBoneSet_AnimatePhysics2();            //Rotate the bones


            if (LookAtWeight <= CloseToZero) { LookAtWeight = 0; return; }//Do nothing on Weight Zero
        }


        /// <summary>Rotates the bones to the Look direction for FIXED UPTADE ANIMALS</summary>
        void LookAtBoneSet_AnimatePhysics2()
        {
            if (AimDirection == Vector3.zero) return; //Skip Rotation to zero

            for (int i = 0; i < Bones.Length; i++)
            {
                var bn = Bones[i];

                if (!bn.bone) continue;

                if (LookAtWeight != 0)
                {
                    var weight = Mathf.SmoothStep(0, 1, LookAtWeight);

                    var TargetTotation = Quaternion.LookRotation(AimDirection, transform.up) * Quaternion.Euler(bn.offset);

                    if (bn.external)
                        bn.bone.localRotation = Quaternion.Lerp(bn.defaultRotation, TargetTotation, weight);
                    else
                        bn.bone.rotation = Quaternion.Lerp(bn.bone.rotation, TargetTotation, weight);
                }
            }
        }


        ///// <summary>Rotates the bones to the Look direction for FIXED UPTADE ANIMALS</summary>
        //void LookAtBoneSet_AnimatePhysics()
        //{
        //    // CalculateAiming();

        //    for (int i = 0; i < Bones.Length; i++)
        //    {
        //        var bn = Bones[i];

        //        if (!bn.bone) continue;

        //        if (IsAiming)
        //        {
        //            var BoneAim = Vector3.Slerp(transform.forward, AimDirection, bn.weight).normalized;
        //            var TargetTotation = Quaternion.LookRotation(BoneAim, UpVector) * Quaternion.Euler(bn.offset);
        //            bn.nextRotation = Quaternion.Lerp(bn.nextRotation, TargetTotation, LookAtWeight);
        //        }
        //        else
        //        {
        //            if (!bn.external)
        //            {
        //                bn.nextRotation = Quaternion.Lerp(bn.bone.rotation, bn.nextRotation, LookAtWeight);
        //            }
        //            // if (LookAtWeight ==0)  bn.nextRotation = bn.bone.rotation;
        //        }

        //        if (LookAtWeight != 0)
        //        {
        //            if (bn.external && !IsAiming)
        //            {
        //                bn.nextRotation = Quaternion.Lerp(bn.nextRotation, bn.defaultRotation, 1 - LookAtWeight);
        //                bn.bone.localRotation = Quaternion.Lerp(bn.bone.localRotation, bn.nextRotation, LookAtWeight); //LOCAL ROTATION!!!????

        //            }
        //            else
        //            {
        //                bn.bone.rotation = bn.nextRotation;
        //            }
        //        }
        //    }
        //}

        /// <summary>Enable Look At from the Animator (Needs Layer)</summary>
        public void EnableLookAt(int layer) => EnableByPriority(layer + 1);

        /// <summary>Disable Look At from the Animator (Needs Layer)</summary>
        public void DisableLookAt(int layer) => DisableByPriority(layer + 1);

        public virtual void SetTargetOnly(bool val) => OnlyTargets = val;

        public virtual void EnableByPriority(int priority)
        {
            if (priority >= DisablePriority)
            {
                EnablePriority = priority;
                if (DisablePriority == EnablePriority) DisablePriority = 0;
            }
            ActiveByAnimation = (EnablePriority > DisablePriority);

            //Debug.Log("ActiveByAnimation: "+ ActiveByAnimation);
        }

        public virtual void ResetByPriority(int priority)
        {
            if (EnablePriority == priority) EnablePriority = 0;
            if (DisablePriority == priority) DisablePriority = 0;

            ActiveByAnimation = (EnablePriority > DisablePriority);
        }


        public virtual void DisableByPriority(int priority)
        {
            if (priority >= EnablePriority)
            {
                DisablePriority = priority;
                if (DisablePriority == EnablePriority) EnablePriority = 0;
            }

            // Debug.Log("Dis");
            ActiveByAnimation = (EnablePriority > DisablePriority);
        }


        //bool OverridePriority;
        //bool lastActivation;  
        public int EnablePriority { get; private set; }
        public int DisablePriority { get; private set; }

        //private int[] LayersPriority = new int[20];





        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        void OnValidate()
        {
            if (Bones != null && Bones.Length > 0)
            {
                EndBone = Bones[^1].bone;
            }
        }

        void Reset()
        {
            aimer = gameObject.FindInterface<Aim>();
            if (aimer == null) aimer = gameObject.AddComponent<Aim>();
        }


#if UNITY_EDITOR && MALBERS_DEBUG
        private void OnDrawGizmosSelected()
        {
            if (UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) //Show Gizmos only when the Inspector is Open
            {
                bool AppIsPlaying = Application.isPlaying;

                if (debug && enabled)
                {
                    Handles.color = IsAiming || !AppIsPlaying ? new Color(0, 1, 0, 0.05f) : new Color(1, 0, 0, 0.05f);

                    if (EndBone != null)
                    {
                        var UpVector = this.UpVector;

                        Handles.color = new Color(0, 1, 0, 0.1f);
                        Handles.DrawSolidArc(EndBone.position, UpVector,
                            Quaternion.Euler(0, -LookAtLimit.minValue, 0) * transform.forward, LookAtLimit.minValue * 2, GizmoRadius);


                        Handles.color = Color.green;
                        Handles.DrawWireArc(EndBone.position,
                            UpVector, Quaternion.Euler(0, -LookAtLimit.minValue, 0) * transform.forward, LookAtLimit.minValue * 2, GizmoRadius);


                        Handles.color = new Color(0, 0.3f, 0, 0.2f);
                        var Maxlimit = (LookAtLimit.minValue - LookAtLimit.maxValue);

                        Handles.DrawSolidArc(EndBone.position,
                            UpVector, Quaternion.Euler(0, -(LookAtLimit.minValue), 0) * transform.forward, (Maxlimit), GizmoRadius);

                        Handles.DrawSolidArc(EndBone.position,
                            UpVector, Quaternion.Euler(0, (LookAtLimit.minValue), 0) * transform.forward, -(Maxlimit), GizmoRadius);


                        Handles.color = Color.black;

                        Handles.DrawWireArc(EndBone.position,
                            UpVector, Quaternion.Euler(0, -(LookAtLimit.minValue), 0) * transform.forward, (Maxlimit), GizmoRadius);

                        Handles.DrawWireArc(EndBone.position,
                            UpVector, Quaternion.Euler(0, (LookAtLimit.minValue), 0) * transform.forward, -(Maxlimit), 1);

                    }
                }
            }
        }
#endif
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(LookAt))]
    public class LookAtED : Editor
    {
        LookAt M;
        void OnEnable()
        {
            M = (LookAt)target;
        }

        public override void OnInspectorGUI()
        {

            if (M.aimer && M.Bones != null)
            {
                var origin = M.aimer.AimOrigin;

                if (origin == null)
                {
                    EditorGUILayout.HelpBox($" Please add a Aim Origin to the Aimer Component", MessageType.Error);
                }
                else
                    foreach (var bn in M.Bones)
                    {
                        if (bn.bone != null && origin.SameHierarchy(bn.bone))
                        {
                            EditorGUILayout.HelpBox($"Aimer Origin [{origin.name}] is child of [{bn.bone.name}]." +
                                $" Use a different Aimer Origin that is not child of [{bn.bone.name}]", MessageType.Error);
                            break;
                        }
                    }
            }
            base.OnInspectorGUI();

            if (Application.isPlaying && M.debug)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.FloatField("LookAtWeight", M.LookAtWeight);
                    EditorGUILayout.FloatField("Current Angle", M.Angle);
                    EditorGUILayout.Toggle("Active by Animation", M.ActiveByAnimation);
                    EditorGUILayout.IntField("Enable Priority", M.EnablePriority);
                    EditorGUILayout.IntField("Disable Priority", M.DisablePriority);
                    Repaint();
                }
            }
        }
    }
#endif
}