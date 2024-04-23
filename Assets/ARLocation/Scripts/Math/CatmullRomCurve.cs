using UnityEngine;

namespace ARLocation
{
    /// <summary>
    /// A catmull-rom curve.
    /// </summary>
    public class CatmullRomCurve : Curve
    {
        // The control points
        private Vector3 p0;
        private Vector3 p1;
        private Vector3 p2;
        private Vector3 p3;

        // The alpha/tension factor
        private float alpha;

        // The knots
        private float t0;
        private float t1;
        private float t2;
        private float t3;

        // Getters for the knots
        public float T0 { get { return t0; } }
        public float T1 { get { return t1; } }
        public float T2 { get { return t2; } }
        public float T3 { get { return t3; } }

        /// <summary>
        /// The curve's cached length
        /// </summary>
        private float length;

        /// <summary>
        /// An array with the sample curve parameters.
        /// </summary>
        private float[] sampleParameters;

        /// <summary>
        /// An array with the curve's lenght at each of each sampled points.
        /// </summary>
        private float[] sampleLengths;

        /// <summary>
        /// Flag dirty sample.
        /// </summary>
        private bool isSampleDirty;

        /// <summary>
        /// The size of the last sample; used for cache validation.
        /// </summary>
        private int lastSampleSize = 100;

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>The alpha.</value>
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                CalculateKnots();

