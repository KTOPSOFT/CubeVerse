using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller.Reactions
{
    /// <summary> Reaction Script for Making the Animal do something </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Stat", fileName = "New Stat Modifier", order = -100)]
    public class ModifyStatSO : ScriptableObject
    {
        [HideInInspector]
        public StatModifier modifier;

        public List<StatModifier> modifiers = new();
        /// <summary>Instant Reaction ... without considering Active or Delay parameters</summary>
        public void Modify(Stats stats)
        {
            foreach (var item in modifiers)
            {
                item.ModifyStat(stats);
            }
        }

        public void Modify(Component stats)
        {
            Modify(stats.MFindComponentInRoot<Stats>());
        }

        public void Modify(GameObject stats)
        {
            Modify(stats.MFindComponentInRoot<Stats>());
        }

        [SerializeField, HideInInspector]

        private bool V2Updated;

        private void OnValidate()
        {
            if (!V2Updated)
            {
                if (modifiers == null || modifiers.Count == 0)
                {
                    modifiers = new() { modifier };
                    // Debug.Log($"Modify Stat SO [{name}] Updated to List of Modifiers", this);

                }
                V2Updated = true;
                MTools.SetDirty(this);
            }
        }
    }
}