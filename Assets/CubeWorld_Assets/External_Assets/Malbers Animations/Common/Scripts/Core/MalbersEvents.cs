using System;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Events
{
    [Serializable] public class GameObjectEvent : UnityEvent<GameObject> { }
    [Serializable] public class TransformEvent : UnityEvent<Transform> { }
    [Serializable] public class SpriteEvent : UnityEvent<Sprite> { }
    [Serializable] public class RayCastHitEvent : UnityEvent<RaycastHit> { }
    [Serializable] public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable] public class Vector2Event : UnityEvent<Vector2> { }
    [Serializable] public class ColorEvent : UnityEvent<Color> { }
    [Serializable] public class IntEvent : UnityEvent<int> { }
    [Serializable] public class Int2Event : UnityEvent<int, int> { }
    [Serializable] public class FloatEvent : UnityEvent<float> { }
    [Serializable] public class IntFloatEvent : UnityEvent<int, float> { }
    [Serializable] public class BoolEvent : UnityEvent<bool> { }
    [Serializable] public class StringEvent : UnityEvent<string> { }
    [Serializable] public class String2Event : UnityEvent<string, string> { }
    [Serializable] public class ColliderEvent : UnityEvent<Collider> { }
    [Serializable] public class CollisionEvent : UnityEvent<Collision> { }
    [Serializable] public class ComponentEvent : UnityEvent<Component> { }
    [Serializable] public class AnimatorEvent : UnityEvent<Animator> { }
    [Serializable] public class AudioEvent : UnityEvent<AudioClip> { }
    [Serializable] public class RigidbodyEvent : UnityEvent<Rigidbody> { }
    [Serializable] public class IDsEvent : UnityEvent<IDs> { }
}