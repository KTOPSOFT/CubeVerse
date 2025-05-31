using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{

    /// <summary> Struct to Apply a Multiplier to anything whenever the Animal is on a State  </summary>
    [System.Serializable]
    public class StateMultiplier
    {
        public StateID ID;
        public float Multiplier;
    }

    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Force")]
    public class ModifierForce : ModeModifier
    {
        [HelpBox]
        public string Desc = "Applies a Force to the Animal when the Mode starts. Remove the force when the mode ends";

        [Tooltip("Direction of the Force")]
        public Vector3Reference Direction = new(Vector3.forward);

        [Tooltip("Use the Raw Input Axis Instead of the Direction Value")]
        public BoolReference UseInputAxis = new();

        [Tooltip("Amount of force to apply to the Animal")]
        public FloatReference Force = new(2);
        [Tooltip("Time the Force will be applied to the Animal. if is set to Zero then it will be applied during the whole Animation")]
        public FloatReference m_Time = new(0);
        [Tooltip("Start Acceleration of the force")]
        public FloatReference EnterAceleration = new(5);
        [Tooltip("Exit Acceleration of the force")]
        public FloatReference ExitAceleration = new(5);
        [Tooltip("Gravity will be Reseted at start")]
        public BoolReference ResetGravity = new(true);

        [Tooltip("When the Force is applied the Gravity will be Reseted")]
        public BoolReference ResetGravityDuring = new(true);

        [Tooltip("Interrupt the Jump Height when the Force is applied")]
        public BoolReference InterruptJumpHeight = new(true);


        [Tooltip("Remove Y value from the Additive position")]
        public BoolReference NoY = new();
        [Header("Check States")]

        [Tooltip("Increase the Force applied depending which state the Animal is playing")]
        public List<StateMultiplier> stateMultipliers;

        public override void OnModeEnter(Mode mode)
        {
            var multiplier = 1f;

            if (stateMultipliers != null && stateMultipliers.Count > 0)
            {
                var ActiveState = mode.Animal.ActiveStateID;

                var stateM = stateMultipliers.Find(x => x.ID == ActiveState);

                if (stateM != null) multiplier = stateM.Multiplier;

            }

            var Direction = UseInputAxis.Value ? mode.Animal.RawInputAxis : this.Direction;

            mode.Animal.Force_Add(mode.Animal.transform.TransformDirection(Direction), Force * multiplier, EnterAceleration, ResetGravity);



            mode.Animal.UpInertia_Clear();

            if (InterruptJumpHeight.Value)
            {
                var Jump = mode.Animal.State_Get<JumpBasic>();
                if (Jump != null) Jump.NoJumpHeight();
            }
        }

        public override void OnModeMove(Mode mode)
        {
            if (m_Time > 0 && MTools.ElapsedTime(mode.ActivationTime, m_Time))
            {
                if (mode.Animal.ExternalForce != Vector3.zero)
                    mode.Animal.Force_Remove(ExitAceleration);
            }
            else
            {
                if (ResetGravityDuring)
                {
                    mode.Animal.GravityOffset = Vector3.zero;
                    mode.Animal.GravityTime--;
                }

                if (NoY) mode.Animal.additivePosition.y = 0;
            }


            //if (ResetGravity)
            //{
            //    // mode.Animal.GravityTime = mode.Animal.StartGravityTime;

            //    mode.Animal.GravityOffset = Vector3.zero;
            //    mode.Animal.GravityTime--;
            //}

        }
        public override void OnModeExit(Mode mode) => mode.Animal.Force_Remove(ExitAceleration);

    }
}