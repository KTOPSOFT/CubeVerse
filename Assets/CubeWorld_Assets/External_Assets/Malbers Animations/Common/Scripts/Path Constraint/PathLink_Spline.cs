using UnityEngine;
using UnityEngine.Splines;


#if UNITY_EDITOR
#endif
namespace MalbersAnimations.PathCreation
{
    [AddComponentMenu("Malbers/Animal Controller/Path Link (Unity Spline)")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/path-constraint/path-link-spline")]
    public class PathLink_Spline : MonoBehaviour, IPath
    {
        public SplineContainer spline;
        [Tooltip("Resolution to find the closest point on the path")]
        [Min(1)] public int m_SearchResolution = 50;

        public Vector3 StartPath =>   spline.EvaluatePosition(0);

        public Vector3 EndPath => spline.EvaluatePosition(1);

        public bool IsClosed => spline.Spline.Closed; 

        public Bounds bounds => spline.Spline.GetBounds();

        public float GetClosestTimeOnPath(Vector3 position)
        {
            return FindClosestPoint(position, m_SearchResolution);
        }


        //REVISAR!!!!!
        public Quaternion GetPathRotation(float NormalizedTime)
        {
            var forward = Vector3.Normalize(spline.EvaluateTangent(NormalizedTime));
            var up = spline.EvaluateUpVector(NormalizedTime);
            var rotation = Quaternion.LookRotation(forward, up)// * axisRemapRotation
                 ;

            return rotation;
        }

        public Vector3 GetPointAtTime(float NormalizedTime) => spline.EvaluatePosition(NormalizedTime);

        private float FindClosestPoint(Vector3 p, int stepsPerSegment)
        {
            stepsPerSegment = Mathf.RoundToInt(Mathf.Clamp(stepsPerSegment, 1f, 100f));
            float stepSize = 1f / stepsPerSegment;
            float start = 0;
            float end = 1;
            float bestPos = 0;
            float bestDistance = float.MaxValue;

            Vector3 v0 = spline.EvaluatePosition(0);

            for (float f = start; f <= end; f += stepSize)
            {
                Vector3 v = (Vector3)spline.EvaluatePosition(f);
                float t = p.ClosestTimeOnSegment(v0, v);
                float d = Vector3.SqrMagnitude(p - Vector3.Lerp(v0, v, t));

                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestPos = f - (1 - t) * stepSize;
                }
                v0 = v;
            }
            return bestPos;
        }


        private void Reset()
        {
            spline = GetComponent<SplineContainer>();
        }

        private void OnDrawGizmos()
        {
            if (spline == null) { return; }
            if (spline.Spline.Closed) { return; }   

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(StartPath, 0.02f * transform.localScale.y);
            Gizmos.DrawSphere(EndPath, 0.02f * transform.localScale.y);
        }
    }
}
