using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> Malbers Debug Class to Draw Different debug shapes  </summary>
    public static class MDebug
    {
        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void Log(object message) => Debug.Log(message);

        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void Log(object message, Object context) => Debug.Log(message, context);

        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void LogWarning(object message, Object context) => Debug.LogWarning(message, context);

        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void LogWarning(object message) => Debug.LogWarning(message);

        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void LogError(object message) => Debug.LogError(message);

        [System.Diagnostics.Conditional("MALBERS_DEBUG")]
        public static void LogError(object message, Object context) => Debug.LogError(message, context);


        /// <summary>  Draw an arrow Using Gizmos  </summary>
        public static void Gizmo_Arrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            if (direction == Vector3.zero) return;

            var length = direction.magnitude;

            Gizmos.DrawRay(pos, direction);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * (arrowHeadLength * length));
            Gizmos.DrawRay(pos + direction, left * (arrowHeadLength * length));
#endif
        }

        /// <summary>  Draw an arrow using Debug.Draw</summary>
        public static void Draw_Arrow(Vector3 pos, Vector3 direction, Color color, float duration = 0, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            if (direction == Vector3.zero) return;
            Debug.DrawRay(pos, direction, color, duration);

            var length = direction.magnitude;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, arrowHeadLength * length * right, color, duration);
            Debug.DrawRay(pos + direction, arrowHeadLength * length * left, color, duration);
#endif
        }

        public static void DrawRay(Vector3 pos, Vector3 direction, Color color, float duration = 0)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Debug.DrawRay(pos, direction, color, duration);
#endif
        }

        public static void DrawLine(Vector3 pos1, Vector3 pos2, Color color, float duration = 0)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Debug.DrawLine(pos1, pos2, color, duration);
#endif
        }

        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG


            var drawAngle = 360 / Steps;
            Vector3 Lastpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 point = position + rotation * new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;
                Debug.DrawLine(point, Lastpoint, color, duration, false);
                Lastpoint = point;
            }
#endif

        }

        public static void DrawCircle(Vector3 position, Vector3 normal, float radius, Color color, bool cross = false, float duration = 0, int steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG


            var forward = Vector3.Cross(normal, Vector3.up).normalized;
            var right = Vector3.Cross(normal, forward).normalized;

            var drawAngle = 360f / steps;
            var lastPoint = position + forward * radius;

            for (int i = 0; i <= steps; i++)
            {
                float angle = i * drawAngle * Mathf.Deg2Rad;
                var point = position + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                Debug.DrawLine(point, lastPoint, color, duration, false);
                lastPoint = point;
            }

            //draw Cross
            if (cross)
            {
                //first line
                var firstPoint = position + forward * radius;
                float angle = steps / 2 * drawAngle * Mathf.Deg2Rad;
                var point1 = position + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                Debug.DrawLine(firstPoint, point1, color, duration, false);

                //second line
                angle = steps * 0.25f * drawAngle * Mathf.Deg2Rad;
                firstPoint = position + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                angle = steps * 0.75f * drawAngle * Mathf.Deg2Rad;
                point1 = position + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                Debug.DrawLine(firstPoint, point1, color, duration, false);
            }

#endif

        }

        public static void DrawWireSphere(Vector3 position, Color color, float radius = 1.0f, float drawDuration = 0, int Steps = 36)
            => DrawWireSphere(position, Quaternion.identity, color, radius, 1, drawDuration, Steps);

        public static void DrawWireSphere(Vector3 position, float radius, Color color, float drawDuration = 0, int Steps = 36)
            => DrawWireSphere(position, Quaternion.identity, color, radius, 1, drawDuration, Steps);

        public static void DrawWireSphere(Vector3 position, Quaternion rotation, float radius, Color color, float drawDuration = 0, int Steps = 36)
            => DrawWireSphere(position, rotation, color, radius, 1, drawDuration, Steps);

        public static void DrawWireSphere(Vector3 position, Quaternion rotation, Color color, float radius = 1.0f, float scale = 1f, float drawDuration = 0, int Steps = 1)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position;

            var drawAngle = 360 / (Steps);
            Gizmos.color = color;

            var r = radius * scale;


            Vector3 LastXpoint = position + rotation * new Vector3(0, Mathf.Cos(0), Mathf.Sin(0)) * r;
            Vector3 LastYpoint = position + rotation * new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * r;
            Vector3 LastZpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * r;

            //draw the 4 lines
            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointX = position + rotation * new Vector3(0, Mathf.Cos(a), Mathf.Sin(a)) * r;
                Vector3 pointY = position + rotation * new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * r;
                Vector3 pointZ = position + rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r;

                Debug.DrawLine(pointX, LastXpoint, color, drawDuration);
                Debug.DrawLine(pointY, LastYpoint, color, drawDuration);
                Debug.DrawLine(pointZ, LastZpoint, color, drawDuration);

                LastXpoint = pointX;
                LastYpoint = pointY;
                LastZpoint = pointZ;
            }
