using UnityEngine;

namespace ARLocation
{

    /// <summary>
    /// A (open-ended) catmull-rom spline, which interpolates a set points by joining
    /// catmull-rom curves together.
    /// </summary>
    public class CatmullRomSpline : Spline
    {

        /// <summary>
        /// The start-point control/handle.
        /// </summary>
        private Vector3 startHandle;

        /// <summary>
        /// The end-point control/handle.
        /// </summary>
        private Vector3 endHandle;

        /// <summary>
        /// The alpha/tension parameter of the spline.
        /// </summary>
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                CalculateSegments(lastSampleSize);
            }
        }

        float alpha;
        int lastSampleSize;

        /// <summary>
        /// Creates a new Catmull-rom spline.
        /// </summary>
        /// <param name="points">The interpolated points.</param>
        /// <param name="n">The number of samples used in each segment of the spline.</param>
        /// <param name="alpha"></param>
        public CatmullRomSpline(Vector3[] points, int n, float alpha)
        {
            Points = (Vector3[])points.Clone();
            this.alpha = alpha;

            CalculateSegments(n);
        }

        /// <summary>
        /// Calculate the catmull-rom segments. Also estimates the curve's length.
        /// </summary>
        /// <param name="n">The number sample points used to estimate each segment's length.</param>
        public sealed override void CalculateSegments(int n)
        {
            lastSampleSize = n;

            segmentCount = (Points.Length - 1);
            lengths = new float[segmentCount];

            segments = new Curve[segmentCount];

            startHandle = 2 * Points[0] - Points[1];
            endHandle = 2 * Points[segmentCount] - Points[segmentCount - 1];

            Length = 0;
            for (var i = 0; i < segmentCount; i++)
            {
                segments[i] = new CatmullRomCurve(
                    i == 0 ? startHandle : Points[i - 1],
                    Points[i],
                    Points[i + 1],
                    (i + 1) == segmentCount ? endHandle : Points[i + 2],
                    Alpha
                );

                Length += segments[i].EstimateLength(n);
                lengths[i] = Length;
            }
        }

    }
}
