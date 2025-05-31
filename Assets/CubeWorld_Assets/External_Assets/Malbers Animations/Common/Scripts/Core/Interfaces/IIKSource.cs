

using MalbersAnimations.IK;
using UnityEngine;

namespace MalbersAnimations
{
    public interface IIKSource
    {
        /// <summary>Owner of the IK Source</summary>
        public Transform Owner { get; }

        /// <summary> Activate an IK Set </summary>
        /// <param name="set">Name of the set</param>
        void Set_Enable(string set);

        /// <summary> Activate an IK Set </summary>
        /// <param name="set">Name of the set</param>
        /// <param name="value">enable/disable the Set</param>
        void Set_Enable(string set, bool value);

        /// <summary> Activate an IK Set </summary>
        /// <param name="set">Name of the set</param>
        /// <param name="value">enable/disable the Set</param>
        void Set_Weight(string set, bool value);

        /// <summary> Deactivate an IK Set </summary>
        /// <param name="set">name of the IKset</param>
        void Set_Disable(string set);

        /// <summary> Set all the targets on an IKSet </summary>
        /// <param name="set">Name of the set</param>
        /// <param name="targets">array of Transfom targets</param>
        void Target_Set(string set, Transform[] targets);

        /// <summary> Clear  all the targets on an IKSet </summary>
        /// <param name="set">Name of the set</param>
        void Target_Clear(string set);

        /// <summary> Set a the Target on an IKSet Targets Array</summary>
        /// <param name="set">Name of the set</param>
        /// <param name="newTarget">New Target to add to the Targets Array</param>
        /// <param name="index">index on the Targets Array</param>
        void Target_Set(string set, Transform newTarget, int index);

        /// <summary> Clear a the Target on an IKSet Targets Array</summary>
        /// <param name="set">Name of the set</param>
        /// <param name="index">index on the Targets Array</param>
        void Target_Clear(string set, int index);
        IKSet FindSet(string set);
    }
}
