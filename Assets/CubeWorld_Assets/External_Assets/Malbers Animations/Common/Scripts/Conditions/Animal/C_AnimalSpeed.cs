namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Animal/Speeds")]
    public class C_AnimalSpeed : MAnimalCondition
    {
        public enum SpeedCondition { VerticalSpeed, CurrentSpeedSet, CurrentSpeedModifier, ActiveIndex, Sprinting, CanSprint }

        public SpeedCondition Condition;

        [Hide(nameof(Condition), (int)SpeedCondition.CurrentSpeedSet, (int)SpeedCondition.CurrentSpeedModifier)]
        public ComparerInt compare = ComparerInt.Equal;

        [Hide(nameof(Condition), (int)SpeedCondition.CurrentSpeedSet, (int)SpeedCondition.CurrentSpeedModifier)]
        public float Value = 0;

        [Hide(nameof(Condition), (int)SpeedCondition.VerticalSpeed)]
        public string SpeedName;

        //public override string DisplayName => "Animal/Speeds";

        public override bool _Evaluate()
        {
            if (Target)
            {
                switch (Condition)
                {
                    case SpeedCondition.VerticalSpeed:
                        return Target.VerticalSmooth.CompareFloat(Value, compare);
                    case SpeedCondition.CurrentSpeedSet:
                        return Target.CurrentSpeedSet.name == SpeedName;
                    case SpeedCondition.CurrentSpeedModifier:
                        return Target.CurrentSpeedModifier.name == SpeedName;
                    case SpeedCondition.ActiveIndex:
                        return Target.CurrentSpeedIndex == Value;
                    case SpeedCondition.Sprinting:
                        return Target.Sprint;
                    case SpeedCondition.CanSprint:
                        return Target.CanSprint;
                }

            }
            return false;
        }
    }
}
