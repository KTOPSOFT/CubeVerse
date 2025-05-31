using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Is Target in RuntimeSet", order = 4)]
    public class IsTargetInSetDecision : MAIDecision
    {
        public override string DisplayName => "Runtime Set/Is Target in RuntimeSet";
        public RuntimeGameObjects Set;

        [Tooltip("Check if the Current Target is the closest ")]
        public bool IsClosest = false;

        [Tooltip("If the closest Object is not the Current Target. Assign it as a new target")]
        [Hide("IsClosest")]
        public bool ClosestIsNewTarget = false;

        [Tooltip("When the new target is assigned, set it to move to the target")]
        [Hide("ClosestIsNewTarget")]
        public bool Move = true;

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            if (brain.AIControl.Target != null)
            {
                var IsInSet = Set.Items.Contains(brain.Target.gameObject);

                if (IsInSet)
                {
                    if (IsClosest)
                    {
                        var ClosestObject = Set.Item_GetClosest(brain.gameObject);

                        if (ClosestObject != brain.Target.gameObject)
                        {
                            if (ClosestIsNewTarget)
                                brain.AIControl.SetTarget(ClosestObject.transform, Move);

                            return false; //Return false if the current target IS not the closest one
                        }
                        return true; //Return true if the current target IS the closest one
                    }
                    return IsInSet;
                }
            }
            return false;
        }

        void Reset() => Description = "Returns true if the Current AI Target is on a Runtime Set.\n " +
            "If [IsClosest] is enabled. It will check also if the current target is the closest one. If is Not, then it will return false";
         
    }
}