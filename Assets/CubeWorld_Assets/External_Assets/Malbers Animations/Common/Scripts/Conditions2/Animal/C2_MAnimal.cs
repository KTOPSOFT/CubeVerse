using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    public abstract class C2_MAnimal : ConditionCore//<MAnimal>
    {
        [Hide(nameof(LocalTarget))] public MAnimal Target;
        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);
    }

    #region Animal General Values 
    [System.Serializable, AddTypeMenu("Animal/General")]
    public class C2_AnimalGeneral : C2_MAnimal
    {
        public enum AnimalCondition
        {
            Grounded, RootMotion, FreeMovement, AlwaysForward, Sleep, AdditivePosition,
            AdditiveRotation, InZone, InGroundChanger, Strafing, CanStrafe, MovementDetected
        }

        public AnimalCondition Condition;

        protected override bool _Evaluate()
        {
            if (Target)
            {
                switch (Condition)
                {
                    case AnimalCondition.Grounded: return Target.Grounded;
                    case AnimalCondition.RootMotion: return Target.RootMotion;
                    case AnimalCondition.FreeMovement: return Target.FreeMovement;
                    case AnimalCondition.AlwaysForward: return Target.AlwaysForward;
                    case AnimalCondition.Sleep: return Target.Sleep;
                    case AnimalCondition.AdditivePosition: return Target.UseAdditivePos;
                    case AnimalCondition.AdditiveRotation: return Target.UseAdditiveRot;
                    case AnimalCondition.InZone: return Target.InZone;
                    case AnimalCondition.InGroundChanger: return Target.GroundChanger != null && Target.GroundChanger.Lerp > 0;
                    case AnimalCondition.Strafing: return Target.Strafe;
                    case AnimalCondition.CanStrafe: return Target.CanStrafe && Target.ActiveStance.CanStrafe && Target.ActiveState.CanStrafe;
                    case AnimalCondition.MovementDetected: return Target.MovementDetected;
                }
            }
            return false;
        }
    }
    #endregion

    #region Animal Modes
    [System.Serializable, AddTypeMenu("Animal/Modes")]
    public class C2_AnimalMode : C2_MAnimal
    {
        public enum ModeCondition { PlayingAnyMode, PlayingMode, PlayingAbility, HasMode, HasAbility, Enabled }
        public ModeCondition Condition;
        [Hide(nameof(Condition), true, 0)]
        public ModeID Value;
        [Hide(nameof(Condition), 2, 4)]
        public StringReference AbilityName;
        private Mode mode;
        public void SetValue(ModeID v) => Value = v;

        protected override bool _Evaluate()
        {
            if (Target == null) return false;

            mode ??= Target.Mode_Get(Value);        //cache the mode
            if (mode == null) return false;

            return Condition switch
            {
                ModeCondition.PlayingMode => Target.IsPlayingMode && (Value == null || Target.ActiveMode.ID == Value),
                ModeCondition.PlayingAbility =>
                Target.IsPlayingMode && (string.IsNullOrEmpty(AbilityName.Value) || Target.ActiveMode.ActiveAbility.Name == AbilityName),
                ModeCondition.HasMode => mode != null,
                ModeCondition.HasAbility => mode != null && mode.Abilities.Exists(x => x.Name == AbilityName),
                ModeCondition.Enabled => mode != null && mode.Active,
                ModeCondition.PlayingAnyMode => Target.IsPlayingMode,
                _ => false,
            };
        }
    }
    #endregion

    #region Animal States
    [System.Serializable, AddTypeMenu("Animal/States")]
    public class C2_AnimalState : C2_MAnimal
    {
        public enum StateCondition { ActiveState, Enabled, HasState, LastState, SleepFromMode, SleepFromState, SleepFromStance, Pending, IsPersistent }
        public StateCondition Condition = StateCondition.ActiveState;
        public StateID Value;
        private State state;

        public void SetValue(StateID v) => Value = v;

        protected override bool _Evaluate()
        {
            if (!Target) return false;

            if (state == null && Target) state = Target.State_Get(Value); //cache the state

            return Condition switch
            {
                StateCondition.ActiveState => Target.ActiveStateID == Value,    //Check if the Active state is the one with this ID
                StateCondition.HasState => state != null,                       //Check if the State exist on the Current Animal
                StateCondition.Enabled => state.Active,
                StateCondition.SleepFromMode => state.IsSleepFromMode,
                StateCondition.SleepFromState => state.IsSleepFromState,
                StateCondition.SleepFromStance => state.IsSleepFromStance,
                StateCondition.LastState => Target.LastState.ID == Value,       //Check if the LastState is this ID
                StateCondition.Pending => state.IsPending,
                StateCondition.IsPersistent => state.IsPersistent,
                _ => false,
            };
        }
    }
    #endregion

    #region Animal Stances
    [System.Serializable, AddTypeMenu("Animal/Stances")]
    public class C2_AnimalStance : C2_MAnimal
    {
        public enum StanceCondition { CurrentStance, DefaultStance, LastStance, HasStance }
        public StanceCondition Condition;
        public StanceID Value;
        private Stance stance;

        public void SetValue(StanceID v) => Value = v;

        protected override bool _Evaluate()
        {
            if (stance == null && Target != null) stance = Target.Stance_Get(Value); //cache the stance

            if (Target != null && stance != null)
            {
                return Condition switch
                {
                    StanceCondition.CurrentStance => Target.Stance == Value,
                    StanceCondition.DefaultStance => Target.DefaultStanceID == Value,
                    StanceCondition.LastStance => Target.LastStanceID == Value,
                    StanceCondition.HasStance => stance != null,
                    _ => false,
                };
            }
            return false;
        }
    }
    #endregion

    #region Animal Speeds
    [System.Serializable, AddTypeMenu("Animal/Speeds")]
    public class C2_AnimalSpeed : C2_MAnimal
    {
        public enum SpeedCondition { VerticalSpeed, CurrentSpeedSet, CurrentSpeedModifier, ActiveIndex, IsSprinting, CanSprint }

        public SpeedCondition Condition;

        [Hide(nameof(Condition), (int)SpeedCondition.VerticalSpeed, (int)SpeedCondition.ActiveIndex)]
        public ComparerInt compare = ComparerInt.Equal;

        [Hide(nameof(Condition), (int)SpeedCondition.VerticalSpeed, (int)SpeedCondition.ActiveIndex)]
        public FloatReference Value = new();

        [Hide(nameof(Condition), (int)SpeedCondition.CurrentSpeedSet, (int)SpeedCondition.CurrentSpeedModifier)]
        public StringReference SpeedName = new();

        protected override bool _Evaluate()
        {
            if (!Target) return false;

            return Condition switch
            {
                SpeedCondition.VerticalSpeed => Target.VerticalSmooth.CompareFloat(Value, compare),
                SpeedCondition.CurrentSpeedSet => Target.CurrentSpeedSet.name == SpeedName,
                SpeedCondition.CurrentSpeedModifier => Target.CurrentSpeedModifier.name == SpeedName,
                SpeedCondition.ActiveIndex => Target.CurrentSpeedIndex == (int)Value,
                SpeedCondition.IsSprinting => Target.Sprint,
                SpeedCondition.CanSprint => Target.CanSprint,
                _ => false,
            };
        }
    }
    #endregion
}
