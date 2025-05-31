using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    public enum CheckSetSize { Empty, Equal, Greater, Less }
    public class CheckRuntimeSet : MAIDecision
    {

        public override string DisplayName => "Runtime Set/Check Runtime Set";
        [RequiredField] public RuntimeGameObjects Set;

        [Tooltip("Check options of the Set")]
        public CheckSetSize CheckSize = CheckSetSize.Equal;

        [Tooltip("Size of the Current Set")]
        [Hide("CheckSize", true, 0)]
        public int Size = 0;

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            return (CheckSize) switch
            {
                CheckSetSize.Empty => Set.IsEmpty,
                CheckSetSize.Equal => Set.Count == Size,
                CheckSetSize.Greater => Set.Count > Size,
                CheckSetSize.Less => Set.Count < Size,
                _ => false
            };
        }

        void Reset() => Description = "Check if a Runtime Set is Empty or has a Size equal, greater or less than a given value";

    }
}