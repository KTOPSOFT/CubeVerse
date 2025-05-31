using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System;
using UnityEngine.Events;
using System.Reflection;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    /// <summary>Redundant functions to be used all over the assets</summary>
    public static class MTools
    {
        #region GameObjects

        /// <summary> Find from a collider the main Root Object. wich contains a IObject Core interface attached to it </summary>
        /// <param name="collider"></param>
        public static GameObject FindRealRoot(Collider collider, bool includeInactive = false)
        {
            var realRoot = collider.transform.root.gameObject;        //Get the animal on the entering collider

            //Find the Right Root if the objets is a Malbers Core Object in Parent
            var coreRoot = collider.GetComponentInParent<IObjectCore>(includeInactive);

            if (coreRoot != null)
            {
                realRoot = coreRoot.transform.gameObject;
            }
            //Means there is no IObjectCore then we are going to find the Parent that uses the same Layer
            else if (realRoot.layer != collider.gameObject.layer)
            {
                realRoot = FindRealParentByLayer(collider.transform);
            }

            return realRoot;
        }
        #endregion

        #region Mesh Rendererers

        public static bool ReboneSkinnedMesh(Transform RootBone, SkinnedMeshRenderer thisRenderer)
        {
            var OldRootBone = thisRenderer.rootBone;

            Transform[] rootBone = RootBone.GetComponentsInChildren<Transform>();

            Dictionary<string, Transform> boneMap = new();

            foreach (Transform bone in rootBone)
            {
                boneMap[bone.name] = bone;
            }

            Transform[] boneArray = thisRenderer.bones;


            for (int idx = 0; idx < boneArray.Length; ++idx) //Remap the bones
            {
                string boneName = boneArray[idx].name;

                if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    Debug.LogError("failed to get bone: " + boneName);
                    return false;
                }
            }
            thisRenderer.bones = boneArray;

            if (boneMap.TryGetValue(OldRootBone.name, out Transform newRoot))
            {
                thisRenderer.rootBone = newRoot; //Remap the rootbone
            }

            return true;
        }
        #endregion

        #region Math


        /// <summary>
        /// Returns a non-normalized projection of the supplied vector onto a plane as described by its normal
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="planeNormal">The normal that defines the plane.  Must have a length of 1.</param>
        /// <returns>The component of the vector that lies in the plane</returns>
        public static Vector3 ProjectOntoPlane(Vector3 vector, Vector3 planeNormal)
        {
            return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
        }

        /// <summary>
        /// Calculate the Range of a value with a min and a max reference. 
        /// 0 if the value is greater than the Max.
        /// 1 if the value is lesser than the Min.
        /// </summary>
        /// <param name="value">Value to modify</param>
        /// <param name="min">Returns 1 if the value is lesser than the Min</param>
        /// <param name="max">Returns 0 if the value is greater than the Min</param>
        /// <returns>Returns a normalized value between the min and the max </returns>
        public static float CalculateRangeWeight(this float value, float min, float max)
        {
            if (value <= min)
                return 1;
            else if (value >= max)
                return 0;
            else
                return 1 - ((value - min) / (max - min));
        }

        /// <summary> Takes a number and stores the digits on an array. E.g: 6542 = [6,5,4,2] </summary>

        public static bool DoSpheresIntersect(Vector3 center1, float radius1, Vector3 center2, float radius2)
        {
            float squaredDistance = (center1 - center2).sqrMagnitude;
            float squaredRadii = Mathf.Pow(radius1 + radius2, 2);

            return squaredDistance <= squaredRadii;
        }

        public static float SmoothStep(float min, float max, float value)
        {
            var p = (value - min) / (max - min);
            p = Mathf.Clamp01(p);
            return p * p * (3 - 2 * p);
        }

        /// <summary> Takes a number and stores the digits on an array. E.g: 6542 = [6,5,4,2] </summary>
        public static int[] GetDigits(int num)
        {
            List<int> listOfInts = new();
            while (num > 0)
            {
                listOfInts.Add(num % 10);
                num /= 10;
            }
            listOfInts.Reverse();
            return listOfInts.ToArray();
        }

        /// <summary>Check if x Seconds have elapsed since the Started Time </summary>
        public static bool ElapsedTime(float StartTime, float intervalTime) => (Time.time - StartTime) >= intervalTime;

        #endregion

        #region Comparizon
        /// <summary> Makes and OR comparison of an Int Value with other Ints</summary>
        public static bool CompareOR(int source, params int[] comparison)
        {
            foreach (var item in comparison)
                if (source == item) return true;

            return false;
        }

        /// <summary> Makes and AND comparison of an INT Value with other INTs</summary>
        public static bool CompareAND(int source, params int[] comparison)
        {
            foreach (var item in comparison)
                if (source != item) return false;

            return true;
        }


        /// <summary> Makes and OR comparison of an Bool Value with other Bools</summary>
        public static bool CompareOR(bool source, params bool[] comparison)
        {
            foreach (var item in comparison)
                if (source == item) return true;

            return false;
        }

        /// <summary> Makes and AND comparison of an Bool Value with other Bools</summary>
        public static bool CompareAND(bool source, params bool[] comparison)
        {
            foreach (var item in comparison)
                if (source != item) return false;

            return true;
        }
        #endregion

        #region Types



        public static List<Type> GetAllTypes<T>() => ReflectionUtility.GetAllTypes<T>();
        public static List<Type> GetAllTypes(Type type) => ReflectionUtility.GetAllTypes(type);
        #endregion

        #region Find References
        public static Camera FindMainCamera()
        {
            var MainCamera = Camera.main != null ? Camera.main : GameObject.FindFirstObjectByType<Camera>();
            return MainCamera;
        }
        #endregion

        #region Resources
        public static List<T> GetAllResources<T>() where T : Object
        {
            var reOfType = Resources.FindObjectsOfTypeAll<T>();

            if (reOfType != null) return reOfType.ToList();

            return null;
        }

        public static T GetResource<T>(string name) where T : Object
        {
            var allInstances = GetAllResources<T>();

            T found = allInstances.Find(x => x.name == name);

            return found;
        }
        #endregion

        #region Layers

        /// <summary>  Returns the Parent Object of a transform if they belong to the same layer  </summary>
        public static GameObject FindRealParentByLayer(Transform other)
        {
            if (other.transform.parent == null)
            {
                return other.gameObject;
            }
            else
            {
                if (other.gameObject.layer == other.parent.gameObject.layer)    //Check the Parent
                {
                    //If the Parent is also on the Layer; Keep searching Upwards
                    return FindRealParentByLayer(other.parent);
                }
                else
                {
                    return other.gameObject;                                            //If the Parent is not on the same Layer ... return the Child
                }
            }
        }





        /// <summary> Set a Layer to the Game Object and all its children</summary>
        public static void SetLayer(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
                SetLayer(child, layer);
        }

        /// <summary>True if the colliders layer is on the layer mask</summary>
        public static bool CollidersLayer(Collider collider, LayerMask layerMask) => layerMask == (layerMask | (1 << collider.gameObject.layer));

        /// <summary>True if the colliders layer is on the layer mask</summary>
        public static bool Layer_in_LayerMask(int layer, LayerMask layerMask) => layerMask == (layerMask | (1 << layer));
        #endregion

        #region XmlSerializer
        /// <summary> Serialize a Class to XML</summary>
        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringWriter writer = new StringWriter();
            xml.Serialize(writer, toSerialize);

            return writer.ToString();
        }

        /// <summary>Finds a bit (index) on a Integer</summary>
        public static bool IsBitActive(int IntValue, int index) => (IntValue & (1 << index)) != 0;

        /// <summary> DeSerialize a Class with xml</summary>
        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer xml = new XmlSerializer(typeof(T));
            StringReader reader = new StringReader(toDeserialize);

            return (T)xml.Deserialize(reader);
        }

        #endregion

        #region Camera Direction
        /// <summary>
        /// Calculate the direction from the center of the Screen
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        ///  <param name="hitmask">Just use this layers</param>
        public static Vector3 DirectionFromCamera(Transform origin, float x, float y, out RaycastHit hit, LayerMask hitmask)
        {
            Camera cam = Camera.main;

            hit = new RaycastHit();

            Ray ray = cam.ScreenPointToRay(new Vector2(x * cam.pixelWidth, y * cam.pixelHeight));
            Vector3 dir = ray.direction;

            hit.distance = float.MaxValue;

            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, 100, hitmask);

            foreach (RaycastHit item in hits)
            {
                //Dont Hit anything in this hierarchy
                if (item.transform.SameHierarchy(origin.transform)) continue; //Don't Find yourself

                //If I hit something behind me skip
                if (Vector3.Distance(cam.transform.position, item.point) < Vector3.Distance(cam.transform.position, origin.position)) continue;

                if (hit.distance > item.distance) hit = item;
            }

            if (hit.distance != float.MaxValue)
            {
                dir = (hit.point - origin.position).normalized;
            }

            return dir;
        }

        /// <summary>
        /// Calculate the direction from the ScreenPoint of the Screen and also saves the RaycastHit Info
        /// </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        ///  <param name="hitmask">Just use this layers</param>
        public static Vector3 DirectionFromCamera
            (Camera cam, Transform origin, Vector3 ScreenPoint, out RaycastHit hit, LayerMask hitmask, Transform Ignore = null)
        {
            Ray ray = cam.ScreenPointToRay(ScreenPoint);
            Vector3 dir = ray.direction;

            hit = new RaycastHit
            {
                distance = float.MaxValue,
                point = ray.GetPoint(100)
            };
            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, 100, hitmask);

            foreach (RaycastHit item in hits)
            {
                if (item.transform.SameHierarchy(Ignore)) continue;           //Dont Hit anything the Ingore
                if (item.transform.SameHierarchy(origin)) continue;           //Dont Hit anything in this hierarchy

                //If I hit something behind me skip
                if (Vector3.Distance(cam.transform.position, item.point) < Vector3.Distance(cam.transform.position, origin.position)) continue;

                if (hit.distance > item.distance) hit = item;
            }

            if (hit.distance != float.MaxValue)
            {
                dir = (hit.point - origin.position).normalized;
            }

            return dir;
        }

        /// <summary>Calculate the direction from the center of the Screen </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        public static Vector3 DirectionFromCamera(Transform origin)
        {
            return DirectionFromCamera(origin, 0.5f * Screen.width, 0.5f * Screen.height, out _, -1);
        }


        /// <summary> Calculate the direction from the center of the Screen </summary>
        /// <param name="origin">The start point to calculate the direction</param>
        public static Vector3 DirectionFromCamera(Transform origin, LayerMask layerMask) =>
            DirectionFromCamera(origin, 0.5f * Screen.width, 0.5f * Screen.height, out _, layerMask);

        #endregion

        public static bool RayArcCast(Vector3 center, Quaternion rotation, float angle, float radius, int resolution, LayerMask layer, out RaycastHit hit)
        {
            rotation *= Quaternion.Euler(-angle / 2, 0, 0);

            for (int i = 0; i < resolution; i++)
            {
                Vector3 A = center + rotation * Vector3.forward * radius;

                rotation *= Quaternion.Euler(angle / resolution, 0, 0);

                Vector3 B = center + rotation * Vector3.forward * radius;

                Vector3 AB = B - A;

                Debug.DrawLine(A, B, Color.green);

                if (Physics.Raycast(A, AB, out hit, AB.magnitude, layer, QueryTriggerInteraction.Ignore))
                {
                    return true;
                }
            }

            hit = new RaycastHit();

            return false;
        }

        #region RayCasting
        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin, Vector3 ScreenCenter, int layerMask = 0)
        {
            RaycastHit hit = new();

            Ray ray = cam.ScreenPointToRay(ScreenCenter);

            hit.distance = float.MaxValue;

            RaycastHit[] hits = Physics.RaycastAll(ray, 100, layerMask);

            foreach (RaycastHit rayhit in hits)
            {
                if (rayhit.transform.SameHierarchy(origin)) continue; //Dont Hit anything in this hierarchy

                //If I hit something behind me skip
                if (Vector3.Distance(cam.transform.position, rayhit.point) < Vector3.Distance(cam.transform.position, origin.position)) continue;

                if (hit.distance > rayhit.distance) hit = rayhit;
            }


            return hit;
        }

        public static Vector3 DirectionFromCameraNoRayCast(Camera cam, Vector3 ScreenCenter)
        {
            Ray ray = cam.ScreenPointToRay(ScreenCenter);

            return ray.direction;
        }

        /// <summary> Returns a RaycastHit to the center of the screen</summary>
        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin)
        {
            var Center = new Vector3(0.5f * Screen.width, 0.5f * Screen.height);

            return RayCastHitToCenter(cam, origin, Center);
        }


        /// <summary> Returns a RaycastHit to the center of the screen</summary>
        public static RaycastHit RayCastHitToCenter(Camera cam, Transform origin, LayerMask layerMask)
        {
            var Center = new Vector3(0.5f * Screen.width, 0.5f * Screen.height);

            return RayCastHitToCenter(cam, origin, Center, layerMask);
        }

        #endregion

        #region Vector Math

        public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Vector3 rotationAmount)
        {
            var headRot = boneToRotate.rotation;
            var headToMesh = Quaternion.Inverse(target) * headRot;
            var headOffsetRot = target * Quaternion.Euler(rotationAmount);

            var finalRot = headOffsetRot * headToMesh;

            boneToRotate.rotation = finalRot;
        }

        public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Quaternion rotationAmount)
        {
            var headRot = boneToRotate.rotation;
            var headToMesh = Quaternion.Inverse(target) * headRot;
            var headOffsetRot = target * rotationAmount;

            var finalRot = headOffsetRot * headToMesh;

            boneToRotate.rotation = finalRot;
        }


        /// <summary> Gives the force needed to throw something at a target using Physyics / </summary>
        public static float PowerFromAngle(Vector3 OriginPos, Vector3 TargetPos, float angle)
        {
            Vector2 OriginPos2 = new(OriginPos.x, OriginPos.z);
            Vector2 TargetPos2 = new(TargetPos.x, TargetPos.z);

            float distance = Vector2.Distance(OriginPos2, TargetPos2);
            float gravity = Physics.gravity.y;

            float OriginHeight = OriginPos.y;
            float TargetHeight = TargetPos.y;

            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            float tan = Mathf.Tan(angle * Mathf.Deg2Rad);

            float SquareSpeed = gravity * Mathf.Pow(distance, 2) / (2 * Mathf.Pow(cos, 2) * (TargetHeight - OriginHeight - distance * tan));

            if (SquareSpeed <= 0.0f) return 0.0f; //Check there's no negative value

            return Mathf.Sqrt(SquareSpeed);
        }


        /// <summary> Get the closest point on a line segment. </summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static Vector3 ClosestPointOnLine(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = point - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB < Epsilon) return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
            return a + (aB * t);
        }

        /// <summary>A useful Epsilon</summary>
        public const float Epsilon = 0.0001f;

        public static Vector3 VelocityFromPower(Vector3 OriginPos, float Power, float angle, Vector3 pos)
        {
            Vector3 hitPos = pos;
            OriginPos.y = 0f;
            hitPos.y = 0f;

            Vector3 dir = (hitPos - OriginPos).normalized;
            Quaternion Rot3D = Quaternion.FromToRotation(Vector3.right, dir);
            Vector3 vec = Power * Vector3.right;
            vec = Rot3D * Quaternion.AngleAxis(angle, Vector3.forward) * vec;

            return vec;
        }

        /// <summary>Calculate a Direction from an origin to a target</summary>
        public static Vector3 DirectionTarget(Transform origin, Transform Target, bool normalized = true) =>
            DirectionTarget(origin.position, Target.position, normalized);


        public static Vector3 Quaternion_to_AngularVelocity(Quaternion quaternion)
        {
            quaternion.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;
            Vector3 angularVelocity = angularDisplacement / Time.deltaTime;

            return angularVelocity;
        }

        public static Vector3 DirectionTarget(Vector3 origin, Vector3 Target, bool normalized = true)
        {
            if (normalized)
                return (Target - origin).normalized;

            return (Target - origin);
        }



        /// <summary>
        /// Gets the horizontal angle between two vectors. The calculation
        /// removes any y components before calculating the angle.
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="From">Angle representing the starting vector</param>
        /// <param name="To">Angle representing the resulting vector</param>
        public static float HorizontalAngle(Vector3 From, Vector3 To, Vector3 Up)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(Up, Vector3.Cross(From, To)), Vector3.Dot(From, To));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001f) { lAngle = 0f; }

            return lAngle;
        }


        /// <summary>The angle between dirA and dirB around axis</summary>
        public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
        {
            // Project A and B onto the plane orthogonal target axis
            dirA -= Vector3.Project(dirA, axis);
            dirB -= Vector3.Project(dirB, axis);

            // Find (positive) angle between A and B
            float angle = Vector3.Angle(dirA, dirB);

            // Return angle multiplied with 1 or -1
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }

        public static Vector3 ClosestPointOnPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
         => point + DistanceFromPlane(planeOffset, planeNormal, point) * planeNormal;

        public static float DistanceFromPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
              => Vector3.Dot(planeOffset - point, planeNormal);

        #endregion

        #region Reflexion
        /// <summary> Creates a Delegate for the Property Set  </summary>
        public static UnityAction<T> Property_Set_UnityAction<T>(UnityEngine.Object component, string propName)
        {
            //Get property info required to fetch the setter method
            PropertyInfo prop = component.GetType().GetProperty(propName);

            //Create a Reference to the Setter of the property
            UnityAction<T> active = (UnityAction<T>)
            System.Delegate.CreateDelegate(typeof(UnityAction<T>), component, prop.GetSetMethod());

            return active;
        }
        #endregion

        #region Alignment Coroutines

        public static IEnumerator AlignTransform_Position(Transform slave, Transform target, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            var Wait = new WaitForFixedUpdate();

            Vector3 CurrentPos = slave.position;

            slave.TryDeltaRootMotion(); //Reset DeltaRootMotion


            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                slave.position = Vector3.LerpUnclamped(CurrentPos, target.position, result);
                elapsedTime += Time.fixedDeltaTime;

                MDebug.DrawWireSphere(slave.position, 0.1f, Color.white, 1f);

                yield return Wait;
            }
            slave.position = target.position;

        }


        public static IEnumerator AlignTransform_Position(Transform t1, Vector3 NewPosition, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            var Wait = new WaitForFixedUpdate();

            Vector3 CurrentPos = t1.position;

            t1.TryDeltaRootMotion(); //Reset DeltaRootMotion????


            MDebug.DrawWireSphere(t1.position, 0.1f, Color.cyan, 1f);
            MDebug.DrawWireSphere(NewPosition, 0.1f, Color.cyan, 1f);
            MDebug.DrawLine(t1.position, NewPosition, Color.cyan, 1f);


            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                t1.position = Vector3.LerpUnclamped(CurrentPos, NewPosition, result);
                elapsedTime += Time.fixedDeltaTime;
                yield return Wait;
            }
            t1.position = NewPosition;
        }


        public static IEnumerator AlignTransform(Transform t1, Transform t2, float time, AnimationCurve curve = null)
        {
            yield return AlignTransform(t1, t2.position, t2.rotation, time, curve);
        }

        public static IEnumerator AlignTransform(Transform t1, Vector3 t2Pos, Quaternion t2Rot, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            t1.GetPositionAndRotation(out Vector3 CurrentPos, out Quaternion CurrentRot);
            var Wait = new WaitForFixedUpdate();

            t1.TryDeltaRootMotion();

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                t1.SetPositionAndRotation
                    (Vector3.LerpUnclamped(CurrentPos, t2Pos, result),
                    Quaternion.LerpUnclamped(CurrentRot, t2Rot, result));
                elapsedTime += Time.fixedDeltaTime;

                yield return Wait;
            }
            t1.SetPositionAndRotation(t2Pos, t2Rot);
        }

        public static IEnumerator AlignLookAtTransform(Transform t1, Vector3 target, float AlignOffset, float time, float scale, AnimationCurve AlignCurve)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (target - t1.position);

            direction = Vector3.ProjectOnPlane(direction, t1.up);
            Quaternion FinalRot = Quaternion.LookRotation(direction);



            Vector3 Offset = t1.position + AlignOffset * scale * t1.forward; //Use Offset

            if (AlignOffset != 0)
            {
                //Calculate Real Direction at the End! 
                Quaternion TargetInverse_Rot = Quaternion.Inverse(t1.rotation);
                Quaternion TargetDelta = TargetInverse_Rot * FinalRot;

                var TargetPosition = t1.position + t1.DeltaPositionFromRotate(Offset, TargetDelta);
                direction = ((target) - TargetPosition);

                var debTime = 3f;

                MDebug.Draw_Arrow(TargetPosition, direction, Color.yellow, debTime);
                MDebug.DrawWireSphere(TargetPosition, 0.1f, Color.green, debTime);
                MDebug.DrawWireSphere(target, 0.1f, Color.yellow, debTime);
                direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values
            }

            if (direction.CloseToZero())
            {
                Debug.LogWarning("Direction is Zero. Please set a correct rotation", t1);
                yield return null;

            }
            else
            {
                direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values
                FinalRot = Quaternion.LookRotation(direction);

                Quaternion Last_Platform_Rot = t1.rotation;

                while ((time > 0) && (elapsedTime <= time))
                {
                    float result = AlignCurve != null ? AlignCurve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                    t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                    if (AlignOffset != 0)
                    {
                        Quaternion Inverse_Rot = Quaternion.Inverse(Last_Platform_Rot);
                        Quaternion Delta = Inverse_Rot * t1.rotation;
                        t1.position += t1.DeltaPositionFromRotate(Offset, Delta);
                    }

                    elapsedTime += Time.fixedDeltaTime;
                    Last_Platform_Rot = t1.rotation;


                    Debug.DrawRay(Offset, Vector3.up, Color.white);
                    MDebug.DrawWireSphere(t1.position, t1.rotation, 0.05f * scale, Color.white, 0.2f);
                    MDebug.DrawWireSphere(t1.position, t1.rotation, 0.05f * scale, Color.white, 0.2f);
                    MDebug.DrawWireSphere(Offset, 0.05f * scale, Color.white, 0.2f);
                    MDebug.Draw_Arrow(t1.position, t1.forward, Color.white, 0.2f);

                    yield return wait;
                }
            }
        }


        public static IEnumerator AlignLookAtTransform(Transform t1, Transform t2, float time, float angleOffset = 0, AnimationCurve curve = null)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (t2.position - t1.position).normalized;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);


            Quaternion FinalRot = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angleOffset, 0);


            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);
                elapsedTime += Time.fixedDeltaTime;
                yield return wait;
            }
            t1.rotation = FinalRot;


        }

        public static IEnumerator AlignLookAtTransformDirection(Transform t1, Vector3 direction, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;

            direction = Vector3.ProjectOnPlane(direction, t1.up);

            Quaternion FinalRot = Quaternion.LookRotation(direction);

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);
                elapsedTime += Time.fixedDeltaTime;
                yield return wait;
            }
            t1.rotation = FinalRot;
        }

        public static IEnumerator AlignTransformToTargetDirection(Transform t1, Vector3 t2Pos, Quaternion t2Rot, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;
            Vector3 currentPos = t1.position;
            Quaternion currentRot = t1.rotation;
            Vector3 targetForward = t2Rot * Vector3.forward;

            // Determine if the animal should align to the forward or backward direction of the target    
            float dotProduct = Vector3.Dot(t1.forward, targetForward);

            // If the dot product is negative, align to the opposite direction
            if (dotProduct < 0)
            {
                targetForward = -targetForward;
                t2Rot = Quaternion.LookRotation(targetForward, Vector3.up);
            }

            var wait = new WaitForFixedUpdate();
            while (time > 0 && elapsedTime <= time)
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;
                t1.SetPositionAndRotation(
                    Vector3.LerpUnclamped(currentPos, t2Pos, result),
                    Quaternion.RotateTowards(currentRot, t2Rot, Time.deltaTime * 1000) // Adjust the speed here
                );
                elapsedTime += Time.fixedDeltaTime;

                yield return wait;
            }
            t1.SetPositionAndRotation(t2Pos, t2Rot);
        }

        public static IEnumerator AlignLookAtTransform(Transform t1, Vector3 targetPosition, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();


            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (targetPosition - t1.position).normalized;
            if (direction.CloseToZero())
            {
                Debug.LogWarning("Direction is Zero. Please set a correct rotation", t1);
                yield return null;

            }
            else
            {
                direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values

                Quaternion FinalRot = Quaternion.LookRotation(direction);


                while ((time > 0) && (elapsedTime <= time))
                {
                    float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                    t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                    elapsedTime += Time.fixedDeltaTime;

                    yield return wait;
                }
                t1.rotation = FinalRot;
            }
        }


        public static IEnumerator AlignTransformRadius(Transform TargetToAlign, Vector3 AlignOrigin, float time, float radius, AnimationCurve curve = null)
        {
            if (radius > 0)
            {
                float elapsedTime = 0;

                var Wait = new WaitForFixedUpdate();

                Vector3 CurrentPos = TargetToAlign.position;

                Ray TargetRay = new(AlignOrigin, (TargetToAlign.position - AlignOrigin).normalized);

                Vector3 TargetPos = TargetRay.GetPoint(radius);

                Debug.DrawRay(TargetRay.origin, TargetRay.direction, Color.white, 1f);

                TargetToAlign.TryDeltaRootMotion(); //Reset delta RootMotion

                MDebug.DrawWireSphere(TargetPos, Color.red, 0.05f, 3);

                while ((time > 0) && (elapsedTime <= time))
                {
                    float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                    TargetToAlign.position = Vector3.LerpUnclamped(CurrentPos, TargetPos, result);
                    MDebug.DrawWireSphere(TargetToAlign.position, Color.white, 0.05f, 3);
                    elapsedTime += Time.fixedDeltaTime;
                    yield return Wait;
                }
                TargetToAlign.position = TargetPos;
            }
            yield return null;
        }

        public static IEnumerator AlignTransformRadius(Transform objectToAlign, Transform target, float time, float radius, AnimationCurve curve = null)
        {
            if (radius > 0)
            {
                float elapsedTime = 0;

                var Wait = new WaitForFixedUpdate();

                objectToAlign.TryDeltaRootMotion(); //Reset delta RootMotion

                while ((time > 0) && (elapsedTime <= time))
                {
                    yield return Wait;

                    Vector3 direction = (target.position - objectToAlign.position).normalized;
                    Vector3 TargetPos = target.position - direction * radius;
                    float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                    objectToAlign.position = Vector3.LerpUnclamped(objectToAlign.position, TargetPos, result);

                    MDebug.DrawWireSphere(TargetPos, Color.white, 0.05f, 3);
                    MDebug.DrawRay(TargetPos, Vector3.up, Color.white);


                    MDebug.DrawWireSphere(objectToAlign.position, Color.white, 0.05f, 3);
                    MDebug.DrawRay(objectToAlign.position, Vector3.up, Color.white);

                    elapsedTime += Time.fixedDeltaTime;
                }


                objectToAlign.position = target.position - (target.position - objectToAlign.position).normalized * radius;

            }
            yield return null;
        }


        public static IEnumerator AlignTransform_Rotation(Transform t1, Quaternion NewRotation, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;
            var Wait = new WaitForFixedUpdate();


            Quaternion CurrentRot = t1.rotation;

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve
                t1.rotation = Quaternion.LerpUnclamped(CurrentRot, NewRotation, result);
                elapsedTime += Time.fixedDeltaTime;
                yield return Wait;
            }
            t1.rotation = NewRotation;
        }

        /// <summary> Aligns a transform to a new Local Position Rotation of its parents  </summary>
        public static IEnumerator AlignTransformLocal(Transform obj, Vector3 LocalPos, Vector3 LocalRot, float time)
        {
            float elapsedtime = 0;
            var Wait = new WaitForFixedUpdate();

            Vector3 startPos = obj.localPosition;
            Quaternion startRot = obj.localRotation;

            while (elapsedtime < time)
            {
                obj.localPosition = Vector3.Slerp(startPos, LocalPos, Mathf.SmoothStep(0, 1, elapsedtime / time));
                obj.localRotation = Quaternion.Slerp(startRot, Quaternion.Euler(LocalRot), elapsedtime / time);
                elapsedtime += Time.deltaTime;
                yield return Wait;
            }

            obj.localPosition = LocalPos;
            obj.localEulerAngles = LocalRot;
        }

        /// <summary> Aligns a transform to a new Local Position Rotation and Scale of its parents  </summary>
        public static IEnumerator AlignTransformLocal(Transform obj, Vector3 LocalPos, Vector3 LocalRot, Vector3 localScale, float time)
        {
            float elapsedtime = 0;
            var Wait = new WaitForFixedUpdate();

            obj.GetLocalPositionAndRotation(out Vector3 startPos, out Quaternion startRot);

            Vector3 startScale = obj.localScale;

            while (elapsedtime < time)
            {
                obj.SetLocalPositionAndRotation(

                    Vector3.Slerp(startPos, LocalPos, Mathf.SmoothStep(0, 1, elapsedtime / time)),
                    Quaternion.Slerp(startRot, Quaternion.Euler(LocalRot), elapsedtime / time));

                obj.localScale = Vector3.Lerp(startScale, localScale, Mathf.SmoothStep(0, 1, elapsedtime / time));

                elapsedtime += Time.deltaTime;
                yield return Wait;
            }

            obj.localPosition = LocalPos;
            obj.localEulerAngles = LocalRot;
            obj.localScale = localScale;
        }

        public static IEnumerator AlignTransform(Transform obj, TransformOffset offset, float time)
        {
            yield return AlignTransformLocal(obj, offset.Position, offset.Rotation, offset.Scale, time);
        }

        #endregion

        #region Animator
        public static Keyframe[] DefaultCurve = { new(0, 0), new(1, 1) };

        public static Keyframe[] DefaultCurveLinear = { new(0, 0, 0, 0, 0, 0), new(1, 1, 0, 0, 0, 0) };

        public static Keyframe[] DefaultCurveLinearInverse = { new(0, 1, 0, 0, 0, 0), new(1, 0, 0, 0, 0, 0) };

        public static bool SearchParameter(AnimatorControllerParameter[] parameters, string name)
        {
            foreach (AnimatorControllerParameter item in parameters)
            {
                if (item.name == name) return true;
            }
            return false;
        }

