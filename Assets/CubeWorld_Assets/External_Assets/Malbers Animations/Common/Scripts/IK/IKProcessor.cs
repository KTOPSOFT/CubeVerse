using System;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [Serializable]
    public abstract class IKProcessor
    {
        [HideInInspector] public string name;
        [HideInInspector] public bool Active = true;

        [Tooltip("Weight Applied for the Processor")]
        [HideInInspector][Range(0, 1)] public float Weight = 1;

        [Tooltip("Target transform reference from the IK Set [Targets Array]. Index Value. Target applied to the Avatar IK Goal")]
        [HideInInspector][Min(-1)] public int TargetIndex = 0;

        [HideInInspector] public string AnimParameter;
        [HideInInspector] public int AnimParameterHash;

        /// <summary> Tells the IK Manager if it needs Targets to check</summary>
        public abstract bool RequireTargets { get; }

        public virtual void Start(IKSet IKSet, Animator anim, int index) { }

        public virtual void OnEnable(IKSet IKSet, Animator anim, int index) { }

        public virtual void OnDisable(IKSet IKSet, Animator anim, int index) { }


        public virtual void OnAnimatorIK(IKSet IKSet, Animator anim, int index, float weight) { }
        public virtual void LateUpdate(IKSet IKSet, Animator anim, int index, float weight) { }
        public virtual void OnDrawGizmos(IKSet IKSet, Animator anim, float weight) { }

        ///<summary> Verify if the IKProcessor is set correctly.If it needs some references </summary>
        public abstract void Validate(IKSet set, Animator animator, int index);

        /// <summary> Process the Animation Curve in case there's one in the IK Processor</summary>
        public float GetProcessorAnimWeight(Animator animator)
            => AnimParameterHash != 0 ? animator.GetFloat(AnimParameterHash) : 1;

        internal virtual void OnSceneGUI(IKSet set, Animator animator, UnityEngine.Object target, int index) { }
    }


    public enum UpVectorType { VectorUp, Local, Global }


}
