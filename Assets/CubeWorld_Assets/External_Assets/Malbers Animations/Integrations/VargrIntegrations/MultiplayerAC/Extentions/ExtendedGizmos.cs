using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    /// <summary>
    /// A simple static class for drawing debug geo using Unity Gizmos;
    /// Currently supports:
    /// - Sphere
    /// - 2D Arrow (x,z plane)
    /// - 3D Arrow
    /// </summary>
    public static class ExtendedGizmos
    {

#if UNITY_EDITOR

        private static Vector3[] m_Arrow2DPoints = null;
        private static Vector3[] m_Arrow3DPoints = null;
        private static Vector3[] m_Arrow2DBuffer = null;
        private static Vector3[] m_Arrow3DBuffer = null;
        private static int[] m_Arrow2DIndices = null;
        private static int[] m_Arrow3DIndices = null;

        private static Vector3[] m_BoxBuffer = null;
        private static int[] m_Box3DIndices = null;
        private static int[] m_Box2DIndices = null;

        #region INITIALISATION

        private static void InitialiseArrow2DPoints()
        {
            if (m_Arrow2DPoints == null)
            {
                // Points array (untransformed)
                m_Arrow2DPoints = new Vector3[]
                {
                    Vector3.zero,
                    new Vector3 (-0.1f, 0f, 0.1f),
                    new Vector3 (-0.1f, 0f, 0.6f),
                    new Vector3 (-0.4f, 0f, 0.6f),
                    new Vector3 (0f, 0f, 1f),
                    new Vector3 (0.4f, 0f, 0.6f),
                    new Vector3 (0.1f, 0f, 0.6f),
                    new Vector3 (0.1f, 0f, 0.1f)
                };
            }

            if (m_Arrow2DBuffer == null)
            {
                // Buffer array (transformed)
                m_Arrow2DBuffer = new Vector3[m_Arrow2DPoints.Length];
            }

            if (m_Arrow2DIndices == null)
            {
                // Indices array
                m_Arrow2DIndices = new int[]
                {
                    0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,0
                };
            }
        }

        private static void InitialiseArrow3DPoints()
        {
            if (m_Arrow3DPoints == null)
            {
                // Points array (untransformed)
                m_Arrow3DPoints = new Vector3[]
                {
                    new Vector3(0f, 0f, 0f),

                    new Vector3 (-0.1f, -0.1f, 0.1f),
                    new Vector3 (-0.1f, 0.1f, 0.1f),
                    new Vector3 (0.1f, 0.1f, 0.1f),
                    new Vector3 (0.1f, -0.1f, 0.1f),

                    new Vector3 (-0.1f, -0.1f, 0.6f),
                    new Vector3 (-0.1f, 0.1f, 0.6f),
                    new Vector3 (0.1f, 0.1f, 0.6f),
                    new Vector3 (0.1f, -0.1f, 0.6f),

                    new Vector3 (-0.3f, -0.3f, 0.6f),
                    new Vector3 (-0.3f, 0.3f, 0.6f),
                    new Vector3 (0.3f, 0.3f, 0.6f),
                    new Vector3 (0.3f, -0.3f, 0.6f),

                    new Vector3 (0f, 0f, 1f)
                };
            }

            if (m_Arrow3DBuffer == null)
            {
                // Buffer array (transformed)
                m_Arrow3DBuffer = new Vector3[m_Arrow3DPoints.Length];
            }

            if (m_Arrow3DIndices == null)
            {
                // Indices array
                m_Arrow3DIndices = new int[]
                {
                    // Draw start pyramid
                    0,1,0,2,0,3,0,4,
                    // Draw start square
                    1,2,2,3,3,4,4,1,
                    // Draw connection square
                    5,6,6,7,7,8,8,5,
                    // Draw struts
                    1,5,2,6,3,7,4,8,
                    // Draw connection spurs
                    5,9,6,10,7,11,8,12,
                    // Draw arrow square
                    9,10,10,11,11,12,12,9,
                    // Draw arrow pyramid
                    9,13,10,13,11,13,12,13
                };
            }
        }

        private static void InitialiseBoxPoints()
        {
            if (m_BoxBuffer == null)
            {
                // Buffer array (transformed)
                m_BoxBuffer = new Vector3[8];
            }

            if (m_Box3DIndices == null)
            {
                // 3D indices array
                m_Box3DIndices = new int[]
                {
                    0,1,1,2,2,3,3,0,
                    4,5,5,6,6,7,7,4,
                    0,4,1,5,2,6,3,7
                };
            }

            if (m_Box2DIndices == null)
            {
                // 2D indices array
                m_Box2DIndices = new int[]
                {
                    0,1,1,2,2,3,3,0
                };
            }
        }

        #endregion

        #region DRAW FUNCTIONS

        public static void DrawArrowMarker2D(Vector3 position, float angle, float size, Color colour)
        {
            // Initialise corner points
            InitialiseArrow2DPoints();

            // Get rotation quaternion
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            // Create new array of transformed verts to reduce calculations
            if (m_Arrow2DBuffer == null)
                m_Arrow2DBuffer = new Vector3[m_Arrow2DPoints.Length];
            m_Arrow2DBuffer[0] = position;
            for (int i = 1; i < m_Arrow2DPoints.Length; ++i)
                m_Arrow2DBuffer[i] = position + (rotation * m_Arrow2DPoints[i] * size);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_Arrow2DBuffer, m_Arrow2DIndices);
            }
        }

        public static void DrawArrowMarkerFlat(Vector3 position, Quaternion rotation, float angle, float size, Color colour)
        {
            // Initialise corner points
            InitialiseArrow2DPoints();

            // Get rotation quaternion
            rotation = rotation * Quaternion.Euler(0f, angle, 0f);

            // Create new array of transformed verts to reduce calculations
            if (m_Arrow2DBuffer == null)
                m_Arrow2DBuffer = new Vector3[m_Arrow2DPoints.Length];
            m_Arrow2DBuffer[0] = position;
            for (int i = 1; i < m_Arrow2DPoints.Length; ++i)
                m_Arrow2DBuffer[i] = position + (rotation * m_Arrow2DPoints[i] * size);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_Arrow2DBuffer, m_Arrow2DIndices);
            }
        }

        public static void DrawArrowMarker3D(Vector3 position, Quaternion rotation, float size, Color colour)
        {
            // Initialise corner points
            InitialiseArrow3DPoints();

            // Create new array of transformed verts to reduce calculations
            m_Arrow3DBuffer[0] = position;
            for (int i = 1; i < m_Arrow3DPoints.Length; ++i)
                m_Arrow3DBuffer[i] = position + (rotation * m_Arrow3DPoints[i] * size);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_Arrow3DBuffer, m_Arrow3DIndices);
            }
        }

        public static void DrawCircleMarker2D(Vector3 position, float radius, Color colour)
        {
            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawWireDisc(position, Vector3.up, radius);
            }
        }

        public static void DrawSphereMarker(Vector3 position, float radius, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            // Draw the sphere
            Gizmos.DrawWireSphere(position, radius);

            // Reset colour
            Gizmos.color = prevColour;
        }

        public static void DrawCapsuleMarker(float radius, float height, Vector3 center, Color colour)
        {
            float offset = height * 0.5f - radius;
            if (offset < 0f)
                DrawSphereMarker(center, radius, colour);
            else
            {
                Vector3 p1 = center + Vector3.down * offset;
                Vector3 p2 = center + Vector3.up * offset;
                DrawCapsuleMarker(p1, p2, radius, colour);
            }
        }

        public static void DrawCapsuleMarker(Vector3 p1, Vector3 p2, float radius, Color colour)
        {
            // Special case when both points are in the same position
            if (p1 == p2)
                DrawSphereMarker(p1, radius, colour);
            else
            {
                using (new Handles.DrawingScope(colour, Gizmos.matrix))
                {
                    Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                    Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
                    // Check if capsule direction is collinear to Vector.up
                    float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                    if (c == 1f || c == -1f)
                    {
                        // Fix rotation
                        p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                    }
                    // First side
                    Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                    Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                    Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
                    // Second side
                    Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                    Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                    Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
                    // Lines
                    Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                    Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                    Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                    Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
                }
            }
        }

        public static void DrawCuboidMarker(Vector3 position, float width, float height, Quaternion rotation, Color colour)
        {
            // Set colour
            InitialiseBoxPoints();

            // Get points
            float halfWidth = width * 0.5f;
            m_BoxBuffer[0] = position + rotation * new Vector3(halfWidth, 0f, halfWidth);
            m_BoxBuffer[1] = position + rotation * new Vector3(halfWidth, 0f, -halfWidth);
            m_BoxBuffer[2] = position + rotation * new Vector3(-halfWidth, 0f, -halfWidth);
            m_BoxBuffer[3] = position + rotation * new Vector3(-halfWidth, 0f, halfWidth);
            m_BoxBuffer[4] = position + rotation * new Vector3(halfWidth, height, halfWidth);
            m_BoxBuffer[5] = position + rotation * new Vector3(halfWidth, height, -halfWidth);
            m_BoxBuffer[6] = position + rotation * new Vector3(-halfWidth, height, -halfWidth);
            m_BoxBuffer[7] = position + rotation * new Vector3(-halfWidth, height, halfWidth);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_BoxBuffer, m_Box3DIndices);
            }
        }


        public static void DrawBoxMarker(Vector3 position, Quaternion rotation, Vector3 size, Color colour)
        {
            // Set colour
            InitialiseBoxPoints();

            // Get points
            Vector3 halfSize = size * 0.5f;
            m_BoxBuffer[0] = position + rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            m_BoxBuffer[1] = position + rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            m_BoxBuffer[2] = position + rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            m_BoxBuffer[3] = position + rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            m_BoxBuffer[4] = position + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
            m_BoxBuffer[5] = position + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            m_BoxBuffer[6] = position + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            m_BoxBuffer[7] = position + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_BoxBuffer, m_Box3DIndices);
            }
        }

        public static void DrawBoxMarker2D(Vector3 position, Quaternion rotation, Vector2 size, Color colour)
        {
            // Set colour
            InitialiseBoxPoints();

            // Get points
            Vector2 halfSize = size * 0.5f;
            m_BoxBuffer[0] = position + rotation * new Vector3(halfSize.x, 0f, halfSize.y);
            m_BoxBuffer[1] = position + rotation * new Vector3(halfSize.x, 0f, -halfSize.y);
            m_BoxBuffer[2] = position + rotation * new Vector3(-halfSize.x, 0f, -halfSize.y);
            m_BoxBuffer[3] = position + rotation * new Vector3(-halfSize.x, 0f, halfSize.y);

            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLines(m_BoxBuffer, m_Box2DIndices);
            }
        }

        public static void DrawRay(Vector3 position, Vector3 direction, float length, Color colour)
        {
            // Draw lines
            using (new Handles.DrawingScope(colour, Gizmos.matrix))
            {
                Handles.DrawLine(position, position + direction.normalized * length);
            }
        }

        #endregion

