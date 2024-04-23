namespace ARLocation
{

    public struct HeadingReading
    {
        public double heading;
        public double magneticHeading;
        public double accuracy;
        public long timestamp;
        public bool isMagneticHeadingAvailable;

        public override string ToString()
        {
            return
                "HeadingReading {\n" +
                "  heading = " + heading + "\n" +
                "  magneticHeading = " + magneticHeading + "\n" +
                "  accuracy = " + accuracy + "\n" +
                "  timestamp = " + timestamp + "\n" +
                "  isMagneticHeadingAvailable = " + isMagneticHeadingAvailable + "\n" +
                "}";
        }
    }
}
