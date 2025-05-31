using UnityEngine;
using UnityEngine.Splines;

namespace MalbersAnimations.Utilities
{
    /// <summary> Based on 3DKit Controller from Unity </summary>
    [AddComponentMenu("Malbers/Utilities/Transform/Spline Translator")]
    [SelectionBase]
    public class SplineTranslator : MSimpleTransformer
    {
        [Tooltip("Attach a Unity Spline here!")]
        [RequiredField] public SplineContainer spline;
        public Vector3 offset;

        public float Start = 0;
        public float End = 1;

        [Tooltip("Use the spline tangent to rotate the object along the trajectory")]
        public bool RotateAlongTanget = true;

        [Tooltip("Clear the Y Rotation of the Tangent")]
        public bool ClearYTangent = false;

        [Hide(nameof(RotateAlongTanget))]
        [Tooltip("Use the spline's up vector to orient the object.")]
        public bool UseSplineUpVector = true;

        float difference;

        private void Awake()
        {
            Inverted = false;
            difference = End - Start; //Calculate the difference between the start and end value
        }


        public override void Evaluate(float curveValue)
        {
            if (spline == null) return;

            if (this.Object != null)
            {
                var curvePosition = Mathf.Lerp(Start, End, m_Curve.Evaluate(curveValue)) % 1;

                Vector3 newPosition = spline.EvaluatePosition(curvePosition);

                if (RotateAlongTanget)
                {
                    Vector3 tangent = spline.EvaluateTangent(curvePosition);

                    if (ClearYTangent) tangent = Vector3.ProjectOnPlane(tangent, Vector3.up);

                    Vector3 UpVector = UseSplineUpVector ? spline.EvaluateUpVector(curvePosition) : Vector3.up;
                    Quaternion newRotation = Quaternion.LookRotation(tangent, UpVector);

                    this.Object.rotation = newRotation * Quaternion.Euler(offset);
                }

                if (float.IsNaN(newPosition.x)) return;
                this.Object.position = newPosition;
            }
        }


        /// <summary> When using Additive the rotation will continue from the last position  </summary>
        protected override void Pre_End()
        {
            if (loopType == LoopType.Once && endType == EndType.Additive)
            {
                Start += difference; //use the end value as start value
                End += difference;
            }
        }

        protected override void Pos_End()
        {
            if (loopType == LoopType.Once && endType == EndType.Invert)
                InvertStartEnd();
        }

        [ContextMenu("Invert Value")]
        public void Invert_Value()
        {
            // if (!enabled) return; //Do not invert while disabled
            if (Playing) { Debug.Log("Cannot invert value while playing"); return; } //Do not invert while playing
            Inverted ^= true;
            difference *= -1;
            End = Start + difference;
        }


        [ContextMenu("Invert Value +")]
        public void Invert_Value_Positive() { if (Inverted) Invert_Value(); }

        [ContextMenu("Invert Value -")]
        public void Invert_Value_Negative() { if (!Inverted) Invert_Value(); }

        [ContextMenu("Invert Start - End")]
        public void InvertStartEnd()
        {
            (Start, End) = (End, Start);
            value = 0;
            Evaluate(0);
            MTools.SetDirty(this);
        }
    }
}
