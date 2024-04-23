using UnityEngine;

namespace ARLocation
{

    public class UnityLocationProvider : AbstractLocationProvider
    {
        private float androidMagneticDeclination;
        private AndroidNativeCompass androidNativeCompass;
        public override string Name => "UnityLocationProvider";

        public override bool IsCompassEnabled => Input.compass.enabled;

        protected override void RequestLocationAndCompassUpdates()
        {
            // Debug.Log("[UnityLocationProvider]: Requesting location updates...");

            Input.compass.enabled = true;

            Input.location.Start(
                (float)Options.AccuracyRadius,
                (float)Options.MinDistanceBetweenUpdates
            );
        }

        protected override void InnerOnEnabled()
        {
            androidMagneticDeclination = AndroidMagneticDeclination.GetDeclination(CurrentLocation.ToLocation());
            androidNativeCompass = new AndroidNativeCompass((float) (1.0  - LowPassFilterFactor));
        }

        protected override void UpdateLocationRequestStatus()
        {
            switch (Input.location.status)
            {
                case LocationServiceStatus.Initializing:
                    Status = LocationProviderStatus.Initializing;
                    break;

                case LocationServiceStatus.Failed:
                    Status = LocationProviderStatus.Failed;
                    break;

                case LocationServiceStatus.Running:
                    Status = LocationProviderStatus.Started;
                    break;

                case LocationServiceStatus.Stopped:
                    Status = LocationProviderStatus.Idle;
                    break;
            }
        }

        protected override LocationReading? ReadLocation()
        {
            if (!HasStarted)
            {
                return null;
            }

            var data = Input.location.lastData;

            return new LocationReading()
            {
                latitude = data.latitude,
                longitude = data.longitude,
                altitude = data.altitude,
                accuracy = data.horizontalAccuracy,
                floor = -1,
                timestamp = (long)(data.timestamp * 1000)
            };
        }

        protected override HeadingReading? ReadHeading()
        {
            if (!HasStarted)
            {
                return null;
            }

            // ReSharper disable once RedundantAssignment
            var magneticHeading = Input.compass.magneticHeading;

            // ReSharper disable once RedundantAssignment
            var trueHeading = Input.compass.trueHeading;

#if PLATFORM_ANDROID
            var tiltCorrectedMagneticHeading = GetMagneticHeading();
            magneticHeading = tiltCorrectedMagneticHeading;
            trueHeading = tiltCorrectedMagneticHeading + androidMagneticDeclination;
#endif

            if (trueHeading < 0)
            {
                trueHeading += 360;
            }

            return new HeadingReading()
            {
                heading = trueHeading,
                magneticHeading = magneticHeading,
                accuracy = Input.compass.headingAccuracy,
                timestamp = (long)(Input.compass.timestamp * 1000),
                isMagneticHeadingAvailable = Input.compass.enabled
            };
        }

        private float GetMagneticHeading()
        {
#if PLATFORM_ANDROID
            if (!SystemInfo.supportsGyroscope || !ApplyCompassTiltCompensationOnAndroid || androidNativeCompass == null)
            {
                return Input.compass.magneticHeading;
            }

            return androidNativeCompass.GetMagneticHeading();

//            if (Screen.orientation == ScreenOrientation.Landscape)
//            {
//                return heading;// + 45;
//            }
//            else
//            {
//                return heading;
//            }

#else
            return Input.compass.magneticHeading;
#endif
        }
    }
}
