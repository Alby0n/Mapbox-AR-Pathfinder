using System.Collections;
using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace ARLocation
{
    /// <summary>
    /// Abstract location provider. All concrete location provider implementations
    /// should derive from this.
    /// </summary>
    public abstract class AbstractLocationProvider : ILocationProvider
    {
        protected double LowPassFilterFactor;

        /// <summary>
        /// The name of the location provider.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// The options of the location provider.
        /// </summary>
        /// <value>The options.</value>
        public LocationProviderOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the current location.
        /// </summary>
        /// <value>The current location.</value>
        public LocationReading CurrentLocation { get; protected set; }

        /// <summary>
        /// Gets or sets the previous location.
        /// </summary>
        /// <value>The last location.</value>
        public LocationReading LastLocation { get; protected set; }

        public LocationReading LastLocationRaw { get; protected set; }

        /// <summary>
        /// Gets or sets the previous raw location reading.
        /// </summary>
        /// <value>The raw location last.</value>
        public LocationReading CurrentLocationRaw { get; protected set; }

        /// <summary>
        /// The current heading reading.
        /// </summary>
        /// <value>The current heading.</value>
        public HeadingReading CurrentHeading { get; protected set; }

        /// <summary>
        /// The previous heading reading.
        /// </summary>
        /// <value>The last heading.</value>
        public HeadingReading LastHeading { get; protected set; }

        /// <summary>
        ///  The start point, i.e., the first measured location.
        /// </summary>
        /// <value>The start point.</value>
        public LocationReading FirstLocation { get; protected set; }

        /// <summary>
        /// Gets or sets the current status of the location provider.
        /// </summary>
        /// <value>The status.</value>
        public LocationProviderStatus Status { get; protected set; }

        /// <summary>
        /// If true, the location provider is enablied and getting regular location
        /// updated from the device.
        /// </summary>
        /// <value><c>true</c> if is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled { get; protected set; }

        /// <summary>
        /// If true, the first reading has not occured yet.
        /// </summary>
        /// <value><c>true</c> if first reading; otherwise, <c>false</c>.</value>
        public bool FirstReading { get; protected set; }

        /// <summary>
        /// If true, the provider has a functioning magnetic compass sensor.
        /// </summary>
        /// <value><c>true</c> if is compass enabled; otherwise, <c>false</c>.</value>
        public abstract bool IsCompassEnabled { get; }

        /// <summary>
        /// The start time of the location provider.
        /// </summary>
        /// <value>The start time.</value>
        public float StartTime { get; protected set; }

        /// <summary>
        /// If true, location updates are paused.
        /// </summary>
        public bool Paused { get; protected set; }

        public int LocationUpdateCount { get; protected set; }
        public bool HasStarted => Status == LocationProviderStatus.Started;
        public bool ApplyCompassTiltCompensationOnAndroid { get; set; } = true;

        public double DistanceFromStartPoint
        {
            get { return LocationReading.HorizontalDistance(FirstLocation, CurrentLocation); }
        }

        /// <summary>
        /// Event for when a new location data is received.
        /// </summary>
        public event LocationUpdatedDelegate LocationUpdated;

        /// <summary>
        /// Event for when a new compass data is received.
        /// </summary>
        public event CompassUpdateDelegate CompassUpdated;

        public event LocationEnabledDelegate LocationEnabled;
        public event LocationFailedDelegate LocationFailed;
        public event LocationUpdatedDelegate LocationUpdatedRaw;
        /// <summary>
        /// Reads the location from the device; should be implemented by each
        /// provider.
        /// </summary>
        /// <returns>The location.</returns>
        protected abstract LocationReading? ReadLocation();

        /// <summary>
        /// Reads the heading from the device; should be implemented by each
        /// provider.
        /// </summary>
        /// <returns>The heading.</returns>
        protected abstract HeadingReading? ReadHeading();

        /// <summary>
        /// Requests the location and compass updates from the device; should be implemented by each
        /// provider.
        /// </summary>
        protected abstract void RequestLocationAndCompassUpdates();

        /// <summary>
        /// Updates the location service status from the device; should be implemented by each
        /// provider.
        /// </summary>
        protected abstract void UpdateLocationRequestStatus();

        protected AbstractLocationProvider()
        {
            IsEnabled = false;
            FirstReading = true;
            Paused = false;
            Status = LocationProviderStatus.Idle;
        }

        public virtual IEnumerator Start(uint maxWaitTime = 10000, uint delay = 0)
        {
            // Debug.Log("[AbstractLocationProvider]: Starting...");

#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }

            yield return new WaitForSeconds(1);
#endif

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            RequestLocationAndCompassUpdates();
            uint maxWait = maxWaitTime;
            UpdateLocationRequestStatus();
            while (Status == LocationProviderStatus.Initializing && maxWait > 0)
            {
                // Debug.Log("[AbstractLocationProvider]: Wait... " + maxWait);

                yield return new WaitForSeconds(1);
                maxWait--;
                UpdateLocationRequestStatus();
            }

            if (maxWait < 1)
            {
                // Debug.LogError("[AbstractLocationProvider]: Timed out.");

                LocationFailed?.Invoke("Timed out");

                yield break;
            }

            if (Status == LocationProviderStatus.Failed)
            {
                // Debug.LogError("[AbstractLocationProvider]: Falied to initialize location updates.");

                LocationFailed?.Invoke("Falied to initialize location updates.");

                yield break;
            }

            if (Status != LocationProviderStatus.Started)
            {
                // Debug.LogError("[AbstractLocationProvider]: Unknown error initializing location updates. " + Status);

                LocationFailed?.Invoke("Unknown error initializing location updates.");

                yield break;
            }

            // Debug.Log("[AbstractLocationProvider]: Started!");

            FirstReading = true;
            StartTime = Time.time;
        }

        public void ForceLocationUpdate()
        {
            LocationUpdated?.Invoke(CurrentLocation, LastLocation);
            LocationUpdatedRaw?.Invoke(CurrentLocationRaw, LastLocationRaw);
        }

        protected virtual void InnerOnEnabled()
        {

        }


        protected void EmitLocationUpdated()
        {
            LocationUpdated?.Invoke(CurrentLocation, LastLocation);
        }

        protected void EmitLocationUpdatedRaw()
        {
            LocationUpdatedRaw?.Invoke(CurrentLocationRaw, LastLocationRaw);
        }

        protected void EmitCompassUpdated()
        {
            CompassUpdated?.Invoke(CurrentHeading, LastHeading);
        }

        protected void UpdateLocation(LocationReading newLocation)
        {
            if (newLocation.timestamp == CurrentLocationRaw.timestamp)
            {
                return;
            }

            LastLocationRaw = CurrentLocationRaw;
            CurrentLocationRaw = newLocation;

            EmitLocationUpdatedRaw();

            if (!ShouldUpdateLocation(newLocation))
            {
                return;
            }

            LastLocation = CurrentLocation;
            CurrentLocation = newLocation;

            LocationUpdateCount++;

            EmitLocationUpdated();
        }

        protected void UpdateHeading(HeadingReading newHeading)
        {
            if (!ShouldUpdateHeading(newHeading))
            {
                return;
            }

            LastHeading = CurrentHeading;

            CurrentHeading = newHeading;

            EmitCompassUpdated();
        }

        protected bool ShouldUpdateHeading(HeadingReading newHeading)
        {
            if (newHeading.timestamp == CurrentHeading.timestamp)
            {
                return false;
            }

            return true;
        }

        protected bool ShouldUpdateLocation(LocationReading newLocation)
        {
            if (Paused)
            {
                return false;
            }

            if (newLocation.timestamp - CurrentLocation.timestamp < ((long) (Options.TimeBetweenUpdates * 1000)))
            {
                return false;
            }

            if (LocationReading.HorizontalDistance(newLocation, CurrentLocation) < Options.MinDistanceBetweenUpdates)
            {
                return false;
            }

            if ((newLocation.accuracy > Options.AccuracyRadius) && (Options.AccuracyRadius > 0))
            {
                return false;
            }


            return true;
        }

        public virtual void Update()
        {
            if (!HasStarted)
            {
                return;
            }

            var location = ReadLocation();
            var heading = ReadHeading();

            if (location == null || heading == null)
            {
                // Debug.Log("[AbstractLocationProvider]: Null reading");
                return;
            }

            if (FirstReading)
            {
                FirstLocation = location.Value;
                CurrentLocation = FirstLocation;
                CurrentLocationRaw = FirstLocation;
                CurrentHeading = heading.Value;

                IsEnabled = true;
                FirstReading = false;

                LocationEnabled?.Invoke();
                InnerOnEnabled();

                EmitCompassUpdated();
                EmitLocationUpdated();
                EmitLocationUpdatedRaw();

                return;
            }

            UpdateLocation(location.Value);
            UpdateHeading(heading.Value);
        }

        public void Restart()
        {
            LocationUpdateCount = 0;
            FirstReading = true;
        }

        public void ResetStartPoint()
        {
            FirstLocation = CurrentLocation;
        }

        public void SetCompassLowPassFactor(double factor)
        {
            LowPassFilterFactor = factor;
        }

        public string GetStatusString()
        {
            switch (Status)
            {
                case LocationProviderStatus.Idle:
                    return "Idle";
                case LocationProviderStatus.Failed:
                    return "Failed";
                case LocationProviderStatus.Initializing:
                    return "Initializing";
                case LocationProviderStatus.Started:
                    return "Started";
            }

            return "UnknownStatus";
        }

        public string GetInfoString()
        {
            return Name +
                "{ \n" +
                CurrentLocation + "\n" +
                CurrentHeading + "\n" +
                "Status = " + GetStatusString() + "\n" +
                "DistanceFromStartPoint = " + DistanceFromStartPoint + "\n" +
                "TimeSinceStart = " + (Time.time - StartTime) + "\n" +
                "}";
        }

        public void OnEnabled(LocationEnabledDelegate del)
        {
            LocationEnabled += del;

            if (IsEnabled)
            {
                del();
            }
        }

        public void OnFail(LocationFailedDelegate del)
        {
            LocationFailed += del;
        }

        /// <summary>
        /// Pauses location updates
        /// </summary>
        public void Pause()
        {
            Paused = true;
        }

        /// <summary>
        /// Resumes location updates
        /// </summary>
        public void Resume()
        {
            Paused = false;
        }
    }
}
