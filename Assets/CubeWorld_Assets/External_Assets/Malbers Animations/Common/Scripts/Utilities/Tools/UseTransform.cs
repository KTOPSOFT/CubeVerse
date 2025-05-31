using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Use (Follow) Transform")]

    public class UseTransform : MonoBehaviour
    {
        public enum UpdateMode                                          // The available methods of updating are:
        {
            Update = 1,
            FixedUpdate = 2,                                            // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate = 4,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
        }

        public enum XYZEnum { X = 1, Y = 2, Z = 4 }

        [Tooltip("Transform to use the Position as Reference")]
        public TransformReference reference = new();

        [Tooltip("Transform to use the Position as Reference (OBSOLETE!)")]
        [HideInInspector] public Transform Reference;

        [Tooltip("Use the Reference's Position")]
        public bool position = true;
        [Hide(nameof(position))]
        public UpdateMode PositionUpdate = UpdateMode.FixedUpdate;

        [Hide(nameof(position))]
        [Flag]
        public XYZEnum posAxis = XYZEnum.X | XYZEnum.Y | XYZEnum.Z;
        [Hide(nameof(position))]
        [Min(0)] public float lerpPos = 0f;

        [Tooltip("Use the Reference's Rotation")]
        public bool rotation = true;

        [Hide(nameof(rotation))] public UpdateMode RotationUpdate = UpdateMode.LateUpdate;

        [Hide(nameof(rotation)), Min(0)] public float lerpRot = 0f;


        // Update is called once per frame
        void Update()
        {
            if (reference.Value == null) return;

            if (PositionUpdate == UpdateMode.Update) SetPositionReference(Time.deltaTime);
            if (RotationUpdate == UpdateMode.Update) SetRotationReference(Time.deltaTime);
        }

        void LateUpdate()
        {
            if (reference.Value == null) return;

            if (PositionUpdate == UpdateMode.LateUpdate) SetPositionReference(Time.deltaTime);
            if (RotationUpdate == UpdateMode.LateUpdate) SetRotationReference(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (reference.Value == null) return;

            if (PositionUpdate == UpdateMode.FixedUpdate) SetPositionReference(Time.fixedDeltaTime);
            if (RotationUpdate == UpdateMode.FixedUpdate) SetRotationReference(Time.fixedDeltaTime);
        }

        private void SetPositionReference(float delta)
        {
            if (position)
            {
                var newPos = transform.position;

                if ((posAxis & XYZEnum.X) == XYZEnum.X) newPos.x = reference.Value.position.x;
                if ((posAxis & XYZEnum.Y) == XYZEnum.Y) newPos.y = reference.Value.position.y;
                if ((posAxis & XYZEnum.Z) == XYZEnum.Z) newPos.z = reference.Value.position.z;


                transform.position = Vector3.Lerp(transform.position, newPos, lerpPos == 0 ? 1 : delta * lerpPos);
            }
        }

        private void SetRotationReference(float delta)
        {
            if (rotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, reference.Value.rotation, lerpRot == 0 ? 1 : delta * lerpRot);
        }

        private void OnValidate()
        {
            if (Reference != null) //Clear the old reference!!!! important
            {
                reference.Value = Reference;
                Reference = null;
                MTools.SetDirty(this);
            }
        }
    }
}