#if UNITY_EDITOR
        public static void AddParametersOnAnimator(UnityEditor.Animations.AnimatorController AnimController, UnityEditor.Animations.AnimatorController Mounted)
        {
            AnimatorControllerParameter[] parameters = AnimController.parameters;
            AnimatorControllerParameter[] Mountedparameters = Mounted.parameters;

            foreach (var param in Mountedparameters)
            {
                if (!SearchParameter(parameters, param.name))
                {
                    AnimController.AddParameter(param);
                }
            }
        }
#endif



        /// <summary> Resets all the Float Parameters on an Animator Controller </summary>

        public static void ResetFloatParameters(Animator animator)
        {
            if (animator)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)                          //Set All Float values to their defaut (For all the Float Values on the Controller
                {
                    if (animator.IsParameterControlledByCurve(parameter.name)) continue;

                    if (parameter.type == AnimatorControllerParameterType.Float)
                    {
                        animator.SetFloat(parameter.nameHash, parameter.defaultFloat);
                    }
                }
            }
        }

        /// <summary>  Finds if a parameter exist on a Animator Controller using its name </summary>
        public static bool FindAnimatorParameter(Animator animator, AnimatorControllerParameterType type, string ParameterName)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == type && parameter.name == ParameterName) return true;
            }
            return false;
        }

        /// <summary>Finds if a parameter exist on a Animator Controller using its nameHash </summary>
        public static bool FindAnimatorParameter(Animator animator, AnimatorControllerParameterType type, int hash)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == type && parameter.nameHash == hash) return true;
            }
            return false;
        }

        #endregion


        /// <summary>Checks and find the correct component to apply a reaction  </summary>  
        public static T VerifyComponent<T>(Object obj, T component) where T : Object
        {
            //Do nothing if is the same object
            if (component == obj) return component;
            //if the object is null then the reference is also null
            if (obj == null) return null;

            var TType = typeof(T);

            if (TType.IsAssignableFrom(obj.GetType()))
            {
                return obj as T;
            }
            else if (obj is GameObject GO)
            {
                if (GO.TryGetComponent(TType, out var result))
                {
                    return result as T;
                }
                else
                {
                    var inparent = GO.GetComponentInParent(TType);
                    if (inparent) return inparent as T;
                    var inchildren = GO.GetComponentInChildren(TType);
                    if (inchildren) return inchildren as T;
                }
            }
            else if (obj is Component CO)
            {
                if (CO.TryGetComponent(TType, out var result))
                {
                    return result as T;
                }
                else
                {
                    var inparent = CO.GetComponentInParent(TType);
                    if (inparent) return inparent as T;
                    var inchildren = CO.GetComponentInChildren(TType);
                    if (inchildren) return inchildren as T;
                }
            }
            return null;
        }


        public static T VerifyInterface<T>(Object obj, T component)
        {
            //Do nothing if is the same object
            if (obj is T ObjAsT && ObjAsT.Equals(component)) return component;
            //if the object is null then the reference is also null
            if (obj == null) return default;

            if (obj is GameObject GO)
            {
                var result = GO.FindInterface<T>();
                return result;
            }
            else if (obj is Component CO)
            {
                var result = CO.FindInterface<T>();
                return result;
            }
            return default;
        }


        /// <summary>Starts the recursive function for the closest transform to the specified point</summary>
        /// <param name="rPosition">Reference point for for the closest transform</param>
        /// <param name="rCollider">Transform that represents the collision</param>
        /// <returns></returns>
        public static Transform GetClosestTransform(Vector3 rPosition, Transform rCollider, LayerMask mask)
        {
            // Find the anchor's root transform
            Transform lActorTransform = rCollider;

            // Grab the closest body transform
            float lMinDistance = float.MaxValue;
            Transform lMinTransform = lActorTransform;
            GetClosestTransform(rPosition, lActorTransform, ref lMinDistance, ref lMinTransform, mask);

            // Return it
            return lMinTransform;
        }

        /// <summary> Find the closes transform to the hit position. This is what we'll attach the projectile to </summary>
        /// <param name="rPosition">Hit position</param>
        /// <param name="rTransform">Transform to be tested</param>
        /// <param name="rMinDistance">Current min distance between the hit position and closest transform</param>
        /// <param name="rMinTransform">Closest transform</param>
        public static void GetClosestTransform(Vector3 rPosition, Transform rTransform, ref float rMinDistance, ref Transform rMinTransform, LayerMask mask)
        {
            // Limit what we'll connect to
            if (!rTransform.gameObject.activeInHierarchy) { return; }
            // if (!Layer_in_LayerMask(rTransform.gameObject.layer, mask)) { return; }

            // Debug.Log($"rTransform {rTransform}");

            // If this transform is closer to the hit position, use it
            float lDistance = Vector3.Distance(rPosition, rTransform.position);

            MDebug.DrawLine(rPosition, rTransform.position, Color.red, 0.5f);

            if (lDistance < rMinDistance && Layer_in_LayerMask(rTransform.gameObject.layer, mask))
            {
                rMinDistance = lDistance;
                rMinTransform = rTransform;
            }

            // Check if any child transform is closer to the hit position
            for (int i = 0; i < rTransform.childCount; i++)
            {
                var child = rTransform.GetChild(i);
                GetClosestTransform(rPosition, child, ref rMinDistance, ref rMinTransform, mask);
            }
        }
        ///------------------------------------------------------------EDITOR ONLY ------------------------------------------------------------

        #region Scriptable Objects
