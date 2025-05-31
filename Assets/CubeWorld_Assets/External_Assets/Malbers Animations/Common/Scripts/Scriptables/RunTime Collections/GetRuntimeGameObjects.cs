using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Runtime Vars/Get Runtime GameObjects")]

    public class GetRuntimeGameObjects : MonoBehaviour
    {
        [RequiredField] public RuntimeGameObjects Collection;

        public FloatReference delay = new ();

        public RuntimeSetTypeGameObject type = RuntimeSetTypeGameObject.Random;
        [Hide("showIndex", false)]
        public int Index = 0;

        [Hide("showName", false)]
        public string m_name; 


        public GameObjectEvent Raise = new();
        public GameObjectEvent OnItemAdded = new();
        public GameObjectEvent OnItemRemoved = new();

        public void SetCollection(RuntimeGameObjects col) => Collection = col;

        private void OnEnable()
        {

            if (delay > 0)
                Invoke(nameof(GetSet), delay);
            else
                this.Delay_Action(() => GetSet());

            if (Collection != null)
            {
                Collection.OnItemAdded.AddListener(ItemAdded);
                Collection.OnItemRemoved.AddListener(ItemRemoved);
            }
        }

        private void OnDisable()
        {
            if (Collection != null)
            {
                Collection.OnItemAdded.RemoveListener(ItemAdded);
                Collection.OnItemRemoved.RemoveListener(ItemRemoved);
            }
        }

        void ItemAdded(GameObject item) => OnItemAdded.Invoke(item);
        void ItemRemoved(GameObject item) => OnItemRemoved.Invoke(item);

        private void GetSet()
        {
            if (Collection != null)
            {
                Raise.Invoke(Collection.GetItem(type, Index, name, gameObject));
            }
        }

        [HideInInspector] public bool showIndex;
        [HideInInspector] public bool showName;
        private void OnValidate()
        {
            showIndex = type == RuntimeSetTypeGameObject.Index;
            showName = type == RuntimeSetTypeGameObject.ByName;
        }
    }
}