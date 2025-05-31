using MalbersAnimations.Controller;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Animal/States")]
    public class C_AnimalState : MAnimalCondition
    {
        //public override string DisplayName => "Animal/States";

        public enum StateCondition { ActiveState, Enabled, HasState, Pending, SleepFromMode, SleepFromState, SleepFromStance, LastState, }
        public StateCondition Condition = StateCondition.ActiveState;
        public StateID Value;

        public void SetValue(StateID v) => Value = v;

        private State st;

        public override bool _Evaluate()
        {
            if (Target)
            {
                if (st == null) st = Target.State_Get(Value);

                switch (Condition)
                {
                    case StateCondition.ActiveState: return Target.ActiveStateID == Value;     //Check if the Active state is the one with this ID
                    case StateCondition.HasState: return st != null;                            //Check if the State exist on the Current Animal
                    case StateCondition.Enabled: return st.Active;
                    case StateCondition.Pending: return st.IsPending;
                    case StateCondition.SleepFromMode: return st.IsSleepFromMode;
                    case StateCondition.SleepFromState: return st.IsSleepFromState;
                    case StateCondition.SleepFromStance: return st.IsSleepFromStance;
                    case StateCondition.LastState: return Target.LastState.ID == Value;       //Check if the LastState is this ID
                }
            }
            return false;
        }
    }
}