#endif
        }


        /// <summary> Draw a Capsule Gizmo using a center and a rotation</summary>
        /// <param name="Center">center of the capsule</param>
        /// <param name="height">Height of the capsule</param>
        /// <param name="radius">Radius of the capsule</param>
        /// <param name="direction">0:(X) Rigth, 1:(Y) Up 2:(Z) Forward</param>
        public static void DrawCapsule(Vector3 Center, Quaternion rotation, float height, float radius, Color color, int direction = 1, int Steps = 36)
        {
            Vector3 point1, point2;

            height = Mathf.Clamp(height, radius * 2, height);

            if (direction == 0)
            {
                point1 = Center + rotation * (Vector3.right * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.right * (height / 2 - radius));
                DrawCapsule(point1, point2, rotation * Quaternion.Euler(0, 0, -90), radius, color, Steps);

            }
            else if (direction == 1)
            {
                point1 = Center + rotation * (Vector3.up * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.up * (height / 2 - radius));

                DrawCapsule(point1, point2, rotation, radius, color, Steps);
            }
            else
            {
                point1 = Center + rotation * (Vector3.forward * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.forward * (height / 2 - radius));

                DrawCapsule(point1, point2, rotation * Quaternion.Euler(90, 0, 0), radius, color, Steps);
            }
        }


        private static void DrawCapsule(Vector3 point1, Vector3 point2, Quaternion rot, float radius, Color color, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            GizmoWireHemiSphere(point1, rot * Quaternion.Euler(-90, 0, 0), radius, color, Steps);
            GizmoWireHemiSphere(point2, rot * Quaternion.Euler(90, 0, 0), radius, color, Steps);

            var Forward = rot * Vector3.forward;
            var Right = rot * Vector3.right;


            // Draw the cylinder
            int lines = 4;
            for (int i = 1; i <= lines; i++)
            {
                // Cylinder
                Debug.DrawLine(point1 +
                    ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                    (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)),
                               point2 +
                               ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                               (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)));
            }
#endif
        }


        /// <summary> Draw a Capsule Gizmo using a center and a rotation</summary>
        /// <param name="Center">center of the capsule</param>
        /// <param name="height">Height of the capsule</param>
        /// <param name="radius">Radius of the capsule</param>
        /// <param name="direction">0:(X) Rigth, 1:(Y) Up 2:(Z) Forward</param>
        public static void GizmoCapsule(Vector3 Center, Quaternion rotation, float height, float radius, Color color, int direction = 1, int Steps = 36)
        {
            Vector3 point1, point2;

            height = Mathf.Clamp(height, radius * 2, height);

            if (direction == 0)
            {
                point1 = Center + rotation * (Vector3.right * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.right * (height / 2 - radius));
                GizmoCapsule(point1, point2, rotation * Quaternion.Euler(0, 0, -90), radius, color, Steps);

            }
            else if (direction == 1)
            {
                point1 = Center + rotation * (Vector3.up * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.up * (height / 2 - radius));

                GizmoCapsule(point1, point2, rotation, radius, color, Steps);
            }
            else
            {
                point1 = Center + rotation * (Vector3.forward * (height / 2 - radius));
                point2 = Center + rotation * (-Vector3.forward * (height / 2 - radius));

                GizmoCapsule(point1, point2, rotation * Quaternion.Euler(90, 0, 0), radius, color, Steps);
            }
        }

        private static void GizmoCapsule(Vector3 point1, Vector3 point2, Quaternion rot, float radius, Color color, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            GizmoWireHemiSphere(point1, rot * Quaternion.Euler(-90, 0, 0), radius, color, Steps);
            GizmoWireHemiSphere(point2, rot * Quaternion.Euler(90, 0, 0), radius, color, Steps);

            var Forward = rot * Vector3.forward;
            var Right = rot * Vector3.right;


            // Draw the cylinder
            int lines = 4;
            for (int i = 1; i <= lines; i++)
            {
                // Cylinder
                Gizmos.DrawLine(point1 +
                    ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                    (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)),
                               point2 +
                               ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                               (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)));
            }
