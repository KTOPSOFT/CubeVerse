using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Malbers Tag", order = 5)]
    public class CheckMalbersTag : MAIDecision
    {
        public override string DisplayName => "General/Check Malbers Tag";
        public Affected CheckOn = Affected.Self;

        public bool CheckInParent = true;
        public Tag[] tags;

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            if (CheckOn == Affected.Self)
            {
                if (CheckInParent)
                    return brain.gameObject.HasMalbersTagInParent(tags);
                else
                    return brain.gameObject.HasMalbersTag(tags);
            }
            else
            {
                if (brain.Target)
                {
                    if (CheckInParent)
                        return brain.Target.HasMalbersTagInParent(tags);
                    else
                        return brain.Target.HasMalbersTag(tags);
                }
            }

            return false;
        }
    }
}