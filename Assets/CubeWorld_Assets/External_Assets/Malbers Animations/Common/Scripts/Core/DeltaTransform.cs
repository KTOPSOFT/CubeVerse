using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>  Used for storing the on a given time all the position, rotation, scale of an object given a transform. </summary>
    [System.Serializable]
    public struct TransformValues
    {
        public Vector3 position;
        public Vector3 localPosition;

        public Quaternion rotation;
        public Quaternion localRotation;

        public Vector3 eulerAngles;
        public Vector3 localEulerAngles;

        public Vector3 lossyScale;
        public Vector3 localScale;

        /// <summary>Store all the position rotations and scale of a transform </summary>
        public TransformValues(Transform transform)
        {
            if (transform != null)
            {
                transform.GetPositionAndRotation(out position, out rotation);
                transform.GetLocalPositionAndRotation(out localPosition, out localRotation);

                eulerAngles = transform.eulerAngles;
                localEulerAngles = transform.localEulerAngles;
                lossyScale = transform.lossyScale;
                localScale = transform.localScale;
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                localPosition = Vector3.zero;
                localRotation = Quaternion.identity;
                eulerAngles = Vector3.zero;
                localEulerAngles = Vector3.zero;
                lossyScale = Vector3.one;
                localScale = Vector3.one;
            }
        }

        public readonly void RestoreTransform(Transform transform)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = localScale;
        }

        public readonly void RestoreLocalTransform(Transform transform)
        {
            transform.SetLocalPositionAndRotation(localPosition, localRotation);
            transform.localScale = localScale;
        }
    }


    /// <summary> Used to store Transform pos, rot and scale values </summary>
    [System.Serializable]
    public struct TransformOffset
    {
        [Tooltip("Local Position")]
        public Vector3 Position;
        [Tooltip("Local rotation Euler")]
        public Vector3 Rotation;
        [Tooltip("Local Scale")]
        public Vector3 Scale;

        public TransformOffset(int _)
        {
            Position = Vector3.zero;
            Rotation = Vector3.zero;
            Scale = Vector3.one;
        }

        public TransformOffset(Transform def)
        {
            Position = def.localPosition;
            Rotation = def.localEulerAngles;
            Scale = def.localScale;
        }

        public readonly void RestoreTransform(Transform def)
        {
            def.localPosition = Position;
            def.localEulerAngles = Rotation;
            def.localScale = Scale;
        }

        public readonly void SetOffset(Transform t)
        {
            t.localPosition = Position;
            t.localEulerAngles = Rotation;
            t.localScale = Scale;
        }
    }
}