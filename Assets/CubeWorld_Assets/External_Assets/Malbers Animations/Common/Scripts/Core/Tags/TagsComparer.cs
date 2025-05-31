using MalbersAnimations.Events;
using UnityEngine;
namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/scriptables/tags")]
    [AddComponentMenu("Malbers/Utilities/Tools/Tags Comparer")]
    public class TagsComparer : MonoBehaviour 
    {
        [System.Serializable]
        public class TagComparerResponse
        {
            public Tag tag;
            public GameObjectEvent HasTag = new();
        }

        public bool CheckInParent = true;
        public TagComparerResponse[] tags;

        public void Evaluate(GameObject gameObject)
        {
            foreach (var tag in tags)
            {
                if (CheckInParent && gameObject.HasMalbersTagInParent(tag.tag) || gameObject.HasMalbersTag(tag.tag))
                {
                    tag.HasTag.Invoke(gameObject);
                }
            }
        }
         
        public void Evaluate(Component co) => Evaluate(co.gameObject);
       
    }
}