using UnityEngine;

namespace ARLocation
{
    public static class MathUtils
    {
        public enum LineSegmentRegion
        {
            Start,
            Middle,
            End,
        }

        public class PointLineSegmentDistanceResult
        {
            public float Distance;
            public LineSegmentRegion Region;
        }

        public static PointLineSegmentDistanceResult PointLineSegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            var result = new PointLineSegmentDistanceResult { };

            var ap = point - a;
            var ab = b - a;
            var bp = point - b;

            float proj = Vector3.Dot(ap, ab) / ab.magnitude;
            float u = proj / ab.magnitude;

            if (u < 0)
            {
                result.Distance = ap.magnitude;
                result.Region = LineSegmentRegion.Start;
            }
            else if (u > 1)
            {
                result.Distance = bp.magnitude;
                result.Region = LineSegmentRegion.End;
            }
            else
            {
                result.Distance = Mathf.Sqrt(ap.sqrMagnitude - proj * proj);
                result.Region = LineSegmentRegion.Middle;
            }

            return result;
        }

        public static Vector2 HorizontalVector(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 HorizontalVectorToVector3(Vector2 v, float y = 0.0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(HorizontalVector(a), HorizontalVector(b));
        }

        public static Vector3 SetY(Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 SetXZ(Vector3 v, Vector3 w)
        {
            return new Vector3(w.x, v.y, w.z);
        }

        public static float DegreesToRadians(float degrees)
        {
            return Mathf.PI * degrees / 180.0f;
        }

        public static float RadiansToDegrees(float degrees)
        {
            return 180.0f * degrees / Mathf.PI;
        }

        public static class Double
        {
            public static double DegreesToRadians(double degrees)
            {
                return System.Math.PI * degrees / 180.0;
            }

            public static double RadiansToDegrees(double degrees)
            {
                return 180.0 * degrees / System.Math.PI;
            }
        }
    }
}
