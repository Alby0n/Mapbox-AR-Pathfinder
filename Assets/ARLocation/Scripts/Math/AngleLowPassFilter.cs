using System;

namespace ARLocation
{
    public class AngleLowPassFilter
    {
        private LowPassFilter sinLpFilter;
        private LowPassFilter cosLpFilter;
        private double smoothFactor;

        public AngleLowPassFilter(double smoothFactor)
        {
            this.smoothFactor = smoothFactor;

            sinLpFilter = new LowPassFilter(smoothFactor);
            cosLpFilter = new LowPassFilter(smoothFactor);
        }

        public double Apply(double angle)
        {
            if (!(smoothFactor > 0.0))
            {
                return angle;
            }

            var rad = MathUtils.Double.DegreesToRadians(angle);
            var s = Math.Sin(rad);
            var c = Math.Cos(rad);

            var newSin = sinLpFilter.Apply(s);
            var newCos = cosLpFilter.Apply(c);

            return MathUtils.Double.RadiansToDegrees(Math.Atan2(newSin, newCos));
        }

        public void SetFactor(double factor)
        {
            sinLpFilter.smoothFactor = factor;
            cosLpFilter.smoothFactor = factor;
        }
    }
}
