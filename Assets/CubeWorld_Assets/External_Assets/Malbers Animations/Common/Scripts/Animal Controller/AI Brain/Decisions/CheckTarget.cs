using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Target",order = -100)]
    public class CheckTarget : MAIDecision
    {
        public override string DisplayName => "Movement/Check Target";

        public CompareTarget compare = CompareTarget.IsNull;

        [Hide("compare",2)]
        public RuntimeGameObjects set;
        [Hide("compare", 1)]
        public TransformVar transform;
        [Hide("compare", 3)]
        public string m_name;


        public override bool Decide(MAnimalBrain brain, int index)
        {
            switch (compare)
            {
                case CompareTarget.IsNull:
                    return brain.Target == null;
                case CompareTarget.isTransformVar:
                    return transform.Value != null && brain.Target == transform.Value;
                case CompareTarget.IsInRuntimeSet:
                    return set != null && set.Items.Contains(brain.Target.gameObject);
                case CompareTarget.HasName:
                    return string.IsNullOrEmpty(m_name) && brain.Target.name.Contains(m_name);
                case CompareTarget.IsActiveInHierarchy:
                    return brain.Target && brain.Target.gameObject.activeInHierarchy;
                default:
                    break;
            }
            return false;
        }

        public enum CompareTarget {IsNull, isTransformVar, IsInRuntimeSet, HasName,  IsActiveInHierarchy}
    }
}