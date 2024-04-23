using UnityEngine;
using M = System.Math;
// ReSharper disable InconsistentNaming

namespace ARLocation
{
    [System.Serializable]
    public struct DVector2
    {
        public double x;
        public double y;

        /// <summary>
        /// Gets the magnitude of the vector.
        /// </summary>
        /// <value>The magnitude.</value>
        public double magnitude
        {
            get
            {
                return M.Sqrt(x * x + y * y);
            }
        }

        /// <summary>
        /// Gets the normalized version of this vector.
        /// </summary>
        /// <value>The normalized.</value>
        public DVector2 normalized
        {
            get
            {
                var m = magnitude;

                if (m < 0.00000001)
                {
                    return new DVector2();
                }

                return new DVector2(x, y) / magnitude;
            }
        }

        public DVector2 Clone()
        {
            return new DVector2(x, y);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DVector2"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public DVector2(double x = 0.0, double y = 0.0)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Converts to a Vector2.
        /// </summary>
        /// <returns>The vector2.</returns>
        public Vector2 toVector2()
        {
            return new Vector2((float)x, (float)y);
        }

        /// <summary>
        /// Equals the specified v and e.
        /// </summary>
        /// <returns>The equals.</returns>
        /// <param name="v">V.</param>
        /// <param name="e">E.</param>
        public bool Equals(DVector2 v, double e = 0.00005)
        {
            return (M.Abs(x - v.x) <= e) && (M.Abs(y - v.y) <= e);
        }

        /// <summary>
        /// Normalize this instance.
        /// </summary>
        public void Normalize()
        {
            double m = magnitude;
            x /= m;
            y /= m;
        }

        /// <summary>
        /// Set the specified x and y.
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        public void Set(double newX = 0.0, double newY = 0.0)
        {
            x = newX;
            y = newY;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:DVector2"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:DVector2"/>.</returns>
        override public string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        /// <summary>
        /// Dot the specified a and b.
        /// </summary>
        /// <returns>The dot.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        public static double Dot(DVector2 a, DVector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        /// <summary>
        /// Distance the specified a and b.
        /// </summary>
        /// <returns>The distance.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        public static double Distance(DVector2 a, DVector2 b)
        {
            return M.Sqrt(a.x * b.x + a.y * b.y);
        }

        /// <summary>
        /// Lerp the specified a, b and t.
        /// </summary>
        /// <returns>The lerp.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="t">T.</param>
        public static DVector2 Lerp(DVector2 a, DVector2 b, double t)
        {
            double s = M.Max(0, M.Min(t, 1));
            return a * (1 - s) + b * s;
        }

        /// <summary>
        /// Computes the product of <c>a</c> and <c>b</c>, yielding a new <see cref="T:DVector2"/>.
        /// </summary>
        /// <param name="a">The <see cref="DVector2"/> to multiply.</param>
        /// <param name="b">The <see cref="double"/> to multiply.</param>
        /// <returns>The <see cref="T:DVector2"/> that is the <c>a</c> * <c>b</c>.</returns>
        public static DVector2 operator *(DVector2 a, double b)
        {
            return new DVector2(
                b * a.x,
                b * a.y
            );
        }

        /// <summary>
        /// Computes the division of <c>a</c> and <c>b</c>, yielding a new <see cref="T:DVector2"/>.
        /// </summary>
        /// <param name="a">The <see cref="DVector2"/> to divide (the divident).</param>
        /// <param name="b">The <see cref="double"/> to divide (the divisor).</param>
        /// <returns>The <see cref="T:DVector2"/> that is the <c>a</c> / <c>b</c>.</returns>
        public static DVector2 operator /(DVector2 a, double b)
        {
            return new DVector2(
                a.x / b,
                a.y / b
            );
        }

        /// <summary>
        /// Adds a <see cref="DVector2"/> to a <see cref="DVector2"/>, yielding a new <see cref="T:DVector2"/>.
        /// </summary>
        /// <param name="a">The first <see cref="DVector2"/> to add.</param>
        /// <param name="b">The second <see cref="DVector2"/> to add.</param>
        /// <returns>The <see cref="T:DVector2"/> that is the sum of the values of <c>a</c> and <c>b</c>.</returns>
        public static DVector2 operator +(DVector2 a, DVector2 b)
        {
            return new DVector2(
                a.x + b.x,
                a.y + b.y
            );
        }

        /// <summary>
        /// Subtracts a <see cref="DVector2"/> from a <see cref="DVector2"/>, yielding a new <see cref="T:DVector2"/>.
        /// </summary>
        /// <param name="a">The <see cref="DVector2"/> to subtract from (the minuend).</param>
        /// <param name="b">The <see cref="DVector2"/> to subtract (the subtrahend).</param>
        /// <returns>The <see cref="T:DVector2"/> that is the <c>a</c> minus <c>b</c>.</returns>
        public static DVector2 operator -(DVector2 a, DVector2 b)
        {
            return new DVector2(
                a.x - b.x,
                a.y - b.y
            );
        }
    }
}
