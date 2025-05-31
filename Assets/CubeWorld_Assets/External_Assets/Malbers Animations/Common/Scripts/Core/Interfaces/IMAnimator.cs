using System;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Interface used to set Parameters to an Animator</summary>
    public interface IMAnimator
    {
        /// <summary>Sets a Bool Parameter on the Animator using the parameter Hash</summary>
        Action<int, bool> SetBoolParameter { get; set; }
        /// <summary>Sets a float Parameter on the Animator using the parameter Hash</summary>
        Action<int, float> SetFloatParameter { get; set; }

        /// <summary>Sets a Integer Parameter on the Animator using the parameter Hash</summary> 
        Action<int, int> SetIntParameter { get; set; }

        /// <summary>Sets a Trigger Parameter on the Animator using the parameter Hash</summary> 
        Action<int> SetTriggerParameter { get; set; }

        /// <summary>Set a int on the Animator</summary>
        void SetAnimParameter(int hash, int value);

        /// <summary>Set a float on the Animator</summary>
        void SetAnimParameter(int hash, float value);

        /// <summary>Set a Bool on the Animator</summary>
        void SetAnimParameter(int hash, bool value);

        /// <summary>Set a Trigger on the Animator</summary>
        void SetAnimParameter(int hash);
    }


    /// <summary>  Recieve messages from the Animator State Machine Behaviours using MessageBehaviour  </summary>
    public interface IAnimatorListener
    {
        Transform transform { get; }


        /// <summary> Recieve messages from the Animator State Machine Behaviours </summary>
        /// <param name="message">The name of the method</param>
        /// <param name="value">the parameter</param>
        bool OnAnimatorBehaviourMessage(string message, object value);

        //ADD THIS METHOD TO THE IMPLEMENTATION WHEN YOU USE THIS INTERFACE
        //public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

    }

    /// <summary> Interface used for Syncing Locomotion Animations .. (E.g. Rider Horse or Horse and Wings) </summary>
    public interface IAnimatorStateCycle
    {
        /// <summary>Broadcast when the Animal change states</summary>
        System.Action<int> StateCycle { get; set; }
    }


    /// <summary>  Use this interface on your Monobehaviours to  Read the Curves from the Animator Behaviour</summary>
    public interface IAnimatorCurve
    {
        void AnimatorCurve(int ID, float value);
    }
}
