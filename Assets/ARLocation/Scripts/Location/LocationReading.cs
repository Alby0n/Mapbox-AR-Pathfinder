namespace ARLocation
{

    public struct LocationReading
    {
        public double latitude;
        public double longitude;
        public double altitude;
        public double accuracy;
        public int floor;

        /// <summary>
        /// Epoch time in ms
        /// </summary>
        public long timestamp;

        public Location ToLocation()
        {
            return new Location(latitude, longitude, altitude);
        }

        public static double HorizontalDistance(LocationReading a, LocationReading b)
        {
            return Location.HorizontalDistance(a.ToLocation(), b.ToLocation());
        }

        public override string ToString()
        {
            return
                "LocationReading { \n" +
                "  latitude = " + latitude + "\n" +
                "  longitude = " + longitude + "\n" +
                "  altitude = " + altitude + "\n" +
                "  accuracy = " + accuracy + "\n" +
                "  floor = " + floor + "\n" +
                "  timestamp = " + timestamp + "\n" +
                "}";
        }
    }
}