                isSampleDirty = true;
            }
        }

        public Vector3 P0
        {
            get
            {
                return p0;
            }
            set
            {
                p0 = value;
                CalculateKnots();

                isSampleDirty = true;
            }
        }

        public Vector3 P1
        {
            get
            {
                return p1;
            }
            set
            {
                p1 = value;
                CalculateKnots();

                isSampleDirty = true;
            }
        }

        public Vector3 P2
        {
            get
            {
                return p2;
            }
            set
            {
                p2 = value;
                CalculateKnots();

                isSampleDirty = true;
            }
        }

        public Vector3 P3
        {
            get
            {
                return p3;
            }
            set
            {
                p3 = value;
                CalculateKnots();

                isSampleDirty = true;
            }
        }

        /// <summary>
        /// Creates a catmull-rom curve with control points p0, p1, p2 and p3, and with
        /// a given alpha/tension parameter.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="alpha"></param>
        public CatmullRomCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float alpha)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            this.alpha = alpha;

            isSampleDirty = true;

            CalculateKnots();
        }

        private void CalculateKnots()
        {
            var x = alpha * 0.5f;
            t0 = 0.0f;
            t1 = t0 + Mathf.Pow((p1 - p0).sqrMagnitude, x);
            t2 = t1 + Mathf.Pow((p2 - p1).sqrMagnitude, x);
            t3 = t2 + Mathf.Pow((p3 - p2).sqrMagnitude, x);
        }

        /// <summary>
        /// Calculates the curve at a point u, where u is between 0 and 1.
        /// </summary>
        /// <param name="u">The curve parameter in the [0, 1] interval.</param>
        /// <returns></returns>
        public override Vector3 GetPoint(float u)
        {
            u = Mathf.Clamp(u, 0, 1);
            float t = t1 * (1 - u) + t2 * u;

            Vector3 a1 = (t1 - t) / (t1 - t0) * p0 + ((t - t0) / (t1 - t0)) * p1;
            Vector3 a2 = (t2 - t) / (t2 - t1) * p1 + ((t - t1) / (t2 - t1)) * p2;
            Vector3 a3 = (t3 - t) / (t3 - t2) * p2 + ((t - t2) / (t3 - t2)) * p3;

            Vector3 b1 = (t2 - t) / (t2 - t0) * a1 + ((t - t0) / (t2 - t0)) * a2;
            Vector3 b2 = (t3 - t) / (t3 - t1) * a2 + ((t - t1) / (t3 - t1)) * a3;

            Vector3 c = (t2 - t) / (t2 - t1) * b1 + ((t - t1) / (t2 - t1)) * b2;

            return c;
        }

        /// <summary>
        /// Calculates the point and the tangent of the curve.
        /// </summary>
        /// <param name="u">The curve parameter in the [0, 1] interval.</param>
        /// <returns></returns>
        public override CurvePointData GetPointAndTangent(float u)
        {
            u = Mathf.Clamp(u, 0, 1);
            float t = t1 * (1 - u) + t2 * u;

            Vector3 a1 = (t1 - t) / (t1 - t0) * p0 + ((t - t0) / (t1 - t0)) * p1;
            Vector3 a2 = (t2 - t) / (t2 - t1) * p1 + ((t - t1) / (t2 - t1)) * p2;
            Vector3 a3 = (t3 - t) / (t3 - t2) * p2 + ((t - t2) / (t3 - t2)) * p3;

            Vector3 b1 = (t2 - t) / (t2 - t0) * a1 + ((t - t0) / (t2 - t0)) * a2;
            Vector3 b2 = (t3 - t) / (t3 - t1) * a2 + ((t - t1) / (t3 - t1)) * a3;

            Vector3 c = (t2 - t) / (t2 - t1) * b1 + ((t - t1) / (t2 - t1)) * b2;

            Vector3 da1 = (1.0f / (t1 - t0)) * (p1 - p0);
            Vector3 da2 = (1.0f / (t2 - t1)) * (p2 - p1);
            Vector3 da3 = (1.0f / (t3 - t2)) * (p3 - p2);

            Vector3 db1 = (t2 - t) / (t2 - t0) * da1 + ((t - t0) / (t2 - t0)) * da2 +
                (1.0f / (t2 - t0)) * (a2 - a1);
            Vector3 db2 = (t3 - t) / (t3 - t1) * da2 + ((t - t1) / (t3 - t1)) * da3 +
                (1.0f / (t3 - t1)) * (a3 - a2);

            Vector3 dc = (t2 - t) / (t2 - t1) * db1 + ((t - t1) / (t2 - t1)) * db2 +
                (1.0f / (t2 - t1)) * (b2 - b1);


            return new CurvePointData { point = c, tangent = dc };
        }

        /// <summary>
        /// Creates a sample of (N+2) points (i.e., N + start and end points) of
        /// the current curve. Also calculates the length estimate.
        /// </summary>
        /// <returns>The sample.</returns>
        /// <param name="n">N.</param>
        public override Vector3[] Sample(int n)
        {
            lastSampleSize = n;

            if (n == 0)
            {
                return new[] { GetPoint(0), GetPoint(1) };
            }

            var sample = new Vector3[n + 2];
            var delta = 1.0f / (n + 1.0f);
            length = 0.0f;

            sampleParameters = new float[n + 2];
            sampleLengths = new float[n + 2];

            for (var i = 0; i < (n + 2); i++)
            {
                sample[i] = GetPoint(i * delta);
                sampleParameters[i] = i * delta;

                if (i > 0)
                {
                    length += (sample[i] - sample[i - 1]).magnitude;
                }

                sampleLengths[i] = length;
            }

            isSampleDirty = false;

            return (Vector3[])sample.Clone();
        }

        /// <summary>
        /// Returns the estimated length.
        /// </summary>
        /// <returns>The length.</returns>
        /// <param name="n">N.</param>
        public override float EstimateLength(int n = 100)
        {
            if (isSampleDirty || lastSampleSize != n)
            {
                Sample(n);
            }

            return length;
        }

        /// <summary>
        /// Gets the curve parameter for a given length.
        /// </summary>
        /// <returns>The parameter for length.</returns>
        /// <param name="s">S.</param>
        public override float GetParameterForLength(float s)
        {
            if (isSampleDirty)
            {
                Sample(lastSampleSize);
            }

            for (var i = 0; i < (sampleParameters.Length - 1); i++)
            {
                if (s >= sampleLengths[i] && s <= sampleLengths[i + 1])
                {
                    var a = (s - sampleLengths[i]) / (sampleLengths[i + 1] - sampleLengths[i]);
                    return (1 - a) * sampleParameters[i] + a * sampleParameters[i + 1];
                }
            }

            return -1.0f;
        }

        /// <summary>
        /// Gets the curve point at a given length.
        /// </summary>
        /// <returns>The point at length.</returns>
        /// <param name="s">S.</param>
        public override Vector3 GetPointAtLength(float s)
        {
            return GetPoint(GetParameterForLength(s));
        }

        /// <summary>
        /// Gets the CurvePointData which stores the point and tangent
        /// at a given arc-length.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public override CurvePointData GetPointAndTangentAtLength(float s)
        {
            return GetPointAndTangent(GetParameterForLength(s));
        }
    }
}
