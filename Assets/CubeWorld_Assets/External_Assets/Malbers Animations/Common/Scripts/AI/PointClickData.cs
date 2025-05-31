using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MalbersAnimations
{
    public class PointClickData : ScriptableObject
    {
        public Action<BaseEventData> baseDataPointerClick;

        public Action<BaseEventData> pointerDown;

        public Action<BaseEventData> pointerUp;

        public void Invoke(BaseEventData data) => baseDataPointerClick?.Invoke(data);
        public void PointerDown(BaseEventData data) => pointerDown?.Invoke(data);
        public void PointerUp(BaseEventData data) => pointerUp?.Invoke(data);
    }
}