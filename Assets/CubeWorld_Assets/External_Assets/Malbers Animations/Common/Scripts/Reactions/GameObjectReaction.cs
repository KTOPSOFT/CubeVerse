using MalbersAnimations.Scriptables;
using System;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Unity/GameObject")]
    public class GameObjectReaction : Reaction
    {
        public enum GameObject_Reaction { SetActive, Instantiate }

        public override Type ReactionType => typeof(GameObject);

        public GameObject_Reaction action = GameObject_Reaction.Instantiate;

        public GameObjectReference Value = new();

        public float time = 0;

        protected override bool _TryReact(Component component)
        {
            // var go = Value.Value.gameObject;

            if (Value.Value = null)
            {
                switch (action)
                {
                    case GameObject_Reaction.SetActive:
                        Value.Value.SetActive(false);
                        break;
                    case GameObject_Reaction.Instantiate:
                        Value.Value = GameObject.Instantiate(Value.Value);
                        break;
                }
            }


            return false;
        }
    }
}
