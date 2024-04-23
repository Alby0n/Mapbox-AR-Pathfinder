using UnityEngine;

namespace ARLocation
{
    public sealed class LinearSpline : Spline
    {
        public LinearSpline(Vector3[] points)
        {
            Points = (Vector3[])points.Clone();

            CalculateSegments(100);
        }

        public override void CalculateSegments(int n)
        {
            segmentCount = (Points.Length - 1);
            segments = new Curve[segmentCount];
            lengths = new float[segmentCount];

            Length = 0.0f;
            for (var i = 0; i < segmentCount; i++)
            {
                segments[i] = new Line(Points[i], Points[i + 1]);
                Length += segments[i].EstimateLength();
                lengths[i] = Length;
            }
        }
    }
}
