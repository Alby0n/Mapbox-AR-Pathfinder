// ReSharper disable ParameterHidesMember
namespace ARLocation.Utils
{
    public class MovingAveragePosition
    {
        public DVector3 CalculateAveragePosition()
        {
            return mPosition;
        }

        public double aMin = 2.0;
        public double aMax = 10.0;
        public double cutoff = 0.01;
        public double alpha = 0.25;

        private DVector3 mPosition;
        private bool first = true;

        private double Weight(double a, double aMin, double aMax, double cutoff = 0.01)
        {
            if (a <= aMin)
            {
                return 1.0;
            }

            if (a >= aMax)
            {
                return 0.0;
            }

            var lambda = System.Math.Log(1 / cutoff) / (aMax - aMin);

            return System.Math.Exp(-lambda * (a - aMin));
        }

        public void AddEntry(DVector3 position, double accuracy)
        {
            if (first)
            {
                mPosition = position;
                first = false;
            }
            else
            {
                var b = Weight(accuracy, aMin, aMax, cutoff);
                var a = alpha * b;

                mPosition = a * position + (1 - a) * mPosition;
            }
        }

        public void Rest()
        {
            first = true;
            mPosition = new DVector3();
        }
    }
}
