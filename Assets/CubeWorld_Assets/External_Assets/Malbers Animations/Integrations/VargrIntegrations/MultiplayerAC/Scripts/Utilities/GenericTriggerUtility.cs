using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;

namespace MalbersAnimations.VargrMultiplayer
{
    public class GenericTriggerUtility : TriggerEventSyncUser
    {
        [SerializeField] protected BoolEvent boolEvent;
        [SerializeField] protected GameObjectEvent gameObjectEvent;
        [SerializeField] protected IntEvent intEvent;
        [SerializeField] protected FloatEvent floatEvent;
        [SerializeField] protected Vector3Event vector3Event;
        [SerializeField] protected Vector2Event vector2Event;
        [SerializeField] protected StringEvent stringEvent;
        
        protected override void Awake()
        {
            base.Awake();
        }

        public void BoolEventInvoke(bool data) => m_TriggerEventSync.EventTrigger(data, indexEncode(0), NetEventSource.GenericTriggerUtility);

        public void GameObjectEventInvoke(GameObject data) => m_TriggerEventSync.EventTrigger(data, indexEncode(1), NetEventSource.GenericTriggerUtility);

        public void IntEventInvoke(int data) => m_TriggerEventSync.EventTrigger(data, indexEncode(2), NetEventSource.GenericTriggerUtility);

        public void FloatEventInvoke(float data) => m_TriggerEventSync.EventTrigger(data, indexEncode(3), NetEventSource.GenericTriggerUtility);

        public void Vector3EventInvoke(Vector3 data) => m_TriggerEventSync.EventTrigger(data, indexEncode(4), NetEventSource.GenericTriggerUtility);

        public void Vector2EventInvoke(Vector2 data) => m_TriggerEventSync.EventTrigger(data, indexEncode(5), NetEventSource.GenericTriggerUtility);

        public void StringEventInvoke(string data) => m_TriggerEventSync.EventTrigger(data, indexEncode(6), NetEventSource.GenericTriggerUtility);

        protected override void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.GenericTriggerUtility && index >= indexEncode(0) && index < indexEncode(10) ) return;

            switch(indexEncode(index, true))
            {
                case 0: boolEvent.Invoke((bool)data); break;
                case 1: gameObjectEvent.Invoke((GameObject)data); break;
                case 2: intEvent.Invoke((int)data); break;
                case 3: floatEvent.Invoke((float)data); break;
                case 4: vector3Event.Invoke((Vector3)data); break;
                case 5: vector2Event.Invoke((Vector2)data); break;
                case 6: stringEvent.Invoke((string)data); break;
            }
        }
    }
}