#endif
        }


        public static void DrawCapsule(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            if (point1 == point2)
            {
                Gizmos.color = color;
                DrawCircle(point1, Quaternion.identity, radius, color, duration, Steps); return;
            }

            var rot = Quaternion.LookRotation(point2 - point1) * Quaternion.Euler(-90, 0, 0);

            DrawWireHemiSphere(point1, rot * Quaternion.Euler(-90, 0, 0), radius, color, duration, Steps);
            DrawWireHemiSphere(point2, rot * Quaternion.Euler(90, 0, 0), radius, color, duration, Steps);

            var Forward = rot * Vector3.forward;
            var Right = rot * Vector3.right;


            // Draw the cylinder
            int lines = 4;
            for (int i = 1; i <= lines; i++)
            {
                // Cylinder
                Debug.DrawLine(point1 +
                    ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                    (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)),
                               point2 +
                               ((Mathf.Cos(i * 2 * Mathf.PI / lines) * radius * Forward) +
                               (Mathf.Sin(i * 2 * Mathf.PI / lines) * radius * Right)), color, duration);
            }
#endif
        }

        public static void GizmoWireSphere(Vector3 position, Quaternion rotation, float radius, Color color, float scale = 1, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position;

            var drawAngle = 360 / Steps;
            Gizmos.color = color;

            var r = radius * scale;


            Vector3 LastXpoint = position + rotation * new Vector3(0, Mathf.Cos(0), Mathf.Sin(0)) * r;
            Vector3 LastYpoint = position + rotation * new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * r;
            Vector3 LastZpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * r;

            //draw the 4 lines
            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointX = position + rotation * new Vector3(0, Mathf.Cos(a), Mathf.Sin(a)) * r;
                Vector3 pointY = position + rotation * new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * r;
                Vector3 pointZ = position + rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r;

                Gizmos.DrawLine(pointX, LastXpoint);
                Gizmos.DrawLine(pointY, LastYpoint);
                Gizmos.DrawLine(pointZ, LastZpoint);

                LastXpoint = pointX;
                LastYpoint = pointY;
                LastZpoint = pointZ;
            }
#endif
        }


        public static void GizmoCircle(Vector3 position, Quaternion rotation, float radius, Color color, float scale = 1, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position;

            var drawAngle = 360 / Steps;
            Gizmos.color = color;
            Vector3 Lastpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius * scale;

            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 point = position + rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius * scale;
                Gizmos.DrawLine(point, Lastpoint);
                Lastpoint = point;
            }
#endif
        }

        public static void DrawWireHemiSphere(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            //Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position;

            var drawAngle = 360 / Steps;

            Vector3 LastXpoint = position + rotation * new Vector3(0, Mathf.Cos(0), Mathf.Sin(0)) * radius;
            Vector3 LastYpoint = position + rotation * new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;
            Vector3 LastZpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            //draw the 4 lines
            for (int i = 0; i <= Steps / 2; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointX = position + rotation * new Vector3(0, Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Vector3 pointY = position + rotation * new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;

                Debug.DrawLine(pointX, LastXpoint, color, duration);
                Debug.DrawLine(pointY, LastYpoint, color, duration);

                LastXpoint = pointX;
                LastYpoint = pointY;
            }

            //Draw the Circle
            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointZ = position + rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius;

                Debug.DrawLine(pointZ, LastZpoint, color, duration);

                LastZpoint = pointZ;
            }
#endif
        }


        public static void GizmoWireHemiSphere(Vector3 position, Quaternion rotation, float radius, Color color, int Steps = 36)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            //Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position;

            var drawAngle = 360 / Steps;
            Gizmos.color = color;


            Vector3 LastXpoint = position + rotation * new Vector3(0, Mathf.Cos(0), Mathf.Sin(0)) * radius;
            Vector3 LastYpoint = position + rotation * new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;
            Vector3 LastZpoint = position + rotation * new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            //draw the 4 lines
            for (int i = 0; i <= Steps / 2; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointX = position + rotation * new Vector3(0, Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Vector3 pointY = position + rotation * new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;

                Gizmos.DrawLine(pointX, LastXpoint);
                Gizmos.DrawLine(pointY, LastYpoint);

                LastXpoint = pointX;
                LastYpoint = pointY;
            }

            //Draw the Circle
            for (int i = 0; i <= Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 pointZ = position + rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * radius;

                Gizmos.DrawLine(pointZ, LastZpoint);

                LastZpoint = pointZ;
            }
#endif
        }


        public static void GizmoCross(Transform m_transform)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            var gizmoSize = 0.25f;
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.forward * gizmoSize / m_transform.localScale.z));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.forward * -gizmoSize / m_transform.localScale.z));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.up * gizmoSize / m_transform.localScale.y));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.up * -gizmoSize / m_transform.localScale.y));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.right * gizmoSize / m_transform.localScale.x));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.right * -gizmoSize / m_transform.localScale.x));
#endif
        }

        public static void DrawCone(Vector3 position, Quaternion rotation, float FOV, float length, Color color, float scale = 1, int Steps = 4)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Vector3 forward = rotation * Vector3.forward;
            Vector3 endPosition = position + forward * length * scale;

            //draw the end of the cone
            float endRadius = Mathf.Tan(FOV * 0.5f * Mathf.Deg2Rad) * length * scale;

            var drawAngle = 360 / Steps;

            Gizmos.color = color;

            //draw the 4 lines
            for (int i = 0; i < Steps; i++)
            {
                float a = i * drawAngle * Mathf.Deg2Rad;
                Vector3 point = rotation * new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * endRadius;
                Gizmos.DrawLine(position, position + point + forward * length * scale);
            }

            GizmoCircle(endPosition, rotation, endRadius, color);
