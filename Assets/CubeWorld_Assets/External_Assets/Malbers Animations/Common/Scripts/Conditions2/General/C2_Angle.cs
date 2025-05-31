using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("General/Check Angle")]
    [ConditionDescription("Check Angle Between GameObjects")]
    public class C2_Angle : ConditionCore
    {
        protected override void _SetTarget(Object target) => Target.Value = MTools.VerifyComponent(target, Target.Value);

        [Tooltip("Target to check for the condition")]
        [Hide(nameof(LocalTarget))]
        public GameObjectReference Target = new();

        [ContextMenuItem("Debug On", "ChangeDebugOn")]
        public AxisDirection Direction = AxisDirection.Forward;
        [Tooltip("Get the Local Axis from a GameObject. Leave empty World Axis")]
        public GameObjectReference Target2 = new();
        public AxisDirection Direction2 = AxisDirection.Forward;

        public ComparerInt Compare = ComparerInt.Equal;
        public FloatReference Angle = new(5);

        [Tooltip("Use Vector3.SignedAngle() instead of normal Vector3.Angle")]
        public bool useSignedAngle = false;

        [Hide(nameof(useSignedAngle))]
        public AxisDirection Axis = AxisDirection.Up;
        [Hide(nameof(useSignedAngle))]
        [Tooltip("Get the Local Axis from a GameObject. Leave empty World Axis")]
        public GameObjectReference AxisTarget = new();


        public Vector3 DirectionConverter(Transform target, AxisDirection direction)
        {
            if (target == null)
            {
                return direction switch
                {
                    AxisDirection.Right => Vector3.right,
                    AxisDirection.Left => -Vector3.right,
                    AxisDirection.Up => Vector3.up,
                    AxisDirection.Down => -Vector3.up,
                    AxisDirection.Forward => Vector3.forward,
                    AxisDirection.Backward => -Vector3.forward,
                    AxisDirection.None => Vector3.zero,
                    _ => Vector3.zero,
                };
            }
            else
            {
                return direction switch
                {
                    AxisDirection.Right => target.right,
                    AxisDirection.Left => -target.right,
                    AxisDirection.Up => target.up,
                    AxisDirection.Down => -target.up,
                    AxisDirection.Forward => target.forward,
                    AxisDirection.Backward => -target.forward,
                    AxisDirection.None => Vector3.zero,
                    _ => Vector3.zero,
                };
            }
        }

        protected override bool _Evaluate()
        {
            var from = DirectionConverter(Target.Value.transform, Direction);
            var to = DirectionConverter(Target2.Value.transform, Direction2);
            var axis = DirectionConverter(AxisTarget.Value.transform, Direction2);


            var angle = useSignedAngle ? Vector3.SignedAngle(from, to, axis) : Vector3.Angle(from, to);

            bool result = angle.CompareFloat(Angle, Compare);
            Debugging($"Angle: {angle:F2} is {Compare} than {Angle.Value} ? ", result, Target.Value);
            return result;
        }



        /// <summary>
        /// To make this work you need to call this method the ONDrawGizmos on any Monobehaviour that has conditions
        /// </summary>
        /// <param name="target"></param>

        public override void DrawGizmos(Component target)
        {
#if UNITY_EDITOR
            var angle = Angle.Value;
            if (angle != 360)
            {
                angle /= 2;

                var Direction = DirectionConverter(target.transform, this.Direction);

                Handles.color = new Color(0, 1, 0, 0.1f);
                Handles.DrawSolidArc(target.transform.position, target.transform.up, Quaternion.Euler(0, -angle, 0) * Direction, angle * 2, target.transform.localScale.y);
                Handles.color = Color.green;
                Handles.DrawWireArc(target.transform.position, target.transform.up, Quaternion.Euler(0, -angle, 0) * Direction, angle * 2, target.transform.localScale.y);
            }
#endif
        }
    }
}
