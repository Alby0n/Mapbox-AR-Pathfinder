using UnityEngine;

namespace ARLocation
{

    public class MockLocationProvider : AbstractLocationProvider
    {
        public override string Name => "MockLocationProvider";
        public override bool IsCompassEnabled => true;
        public Location mockLocation = new Location();

        private bool requested = true;
        private bool first = true;

        protected override HeadingReading? ReadHeading()
        {
            var mainCamera = Camera.main;

            if (mainCamera != null)
            {

                var transform = mainCamera.transform;

                var localEulerAngles = transform.localEulerAngles;
                return new HeadingReading
                {
                    heading = localEulerAngles.y,
                    magneticHeading = localEulerAngles.y,
                    accuracy = 0,
                    isMagneticHeadingAvailable = true,
                    timestamp = (long)(Time.time * 1000)
                };
            }
            else
            {
                return new HeadingReading
                {
                    heading = 0,
                    magneticHeading = 0,
                    accuracy = 0,
                    isMagneticHeadingAvailable = true,
                    timestamp = (long)(Time.time * 1000)
                };
            }
        }

        protected override LocationReading? ReadLocation()
        {
            if (first)
            {
                first = false;
                return new LocationReading
                {
                    latitude = mockLocation.Latitude,
                    longitude = mockLocation.Longitude,
                    altitude = mockLocation.Altitude,
                    accuracy = 0.0,
                    floor = -1,
                    timestamp = (long)(Time.time * 1000)
                };
            }

            return null;
        }


        protected override void RequestLocationAndCompassUpdates()
        {
            requested = true;
        }

        protected override void UpdateLocationRequestStatus()
        {
            if (requested)
            {
                Status = LocationProviderStatus.Initializing;
                requested = false;
            }

            if (Status == LocationProviderStatus.Initializing)
            {
                Status = LocationProviderStatus.Started;
            }
        }
    }
}