#endif
        }

        public static void DrawTriggers(Transform transform, Collider col, Color DebugColor, bool always = false)
        {

#if UNITY_EDITOR && MALBERS_DEBUG

            Gizmos.color = DebugColor;
            var DColorFlat = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1f);

            Gizmos.matrix = transform.localToWorldMatrix;

            if (col != null)
                if (always || col.enabled)
                {
                    var isen = col.enabled;
                    col.enabled = true;

                    if (col is BoxCollider)
                    {
                        BoxCollider _C = col as BoxCollider;

                        var sizeX = transform.lossyScale.x * _C.size.x;
                        var sizeY = transform.lossyScale.y * _C.size.y;
                        var sizeZ = transform.lossyScale.z * _C.size.z;
                        Matrix4x4 rotationMatrix = Matrix4x4.TRS(_C.bounds.center, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));

                        Gizmos.matrix = rotationMatrix;

                        //Debug.Log("dasd = " + DColorFlat);
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.color = DColorFlat;
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                    }
                    else if (col is SphereCollider)
                    {
                        SphereCollider _C = col as SphereCollider;
                        Gizmos.matrix = transform.localToWorldMatrix;


                        Gizmos.DrawSphere(Vector3.zero + _C.center, _C.radius);
                        Gizmos.color = DColorFlat;
                        Gizmos.DrawWireSphere(Vector3.zero + _C.center, _C.radius);
                    }
                    col.enabled = isen;
                }
#endif
        }

        public static void DebugCross(Vector3 center, float radius, Color color)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Debug.DrawLine(center - new Vector3(0, radius, 0), center + new Vector3(0, radius, 0), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);
#endif

        }

        public static void DebugPlane(Vector3 center, float radius, Color color, bool cross = false)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, -radius), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center + new Vector3(0, 0, radius), center - new Vector3(-radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(radius, 0, 0), color);

            if (cross)
            {
                Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
                Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);
            }
#endif

        }

        public static void DebugTriangle(Vector3 center, float radius, Color color)
        {
#if UNITY_EDITOR && MALBERS_DEBUG

            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(0, 0, radius), color);

            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(-radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center - new Vector3(0, -radius, 0), center + new Vector3(0, 0, -radius), color);

            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, -radius), color);
            Debug.DrawLine(center - new Vector3(radius, 0, 0), center + new Vector3(0, 0, radius), color);
            Debug.DrawLine(center + new Vector3(0, 0, radius), center - new Vector3(-radius, 0, 0), color);
            Debug.DrawLine(center - new Vector3(0, 0, radius), center + new Vector3(radius, 0, 0), color);