#else

        public static void DrawArrowMarker2D(Vector3 position, float angle, float size, Color colour) { }
        
        public static void DrawArrowMarkerFlat(Vector3 position, Quaternion rotation, float angle, float size, Color colour) { }

        public static void DrawArrowMarker3D(Vector3 position, Quaternion rotation, float size, Color colour) { }

        public static void DrawCircleMarker2D(Vector3 position, float radius, Color colour) { }

        public static void DrawSphereMarker(Vector3 position, float radius, Color colour) { }

        public static void DrawCapsuleMarker(float radius, float height, Vector3 center, Color colour) { }

        public static void DrawCapsuleMarker(Vector3 p1, Vector3 p2, float radius, Color colour) { }

        public static void DrawCuboidMarker(Vector3 position, float width, float height, Quaternion rotation, Color colour) { }

        public static void DrawBoxMarker(Vector3 position, Quaternion rotation, Vector3 size, Color colour) { }

        public static void DrawBoxMarker2D(Vector3 position, Quaternion rotation, Vector2 size, Color colour) { }

        public static void DrawRay(Vector3 position, Vector3 direction, float length, Color colour) { }
        
#endif

        #region ALTERNATIVES

        // Alternate spheres
        public static void DrawSphereMarker (Vector3 position, Color colour) {
			DrawSphereMarker (position, 1f, colour);
		}

		// Alternate 2D arrow
		public static void DrawArrowMarker2D (Vector3 position, Vector3 direction, float size, Color colour) {
            float angle = Quaternion.FromToRotation (Vector3.forward, direction.normalized).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector2 direction, float size, Color colour) {
			float angle = Quaternion.FromToRotation (Vector3.forward, new Vector3 (direction.x, 0f, direction.y)).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
        }

        // Alternate 3D arrow
        public static void DrawArrowMarker3D (Vector3 position, float rx, float ry, float rz, float size, Color colour) {
			DrawArrowMarker3D (position, Quaternion.Euler (rx, ry, rz), size, colour);
		}
		public static void DrawArrowMarker3D (Vector3 position, Vector3 direction, float size, Color colour) {
            DrawArrowMarker3D (position, Quaternion.FromToRotation  (Vector3.forward, direction.normalized), size, colour);
		}

        #endregion
	}
}