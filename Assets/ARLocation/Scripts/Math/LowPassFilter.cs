namespace ARLocation
{
    public class LowPassFilter
    {
        private double lastValue;
        public double smoothFactor;

        public LowPassFilter(double smoothFactor = 0.5f)
        {
            this.smoothFactor = smoothFactor;
        }

        public double Apply(double value)
        {
            if (!(smoothFactor > 0.0))
            {
                return value;
            }

            lastValue = smoothFactor * lastValue + (1 - smoothFactor) * value;

            return lastValue;
        }
    }
}