#endif

        }

        //        public static void DrawThickLine(Vector3 start, Vector3 end, Color color, float thickness = 2f)
        //        {
        //#if UNITY_EDITOR && MALBERS_DEBUG
        //            Camera c = Camera.current;
        //            if (c == null) return;

        //            // Only draw on normal cameras
        //            if (c.clearFlags == CameraClearFlags.Depth || c.clearFlags == CameraClearFlags.Nothing)
        //            {
        //                return;
        //            }
        //            Handles.color = color;
        //            // Only draw the line when it is the closest thing to the camera
        //            // (Remove the Z-test code and other objects will not occlude the line.)
        //            var prevZTest = Handles.zTest;
        //            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        //            Handles.color = Gizmos.color;
        //            Handles.DrawAAPolyLine(thickness * 10, new Vector3[] { start, end });

        //            Handles.zTest = prevZTest;
        //#endif
        //        }

        public static void GizmoRay(Vector3 p1, Vector3 dir, float width = 2f)
        {
#if UNITY_EDITOR 

            var p2 = p1 + dir;

            int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
            if (count == 1)
            {
                Gizmos.DrawLine(p1, p2);
            }
            else
            {
                Camera c = Camera.current;
                if (c == null)
                {
                    Debug.LogError("Camera.current is null");
                    return;
                }
                var scp1 = c.WorldToScreenPoint(p1);
                var scp2 = c.WorldToScreenPoint(p2);

                Vector3 v1 = (scp2 - scp1).normalized; // line direction
                Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

                for (int i = 0; i < count; i++)
                {
                    Vector3 o = ((float)i / (count - 1) - 0.5f) * 0.99f * width * n;
                    Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                    Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                    Gizmos.DrawLine(origin, destiny);
                }
            }
#endif
        }

        public static void DrawLine(Vector3 p1, Vector3 p2, float width = 2f)
        {
#if UNITY_EDITOR && MALBERS_DEBUG
            int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
            if (count == 1)
            {
                Gizmos.DrawLine(p1, p2);
            }
            else
            {
                Camera c = Camera.current;
                if (c == null)
                {
                    Debug.LogError("Camera.current is null");
                    return;
                }
                var scp1 = c.WorldToScreenPoint(p1);
                var scp2 = c.WorldToScreenPoint(p2);

                Vector3 v1 = (scp2 - scp1).normalized; // line direction
                Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

                for (int i = 0; i < count; i++)
                {
                    Vector3 o = ((float)i / (count - 1) - 0.5f) * 0.99f * width * n;
                    Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                    Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                    Gizmos.DrawLine(origin, destiny);
                }
            }
#endif
        }


        //internal static void DebugCapsule(Vector3 baseSphere, Vector3 endSphere, Color color, float radius = 1,
        //  bool colorizeBase = true, float drawDuration = 0,  bool drawDepth = false)

        //{
        //    Vector3 up = (endSphere - baseSphere).normalized * radius;
        //    if (up == Vector3.zero)
        //        up = Vector3.up;
        //    Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        //    Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        //    //Radial circles
        //    DrawCircle(baseSphere, up, colorizeBase ? color : Color.red, radius, drawDuration, preview, drawDepth);
        //    DrawCircle(endSphere, -up, color, radius, drawDuration, preview, drawDepth);

        //    bool drawEditor = false;
        //    bool drawGame = false;



        //    if (drawEditor)
        //    {
        //        //Side lines
        //        Debug.DrawLine(baseSphere + right, endSphere + right, color, drawDuration, drawDepth);
        //        Debug.DrawLine(baseSphere - right, endSphere - right, color, drawDuration, drawDepth);

        //        Debug.DrawLine(baseSphere + forward, endSphere + forward, color, drawDuration, drawDepth);
        //        Debug.DrawLine(baseSphere - forward, endSphere - forward, color, drawDuration, drawDepth);

        //        //Draw end caps
        //        for (int i = 1; i < 26; i++)
        //        {
        //            //End endcap
        //            Debug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + endSphere, Vector3.Slerp(right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration,
        //                drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + endSphere, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + endSphere, color,
        //                drawDuration, drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + endSphere, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + endSphere, color,
        //                drawDuration, drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + endSphere, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + endSphere, color,
        //                drawDuration, drawDepth);

        //            //Start endcap
        //            Debug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + baseSphere,
        //                colorizeBase ? color : Color.red, drawDuration, drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + baseSphere,
        //                colorizeBase ? color : Color.red, drawDuration, drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + baseSphere,
        //                colorizeBase ? color : Color.red, drawDuration, drawDepth);
        //            Debug.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + baseSphere,
        //                colorizeBase ? color : Color.red, drawDuration, drawDepth);
        //        }
        //    }
        //}
    }
}
