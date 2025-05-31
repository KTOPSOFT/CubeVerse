using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>  Based on 3DKit Controller from Unity  </summary>
    [AddComponentMenu("Malbers/Utilities/Transform/Simple Rotator")]
    [SelectionBase]
    public class MSimpleRotator : MSimpleTransformer
    {
        public Vector3Reference axis = new(Vector3.up);

        [ContextMenuItem("Invert", nameof(Invert_Start_End))]
        public FloatReference startAngle;
        [ContextMenuItem("Invert", nameof(Invert_Start_End))]
        public FloatReference endAngle = new(90f);

        private float difference;


        private void Awake()
        {
            Inverted = false;
            difference = endAngle - startAngle;
        }


        public override void Evaluate(float value)
        {
            var curvePosition = m_Curve.Evaluate(value);
            var q = Quaternion.AngleAxis(Mathf.LerpUnclamped(startAngle, endAngle, curvePosition), axis);
            Object.localRotation = q;
        }


        /// <summary> When using Additive the rotation will continue from the last position  </summary>
        protected override void Pre_End()
        {
            if (loopType == LoopType.Once && endType == EndType.Additive)
            {
                startAngle.Value = endAngle.Value; //use the end value as start value
                endAngle.Value += difference;
            }
        }


        protected override void Pos_End()
        {
            if (loopType == LoopType.Once && endType == EndType.Invert)
                Invert_Start_End();
        }


        [ContextMenu("Invert Value")]
        public void Invert_Value()
        {
            //if (!enabled) return; //Do not invert while disabled
            if (Playing) { Debug.Log("Cannot invert value while playing"); return; } //Do not invert while playing

            Inverted ^= true;
            difference *= -1;
            endAngle.Value = startAngle.Value + difference;

            //Debug.Log("Rotation Value Inverted");
        }


        [ContextMenu("Invert Value +")]
        public void Invert_Value_Positive() { if (Inverted) Invert_Value(); }


        [ContextMenu("Invert Value -")]
        public void Invert_Value_Negative() { if (!Inverted) Invert_Value(); }


        [ContextMenu("Invert Start - End")]
        public void Invert_Start_End()
        {
            (startAngle.Value, endAngle.Value) = (endAngle.Value, startAngle.Value);
            value = 0;
            Evaluate(0);
            MTools.SetDirty(this);
        }
    }
}