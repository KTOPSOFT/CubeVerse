using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [CreateAssetMenu(menuName = "Malbers Animations/Reaction Var", order = 100)]
    public class MReactionsVar : ScriptableObject
    {
        [SerializeReference,SubclassSelector]
        public Reaction reaction;

        public void React(Component component)
        {
            if (component == null)
            {
                Debug.LogWarning("There's no component set to apply the reactions");
                return;
            }
            reaction.React(component);
        }

        public void React(GameObject go)
        {
            if (go == null)
            {
                Debug.LogWarning("There's no gameobject set to apply the reactions");
                return;
            }
            reaction.React(go);
        }
    }
}

 