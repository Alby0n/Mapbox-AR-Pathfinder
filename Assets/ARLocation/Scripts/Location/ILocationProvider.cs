using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation
{
    [Serializable]
    public class LocationProviderOptions
    {
        /// <summary>
        /// The minimum desired update time, in seconds.
        /// </summary>
        [Tooltip("The minimum desired update time, in seconds.")]
        public float TimeBetweenUpdates = 2.0f;

        /// <summary>
        /// The minimum distance between consecutive location updates, in meters.
        /// </summary>
        [Tooltip("The minimum distance between consecutive location updates, in meters.")]
        public double MinDistanceBetweenUpdates = 0;

        /// <summary>
        /// The minimum accuracy of accepted location measurements, in meters.
        /// </summary>
        [FormerlySerializedAs("MaxAccuracyRadius")]
        [Tooltip("The minimum accuracy of accepted location measurements, in meters. " +
            "Accuracy here means the radius of uncertainty of the device's location, " +
            "defining a circle where it can possibly be found in.")]
        public double AccuracyRadius = 25.0f;

        [Tooltip("The global maximum number of location updates. The updates will be paused after this amount. Zero means there is no limit and " +
            "the updates won't be paused automatically. Note that this will possibly override the settings from individual components, like 'PlaceAtLocation'.")]
        public uint MaxNumberOfUpdates;
    }

    public enum LocationProviderStatus
    {
        Idle,
        Initializing,
        Started,
        Failed
    }

    // Location provider delegates/events
    public delegate void LocationUpdatedDelegate(LocationReading currentLocation, LocationReading lastLocation);
    public delegate void CompassUpdateDelegate(HeadingReading heading, HeadingReading lastReading);
    public delegate void LocationEnabledDelegate();
    public delegate void LocationFailedDelegate(string message);

    public interface ILocationProvider
    {
        string Name { get; }

        LocationProviderOptions Options { get; set; }

        LocationReading CurrentLocation { get; }
        LocationReading CurrentLocationRaw { get; }
        LocationReading LastLocation { get; }
        LocationReading LastLocationRaw { get; }
        LocationReading FirstLocation { get; }

        HeadingReading CurrentHeading { get; }
        HeadingReading LastHeading { get; }

        float StartTime { get; }
        bool IsCompassEnabled { get; }
        double DistanceFromStartPoint { get; }
        bool IsEnabled { get; }
        bool Paused { get; }
        int LocationUpdateCount { get; }

        bool HasStarted { get; }

        bool ApplyCompassTiltCompensationOnAndroid { get; set; }

        event LocationUpdatedDelegate LocationUpdated;
        event LocationUpdatedDelegate LocationUpdatedRaw;
        event CompassUpdateDelegate CompassUpdated;
        event LocationEnabledDelegate LocationEnabled;
        event LocationFailedDelegate LocationFailed;

        IEnumerator Start(uint maxWaitTime = 10000, uint delay = 0);

        void ForceLocationUpdate();

        void Pause();
        void Resume();
        void Update();

        void Restart();

        void OnEnabled(LocationEnabledDelegate del);
        void OnFail(LocationFailedDelegate del);

        void SetCompassLowPassFactor(double factor);

        string GetInfoString();
        string GetStatusString();
    }
}
