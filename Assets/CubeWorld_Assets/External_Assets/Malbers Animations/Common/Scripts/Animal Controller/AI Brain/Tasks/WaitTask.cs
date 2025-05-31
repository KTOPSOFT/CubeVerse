using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Wait")]
    public class WaitTask : MTask
    {
        public override string DisplayName => "General/Wait";

        [Space] 
        public FloatReference WaitMinTime = new(5);
        public FloatReference WaitMaxTime = new(5);

        [Tooltip("After the wait is over if this MAI State is not null. Execute it!")]
        public MAIState NextState;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            brain.TasksVars[index].floatValue = UnityEngine.Random.Range(WaitMinTime, WaitMaxTime);
        }

        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            if (MTools.ElapsedTime(brain.TasksStartTime[index], brain.TasksVars[index].floatValue))
            {
                brain.TaskDone(index);
                if (NextState) brain.StartNewState(NextState); //Go to the next state if it has any
            }
        }
    }
}
