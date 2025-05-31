using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    public class EventBehaviour : StateMachineBehaviour
    {
        [SerializeField] private MEvent _mEvent;

        [Range(0, 1)]
        [SerializeField] private float _time = 0;

        [SerializeField] private AnimalEvent _animalEvent = new AnimalEvent();

        public AnimalEvent AnimalEvent => _animalEvent;
        private bool MessageSent = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            MessageSent = false;

            if (_time == 0)
            {
                _animalEvent.Invoke(_mEvent);
                MessageSent = true;
            }
        }

        public override void OnStateUpdate(Animator anim, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;

            if (!MessageSent && time >= _time)
            {
                _animalEvent.Invoke(_mEvent);
                MessageSent = true;
            }
        }

    }

    [System.Serializable]
    public class AnimalEvent
    {
        ///// <summary>Animation State with the Tag  to apply the modifiers</summary>
        //public string AnimationTag;

        [Utilities.Flag]
        public EventBehaviorType eventBehaviorType;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.BoolEvent)]
        public bool boolEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.FloatEvent)]
        public float floatEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.IntEvent)]
        public int intEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.StringEvent)]
        public string stringEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.Vector3Event)]
        public Vector3 vector3Event;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.Vector2Event)]
        public Vector2 vector2Event;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.GameObjectEvent)]
        public GameObject gameObjectEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.TransformEvent)]
        public Transform transformEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.ComponentEvent)]
        public Component componentEvent;
        [Hide("eventBehaviorType", false, true, true, (int)EventBehaviorType.SpriteEvent)]
        public Sprite spriteEvent;

        public void Invoke(MEvent mEvent)
        {
            if ((int)eventBehaviorType == 0) return; //Means that the event have no modification

            if (CheckEventType(EventBehaviorType.VoidEvent)) { mEvent.Invoke(); }
            if (CheckEventType(EventBehaviorType.BoolEvent)) { mEvent.Invoke(boolEvent); }
            if (CheckEventType(EventBehaviorType.FloatEvent)) { mEvent.Invoke(floatEvent); }
            if (CheckEventType(EventBehaviorType.IntEvent)) { mEvent.Invoke(intEvent); }
            if (CheckEventType(EventBehaviorType.StringEvent)) { mEvent.Invoke(stringEvent); }
            if (CheckEventType(EventBehaviorType.Vector3Event)) { mEvent.Invoke(vector3Event); }
            if (CheckEventType(EventBehaviorType.Vector2Event)) { mEvent.Invoke(vector2Event); }
            if (CheckEventType(EventBehaviorType.GameObjectEvent)) { mEvent.Invoke(gameObjectEvent); }
            if (CheckEventType(EventBehaviorType.TransformEvent)) { mEvent.Invoke(transformEvent); }
            if (CheckEventType(EventBehaviorType.ComponentEvent)) { mEvent.Invoke(componentEvent); }
            if (CheckEventType(EventBehaviorType.SpriteEvent)) { mEvent.Invoke(spriteEvent); }
        }

        private bool CheckEventType(EventBehaviorType modifier) => (eventBehaviorType & modifier) == modifier;
    }

    public enum EventBehaviorType
    {
        VoidEvent = 1,
        BoolEvent = 2,
        FloatEvent = 4,
        IntEvent = 8,
        StringEvent = 16,
        Vector3Event = 32,
        Vector2Event = 64,
        GameObjectEvent = 128,
        TransformEvent = 256,
        ComponentEvent = 512,
        SpriteEvent = 1024,
    }
}