using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MalbersAnimations.Conditions
{
    /// <summary>Conditions to Run on a Object </summary>
    [System.Serializable]
    //public abstract class MCondition<T>  where T : Object
    public abstract class ConditionCore//<T> where T : Object
    {
        [Tooltip("Description of the Condition")]
        [HideInInspector] public string desc = string.Empty;
        [HideInInspector] public bool invert;
        [HideInInspector] public bool OrAnd;
        [HideInInspector] public bool LocalTarget = false;
        private Object CacheTarget;

        [SerializeField, HideInInspector] bool debug;

        public bool DebugCondition => debug;



        //Call this on On Enable on any component
        public virtual void OnEnable() { }

        //Call this on On Disable on any component
        public virtual void OnDisable() { }

        /// <summary>Evaluate a condition using the Target</summary>
        protected abstract bool _Evaluate();

        /// <summary>Evaluate a condition using the Target</summary>
        public bool Evaluate(Object target)
        {
            SetTarget(target);
            return Evaluate();
        }

        /// <summary>Set target correct type on the on the Conditions</summary>
        protected abstract void _SetTarget(Object target);

        public virtual void SetTarget(Object target)
        {
            if (LocalTarget) return; //Do nothing if is Local Target

            //If the target is different from the last one
            if (CacheTarget != target)
            {
                CacheTarget = target;
                _SetTarget(target);
            }
        }

        public bool Evaluate() => invert ? !_Evaluate() : _Evaluate();
        // protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);

        /// <summary> Optional Method to Draw Gizmos on the Scene View </summary>
        public virtual void DrawGizmos(Component target) { }

        /// <summary> Optional Method to Draw GizmosOnSelected on the Scene View </summary>
        public virtual void DrawGizmosSelected(Component target) { }


        public virtual void Debugging(string data, bool Value, Object target)
        {
            if (debug)
            {
                var color = Value ? "cyan" : "orange";

                var inverted = invert ? "<B><color=red>[INVERTED]</color></B>" : string.Empty;

                Value = invert ? !Value : Value;

                MDebug.Log($"Condition: <B>[{GetType().Name}]</B> [{data}] Result: <color={color}> <B>{Value}</B> </color> {inverted}", target);
            }
        }
    }



    public sealed class ConditionDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public ConditionDescriptionAttribute(string menuName)
        {
            Description = menuName;
        }
    }

}