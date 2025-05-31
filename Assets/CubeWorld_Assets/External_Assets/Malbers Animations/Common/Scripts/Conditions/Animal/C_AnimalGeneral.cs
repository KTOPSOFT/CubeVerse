namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Animal/General")]
    public class C_AnimalGeneral : MAnimalCondition
    {
        public enum AnimalCondition
        { Grounded, RootMotion, FreeMovement, AlwaysForward, Sleep, AdditivePosition, AdditiveRotation, InZone, InGroundChanger }

        public AnimalCondition Condition;

        public override bool _Evaluate()
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
                }
            }
            return false;
        }


        private void Reset() => Name = "New Animal Condition";

    }
}
