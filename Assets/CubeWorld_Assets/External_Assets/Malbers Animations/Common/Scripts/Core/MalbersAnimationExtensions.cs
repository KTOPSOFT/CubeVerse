using MalbersAnimations.Scriptables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations
{
    public static class MalbersAnimationsExtensions
    {
        #region Dictionary Extensions
        public static T Get<T>(this Dictionary<string, object> instance, string key)
        {
            return (T)instance[key];
        }

        //public static void Add<T>(this Dictionary<string, object> instance, string key, object newValue)
        //{
        //    instance.Add(key, (T)newValue);
        //}
        #endregion

        #region Types
        public static bool IsSubclassDeep(this Type type, Type parenType)
        {
            while (type != null)
            {
                if (type.IsSubclassOf(parenType))
                    return true;
                type = type.BaseType;
            }

            return false;
        }

        public static bool TryGetGenericTypeOfDefinition(this Type type, Type genericTypeDefinition,
            out Type generictype)
        {
            generictype = null;
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    generictype = type;
                    return true;
                }
                type = type.BaseType;
            }

            return false;
        }

        public static bool IsSubclassOfGenericTypeDefinition(this Type t, Type genericTypeDefinition)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new Exception("genericTypeDefinition parameter isn't generic type definition");
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return true;
            }
            else
            {
                t = t.BaseType;
                while (t != null)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
                        return true;

                    t = t.BaseType;
                }
            }

            return false;
        }
        #endregion

        #region NullCheck
        /// <summary> Use this to check if a Unity Object is null or not. Usage:  [ a=b.Ref() ?? c; ] </summary>
        public static T Ref<T>(this T o) where T : UnityEngine.Object
        {
            return o == null ? null : o;
        }
        #endregion

        #region Float Int
        public static bool CompareFloat(this float current, float newValue, ComparerInt comparer)
        {
            return comparer switch
            {
                ComparerInt.Equal => (current == newValue),
                ComparerInt.Greater => (current > newValue),
                ComparerInt.Less => (current < newValue),
                ComparerInt.NotEqual => (current != newValue),
                _ => false,
            };
        }
        public static bool CompareInt(this int current, int newValue, ComparerInt comparer)
        {
            return comparer switch
            {
                ComparerInt.Equal => (current == newValue),
                ComparerInt.Greater => (current > newValue),
                ComparerInt.Less => (current < newValue),
                ComparerInt.NotEqual => (current != newValue),
                _ => false,
            };
        }
        public static bool InRange(this float current, float min, float max) => current >= min && current <= max;
        public static bool InRange(this int current, float min, float max) => current >= min && current <= max;

        #endregion

        /// <summary> Same as StartCoroutine but it also stores the coroytine in an IEnumerator </summary>
        public static void StartCoroutine(this MonoBehaviour Mono, out IEnumerator Cor, IEnumerator newCoro)
        {
            Cor = null;
            if (Mono.gameObject.activeInHierarchy)
            {
                Cor = newCoro;
                Mono.StartCoroutine(Cor);
            }
        }

        #region Vector3
        /// <summary>A useful Epsilon</summary>
        public const float Epsilon = 0.0001f;

        /// <summary>Round Decimal Places on a Vector</summary>
        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }


        public static Vector3 FlattenY(this Vector3 origin) => new(origin.x, 0, origin.z);
        public static Vector3 SetY(this Vector3 origin, float value) => new(origin.x, value, origin.z);
        public static Vector3 SetX(this Vector3 origin, float value) => new(value, origin.y, origin.z);
        public static Vector3 SetZ(this Vector3 origin, float value) => new(origin.x, origin.y, value);

        /// <summary>Checks if a vector is close to Vector3.zero</summary>
        public static bool CloseToZero(this Vector3 v, float threshold = 0.0001f) => v.sqrMagnitude < threshold * threshold;

        /// <summary> Get the closest point on a line segment. </summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static Vector3 ClosestPointOnLine(this Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = point - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB < Epsilon) return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
            return a + (aB * t);
        }


        public static Vector3 ProjectPointOnPlane(this Vector3 point, Vector3 planeNormal, Vector3 planePoint)
        {
            float distance;
            Vector3 translationVector;

            //First calculate the distance from the point to the plane:
            distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

            //Reverse the sign of the distance
            distance *= -1;

            //Get a translation vector
            translationVector = SetVectorLength(planeNormal, distance);

            //Translate the point to form a projection
            return point + translationVector;
        }

        public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
        {
            return Vector3.Dot(planeNormal, (point - planePoint));
        }

        //create a vector of direction "vector" with length "size"
        public static Vector3 SetVectorLength(Vector3 vector, float size)
        {

            //normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize(vector);

            //scale the vector
            return vectorNormalized *= size;
        }

        /// <summary>Get the closest point (0-1) on a line segment</summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static float ClosestTimeOnSegment(this Vector3 p, Vector3 s0, Vector3 s1)
        {
            Vector3 s = s1 - s0;
            float len2 = Vector3.SqrMagnitude(s);
            if (len2 < Epsilon)
                return 0; // degenrate segment
            return Mathf.Clamp01(Vector3.Dot(p - s0, s) / len2);
        }


        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Vector3 origin, Vector3 destination) => Vector3.Normalize(destination - origin);

        /// <summary>returns the delta position from a rotation.</summary>
        public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 point, Vector3 axis, float deltaAngle)
        {
            var pos = transform.position;
            var direction = pos - point;
            var rotation = Quaternion.AngleAxis(deltaAngle, axis);
            direction = rotation * direction;

            pos = point + direction - pos;
            pos.y = 0;                                                      //the Y is handled by the Fix Position method

            return pos;
        }

        /// <summary>returns the delta position from a rotation.</summary>
        public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 platform, Quaternion deltaRotation)
        {
            var pos = transform.position;

            var direction = pos - platform;
            var directionAfterRotation = deltaRotation * direction;

            var NewPoint = platform + directionAfterRotation;


            pos = NewPoint - transform.position;

            return pos;
        }

        /// <summary>  Returns if a point is inside a Sphere Radius </summary>
        /// <param name="point">Point you want to find inside a sphere</param>
        public static bool PointInsideSphere(this Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 direction = point - sphereCenter;
            float distanceSquared = direction.sqrMagnitude;
            return (distanceSquared <= sphereRadius * sphereRadius);
        }


        #endregion

        #region Transforms
        /// <summary> Find the first transform grandchild with this name inside this transform</summary>
        public static Transform FindGrandChild(this Transform aParent, string aName)
        {
            if (string.IsNullOrEmpty(aName)) return null; //Do nothing if the name is empty

            var result = aParent.ChildContainsName(aName);

            if (result != null) return result;

            foreach (Transform child in aParent)
            {
                result = child.FindGrandChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>Returns the Real Transform Core</summary> 
        public static Transform FindObjectCore(this Transform transf)
        {
            var core = transf;
            var IsObjectCore = core.FindInterface<IObjectCore>();
            if (IsObjectCore != null) return IsObjectCore.transform;

            return core;
        }


        /// <summary> Find the if a Transform is in the same hierachy(grandchild) of a parent. Returns true also if the Child = Parent</summary>
        public static bool SameHierarchy(this Transform child, Transform parent)
        {
            if (child == parent) return true; //Include yourself!! IMPORTANT
            if (child.parent == null) return false;
            if (child.parent == parent) return true;

            return SameHierarchy(child.parent, parent);
        }

        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Transform origin, Transform destination) => DirectionTo(origin.position, destination.position);
        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Transform origin, Vector3 destination) => DirectionTo(origin.position, destination);

        /// <summary> Find the closest transform from the origin </summary>
        public static Transform NearestTransform(this Transform origin, params Transform[] transforms)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = origin.position;
            foreach (Transform potentialTarget in transforms)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }

        /// <summary> Find the closest TransformReference from the origin </summary>
        public static Transform NearestTransform(this Transform origin, params TransformReference[] transforms)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = origin.position;
            foreach (Transform potentialTarget in transforms)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }

        /// <summary> Find the closest point from a transform </summary>
        public static Vector3 NearestPoint(this Transform origin, params Vector3[] allPoints)
        {
            Vector3 nearest = Vector3.zero;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = origin.position;
            foreach (var point in allPoints)
            {
                float dSqrToTarget = (point - currentPosition).sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    nearest = point;
                }
            }
            return nearest;
        }

        /// <summary> Find the farest transform from the origin </summary>
        public static Transform FarestTransform(this Transform t, params Transform[] transforms)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = t.position;
            foreach (Transform potentialTarget in transforms)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget > closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }

        public static Transform ChildContainsName(this Transform aParent, string aName)
        {
            foreach (Transform child in aParent)
            {
                if (child.name.Contains(aName))
                    return child;
            }
            return null;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void SetLocalTransform(this Transform transform, Vector3 LocalPos, Vector3 LocalRot, Vector3 localScale)
        {
            transform.localPosition = LocalPos;
            transform.localEulerAngles = LocalRot;
            transform.localScale = localScale;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void SetLocalTransform(this Transform transform, TransformOffset offset) => offset.RestoreTransform(transform);


        /// <summary>Parent a transform to another Transform, and Solves the Scale problem in case the Parent has a deformed scale  </summary>
        /// <param name="parent">Transform to be the parent</param>
        /// <param name="Position">Relative position to the Parent (World Position)</param>
        public static Transform SetParentScaleFixer(this Transform transform, Transform parent, Vector3 Position, GameObject Link = null)
        {
            Vector3 NewScale = parent.transform.lossyScale;
            NewScale.x = 1f / Mathf.Max(NewScale.x, Epsilon);
            NewScale.y = 1f / Mathf.Max(NewScale.y, Epsilon);
            NewScale.z = 1f / Mathf.Max(NewScale.z, Epsilon);

            //Create a new Link if is not created already.
            if (Link == null) Link = new() { name = transform.name + "Link" };

            //  Debug.Log("Hlper = " + Hlper);

            Link.transform.SetParent(parent);
            Link.transform.localScale = NewScale;
            Link.transform.position = Position;
            Link.transform.localRotation = Quaternion.identity;

            transform.SetParent(Link.transform);
            transform.localPosition = Vector3.zero;
            return Link.transform;
        }

        #endregion


        #region Animator


        /// <summary>  Return the Hash value of a parameter if it exists on the Animator. If it not exists it returns 0 </summary>
        public static int TryOptionalParameter(this Animator m_Animator, string param)
        {
            var AnimHash = Animator.StringToHash(param);

            foreach (var p in m_Animator.parameters)
            {
                if (p.nameHash == AnimHash) return AnimHash;
            }

            return 0;
        }


        #endregion

        #region String
        public static string RemoveSpecialCharacters(this string str)
        {
            System.Text.StringBuilder sb = new();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>  Resize a List </summary>
        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        #region GameObjects
        /// <summary>The GameObject is a prefab, Meaning in not in any scene</summary>
        public static bool IsPrefab(this GameObject go) => !go.scene.IsValid();
        #endregion


        #region Delay Action

        /// <summary>Do an action the next frame</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, Action action) => Delay_Action(mono, (int)1, action);

        /// <summary>Do an action the next given frames</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, int frames, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(frames, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        /// <summary>If the action is active stop it!</summary>
        public static void Stop_Action(this MonoBehaviour mono, IEnumerator action)
        {
            if (action != null) mono.StopCoroutine(action);
        }



        /// <summary>Do an action after certain time</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, float time, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(time, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        /// <summary>Do an action after certain time and stop an oldone</summary>
        public static void Delay_Action(this MonoBehaviour mono, ref IEnumerator oldAction, float time, Action action)
        {
            if (oldAction != null) mono.StopCoroutine(oldAction);
            oldAction = Delay_Action(mono, time, action);
        }

        public static IEnumerator Delay_Action(this MonoBehaviour mono, Func<bool> Condition, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(Condition, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        public static IEnumerator Delay_Action(this MonoBehaviour mono, WaitForSeconds time, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(time, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        private static IEnumerator DelayedAction(int frame, Action action)
        {
            for (int i = 0; i < frame; i++)
                yield return null;

            action.Invoke();
        }


        private static IEnumerator DelayedAction(Func<bool> Condition, Action action)
        {
            yield return new WaitWhile(Condition);
            action.Invoke();
        }


        /// <summary>  Use on custom C# types that are Unity objects to double check if the underlying Unity object is actually null or not.
        /// </summary>
        public static bool IsUnityRefNull<T>(this T o) where T : class
            => o == null || (o is UnityEngine.Object unityObj) && unityObj == null;

        private static IEnumerator DelayedAction(float time, Action action)
        {
            //Debug.Log("DelayStart");

            yield return new WaitForSeconds(time);
            action.Invoke();

            // Debug.Log("DelayEnd");
        }

        private static IEnumerator DelayedAction(WaitForSeconds time, Action action)
        {
            yield return time;
            action.Invoke();
        }

        #endregion

        #region Find Components/Interfaces
        public static T CopyComponent<T>(this T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();

            Component copy = destination.AddComponent(type);

            var fields = type.GetFields();

            foreach (System.Reflection.FieldInfo field in fields)
                field.SetValue(copy, field.GetValue(original));

            return copy as T;
        }

        public static T FindComponent<T>(this GameObject c) where T : Component
        {
            if (c.TryGetComponent<T>(out var Ttt)) return Ttt;

            Ttt = c.GetComponentInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }



        public static Component FindComponent(this GameObject c, Type t)
        {
            if (c.TryGetComponent(t, out var Ttt)) return Ttt;

            Ttt = c.GetComponentInParent(t);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren(t, true);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T[] FindComponents<T>(this GameObject c) where T : Component
        {
            T[] Ttt = c.GetComponents<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentsInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentsInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }

        /// <summary>Search for the Component in the root of the Object </summary>
        public static T MFindComponentInRoot<T>(this GameObject c) where T : Component
        {
            var root = c.transform.root;

            if (root.TryGetComponent<T>(out var Ttt)) return Ttt;

            Ttt = c.GetComponentInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = root.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }


        public static T FindInterface<T>(this GameObject c)
        {
            if (c.TryGetComponent<T>(out var Ttt)) return Ttt;

            Ttt = c.GetComponentInParent<T>(true);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T FindInterface<T>(this GameObject c, bool includeInactive)
        {
            T Ttt = c.GetComponent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInParent<T>(includeInactive);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(includeInactive);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T[] FindInterfaces<T>(this GameObject c)
        {
            T[] Ttt = c.GetComponents<T>();
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            Ttt = c.GetComponentsInParent<T>();
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            Ttt = c.GetComponentsInChildren<T>(true);
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            return default;
        }

        /// <summary>Search for the Component in the hierarchy Up or Down</summary>
        public static T FindComponent<T>(this Component c) where T : Component => c.gameObject.FindComponent<T>();

        public static T FindInterface<T>(this Component c) => c.gameObject.FindInterface<T>();
        public static T FindInterface<T>(this Component c, bool includeInactive) => c.gameObject.FindInterface<T>(includeInactive);
        public static T[] FindInterfaces<T>(this Component c) => c.gameObject.FindInterfaces<T>();

        /// <summary>Search for the Component in the root of the Object </summary>
        public static T MFindComponentInRoot<T>(this Component c) where T : Component => c.gameObject.MFindComponentInRoot<T>();

        /// <summary>  Reset the delta RootMotion of the Animator  </summary>
        public static IDeltaRootMotion TryDeltaRootMotion(this Component c)
        {
            if (c.TryGetComponent(out IDeltaRootMotion target))
            {
                target.ResetDeltaRootMotion();
                return target;
            }
            return null;
        }

        #endregion

        /// <summary>  Checks if a GameObject has been destroyed. </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }





        #region Layers and Colliders
        /// <summary> Changes the Layer of a GameObject and its children.  </summary>
        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {
            parent.layer = layer;
            if (includeChildren)
            {
                foreach (var trans in parent.transform.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = layer;
            }
        }

        #endregion

        #region SetEnable
        /// <summary>Enable disable the Mono</summary>
        public static void SetEnable(this MonoBehaviour c, bool enable) => c.enabled = enable;
        /// <summary>Enable disable the Mono</summary>
        public static void SetEnable(this Collider c, bool enable) => c.enabled = enable;
        #endregion
    }
}