#if UNITY_EDITOR
        #region Styles      
        public static GUIStyle StyleDarkGray => Style(new Color(0.35f, 0.5f, 0.7f, 0.2f));
        public static GUIStyle StyleGray => Style(new Color(0.35f, 0.5f, 0.7f, 0.2f));
        public static GUIStyle StyleBlue => Style(MBlue);
        public static GUIStyle StyleGreen => Style(MGreen);
        public static GUIStyle StyleOrange => Style(MOrange);


        public static Color MBlue = new(0.2f, 0.5f, 1f, 0.42f);
        public static Color MRed = new(1.1f, 0.500f, 0.500f, 1.100f);

        public static Color MGreen = new(0f, 1f, 0.4f, 0.3f);
        public static Color MOrange = new(1f, 0.3f, 0.0f, 0.5f);

        #endregion

        public static GUIStyle Style(Color color)
        {
            GUIStyle currentStyle = new(GUI.skin.box) { border = new RectOffset(-1, -1, -1, -1) };
            Color32[] pix = new Color32[1];
            pix[0] = color;
            Texture2D bg = new(1, 1);
            bg.SetPixels32(pix);
            bg.Apply();

            currentStyle.normal.background = bg;
            currentStyle.normal.scaledBackgrounds = new Texture2D[] { };

            return currentStyle;
        }


        public static Object ExtractObject(Object asset, int index)
        {
            bool shouldExtract = true;
            string path = AssetDatabase.GetAssetPath(asset);
            string destinationPath =
                $"{path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal))}/{asset.name}.asset";

            if (AssetDatabase.LoadAssetAtPath(destinationPath, typeof(Object)) != null)
            {
                // Asset with same name found
                shouldExtract = EditorUtility.DisplayDialog($"An asset named {asset.name} already exists",
                    "do you want to override it?", "Yes", "No");
            }

            if (!shouldExtract)
                return null;

            Object clone = Object.Instantiate(asset);
            AssetDatabase.CreateAsset(clone, destinationPath);

            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            assetImporter.AddRemap(new AssetImporter.SourceAssetIdentifier(asset), clone);

            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            AssetDatabase.SaveAssets();
            return clone;
        }

        public static void CheckListIndex(UnityEditorInternal.ReorderableList list)
        {
            list.index -= 1;
            if (list.index == -1 && list.serializedProperty.arraySize > 0) //In Case you remove the first one
                list.index = 0;
        }

        public static void DrawScriptableObject(ScriptableObject serializedObject, bool showscript = true, int skip = 0)
        {
            if (serializedObject == null) return;

            SerializedObject serialied_element = new SerializedObject(serializedObject);
            serialied_element.Update();

            EditorGUI.BeginChangeCheck();

            var property = serialied_element.GetIterator();
            property.NextVisible(true);

            if (!showscript) property.NextVisible(true);

            for (int i = 0; i < skip; i++)
                property.NextVisible(true);

            do
            {
                EditorGUILayout.PropertyField(property, true);
            } while (property.NextVisible(false));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(serializedObject, "Scriptable Object Changed");
                serialied_element.ApplyModifiedProperties();
                if (serializedObject != null) EditorUtility.SetDirty(serializedObject);
            }
        }

        public static void DrawScriptableObject(SerializedProperty property, bool internalInspector = true, bool internalAsset = false, string labelOverride = "")
        {
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference ||
                (property.objectReferenceValue != null && property.objectReferenceValue is not ScriptableObject))
            {
                Debug.LogErrorFormat("Is not a ScriptableObject");
                return;
            }

            if (property.objectReferenceValue != null)
            {
                if (internalInspector)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var title = string.IsNullOrEmpty(labelOverride) ? property.displayName : labelOverride;
                        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, title, true);

                        EditorGUILayout.PropertyField(property, GUIContent.none, true);

                        var remove = EditorGUIUtility.IconContent("d_Toolbar Minus");
                        remove.tooltip = "Remove";

                        if (property.objectReferenceValue != null)
                        {
                            if (GUILayout.Button(remove, GUILayout.Width(24), GUILayout.Height(20)))
                            {
                                if (AssetDatabase.GetAssetPath(property.objectReferenceValue) == null)
                                {
                                    UnityEngine.Object.DestroyImmediate(property.objectReferenceValue); //if the asset exist only in the Monovehaviour
                                }
                                property.objectReferenceValue = null;
                            }
                        }

                    }


                    if (GUI.changed) property.serializedObject.ApplyModifiedProperties();

                    if (property.objectReferenceValue == null) GUIUtility.ExitGUI();

                    if (property.isExpanded)
                    {
                        if (internalAsset)
                            property.objectReferenceValue.name = EditorGUILayout.TextField("Name", property.objectReferenceValue.name);

                        DrawObjectReferenceInspector(property);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(property);

                    var plus = EditorGUIUtility.IconContent("d_Toolbar Plus");
                    plus.tooltip = "Create";


                    if (GUILayout.Button(plus, GUILayout.Width(24), GUILayout.Height(20)))
                    {
                        if (!internalAsset)
                            MTools.CreateScriptableAsset(property, MalbersEditor.GetSelectedPathOrFallback());
                        else
                            MTools.CreateScriptableAssetInternal(property);
                    }
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        public static void AddScriptableAssetContextMenu(SerializedProperty property, Type type, string path)
        {
            var StatesType = MTools.GetAllTypes(type);

            StatesType.OrderBy(t => t.Name);

            var addMenu = new GenericMenu();

            for (int i = 0; i < StatesType.Count; i++)
            {
                Type st = StatesType[i];

                string name = Regex.Replace(st.Name, @"([a-z])([A-Z])", "$1 $2");

                addMenu.AddItem(new GUIContent(name), false, () => CreateScriptableAsset(property, path));
            }
            addMenu.ShowAsContext();
        }

        public static void DrawObjectReferenceInspector(SerializedProperty property)
        {
            if (property != null)
            {
                var objectReference = property.objectReferenceValue;

                if (objectReference != null)
                {
                    MMDrawnPropertiesEditor.MMDrawnProperties(objectReference);
                }
            }
        }

        public static void DrawObjectReferenceInspectorOld(SerializedProperty property)
        {
            if (property != null)
            {
                var objectReference = property.objectReferenceValue;

                if (objectReference != null)
                {
                    Editor.CreateEditor(objectReference).OnInspectorGUI();
                }
            }
        }

        public static void GetSelectedPathOrFallback() => MalbersEditor.GetSelectedPathOrFallback();
        public static void CreateScriptableAsset(SerializedProperty property, string selectedAssetPath)
        {
            var type = MSerializedTools.GetPropertyType(property);

            if (type != null && type.IsAbstract)
            {
                var StatesType = MTools.GetAllTypes(type);

                var addMenu = new GenericMenu();

                for (int i = 0; i < StatesType.Count; i++)
                {
                    Type st = StatesType[i];
                    addMenu.AddItem(new GUIContent(st.Name), false, () => CreateAsset_SavePrompt(property, st, selectedAssetPath));
                }
                addMenu.ShowAsContext();
            }
            else
            {
                CreateAsset_SavePrompt(property, type, "Asset/");
            }
        }

        public static void CreateAsset_SavePrompt(SerializedProperty property, Type type, string selectedAssetPath)
        {
            var newAsset = CreateAssetWithSavePrompt(type, selectedAssetPath);
            property.objectReferenceValue = newAsset;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static void CreateScriptableAssetInternal(SerializedProperty property)
        {
            var type = MSerializedTools.GetPropertyType(property);

            if (type.IsAbstract)
            {
                var StatesType = GetAllTypes(type);

                var addMenu = new GenericMenu();

                for (int i = 0; i < StatesType.Count; i++)
                {
                    Type st = StatesType[i];
                    addMenu.AddItem(new GUIContent(st.Name), false, () => MSerializedTools.CreateAssetInternal(property, st));
                }

                addMenu.ShowAsContext();
            }
            else
            {
                MSerializedTools.CreateAssetInternal(property, type);
            }
        }




        // Creates a new ScriptableObject via the default Save File panel
        public static ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
        {
            if (type == null) return null; //HACK
            if (type.IsAbstract) return null; //HACK

            string defaultName = string.Format("New {0}.asset", type.Name);
            string message = string.Format("Enter a file name for the {0} ScriptableObject.", type.Name);
            path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", defaultName, "asset", message, path);

            if (string.IsNullOrEmpty(path)) return null;


            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        #region NOT USED
        //public static ScriptableObject CreateScriptableAsset(Type type)
        //{
        //    return CreateAssetWithSavePrompt(type, MalbersEditor.GetSelectedPathOrFallback());
        //}

        //public static void CreateScriptableAssetInternal(SerializedProperty property, Type type, string path)
        //{
        //    var newInstance = ScriptableObject.CreateInstance(type);
        //    newInstance.hideFlags = HideFlags.None;
        //    newInstance.name = type.Name;

        //    property.objectReferenceValue = newInstance;
        //    property.serializedObject.ApplyModifiedProperties();

        //    AssetDatabase.AddObjectToAsset(newInstance, path);
        //    AssetDatabase.SaveAssets();
        //}

        //public static void CreateAsset(Type AssetType)
        //{
        //    var asset = ScriptableObject.CreateInstance(AssetType);
        //    AssetDatabase.CreateAsset(asset, "Assets/New " + AssetType.Name + ".asset");
        //}

        //public static void AddScriptableAssetContextMenuInternal(SerializedProperty property, Type type)
        //{
        //    var StatesType = MTools.GetAllTypes(type);

        //    var addMenu = new GenericMenu();

        //    for (int i = 0; i < StatesType.Count; i++)
        //    {
        //        Type st = StatesType[i];
        //        addMenu.AddItem(new GUIContent(st.Name), false, () => CreateScriptableAssetInternal(property));
        //    }

        //    addMenu.ShowAsContext();
        //}

        //public static void AddScriptableAssetContextMenuInternal(SerializedProperty property, Type type, string path)
        //{
        //    var StatesType = MTools.GetAllTypes(type);
        //    var addMenu = new GenericMenu();

        //    for (int i = 0; i < StatesType.Count; i++)
        //    {
        //        Type st = StatesType[i];
        //        addMenu.AddItem(new GUIContent(st.Name), false, () => CreateScriptableAssetInternal(property, st, path));
        //    }

        //    addMenu.ShowAsContext();
        //}

        //public static void CreateAssetWithPath(SerializedProperty property, string selectedAssetPath)
        //{
        //    Type type = MPropertyTools.GetPropertyType(property); //Get all the Types from an Abstract class

        //    if (type.IsAbstract)
        //    {
        //        var allTypes = MTools.GetAllTypes(type);

        //        var addMenu = new GenericMenu();

        //        for (int i = 0; i < allTypes.Count; i++)
        //        {
        //            Type st = allTypes[i];

        //            var Rname = st.Name;
        //            addMenu.AddItem(new GUIContent(Rname), false, () => CreateScriptableAsset(property, selectedAssetPath));

        //        }

        //        addMenu.ShowAsContext();
        //        EditorGUILayout.EndHorizontal();
        //        property.serializedObject.ApplyModifiedProperties();
        //        return;
        //    }
        //    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        //    {
        //        type = type.GetGenericArguments()[0];
        //    }
        //    property.objectReferenceValue = CreateAssetWithSavePrompt(type, selectedAssetPath);
        //}
        #endregion

#endif
        #endregion

        //        /// <summary>  Removes a Method from a Unity Event  </summary>
        //        public static void RemovePersistentListener(UnityEvent _event, string methodName, UnityEngine.Object Methodtarget)
        //        {
        //#if UNITY_EDITOR
        //            int isThere = -1;

        //            for (int i = 0; i < _event.GetPersistentEventCount(); i++)
        //            {
        //                var L_methName = _event.GetPersistentMethodName(i);
        //                UnityEngine.Object targetListener = _event.GetPersistentTarget(i);

        //                Debug.Log("Method: " + L_methName + " Target: " + targetListener);
        //                if (L_methName == methodName && targetListener == Methodtarget)
        //                {
        //                    isThere = i;
        //                    break;
        //                }
        //            }

        //            if (isThere != -1) UnityEditor.Events.UnityEventTools.RemovePersistentListener(_event, isThere);
        //#endif
        //        }


        public static void SetDirty(Object ob)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(ob);
#endif
        }

        /// <summary> Returns all the Instances created on the Project for an Scriptable Asset. WORKS ON EDITOR ONLY </summary>
        public static List<T> GetAllInstances<T>() where T : Object
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
                T[] a = new T[guids.Length];

                for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                    a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                }
                var aA = a.ToList();

                return aA;
            }
#endif
            return null;
        }

        /// <summary>Returns the Instance of an Scriptable Object by its name. WORKS ON EDITOR ONLY</summary>
        public static T GetInstance<T>(string name) where T : Object
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var allInstances = GetAllInstances<T>();

                T found = allInstances.Find(x => x.name == name);

                return found;
            }
#endif
            return null;
        }


        ///// <summary>
        ///// Extracts the type of a serialized property.
        ///// </summary>
        ///// <param name="property">The serialized property.</param>
        ///// <returns>The type of the serialized property.</returns>
        //public static Type ExtractPropertyType(SerializedProperty property)
        //{
        //    string propertyType = property.type;
        //    string[] typeParts = propertyType.Split(' ');

        //    if (typeParts.Length > 0)
        //    {
        //        string typeName = typeParts[0];
        //        return Type.GetType(typeName);
        //    }

        //    return null;
        //}
    }
}