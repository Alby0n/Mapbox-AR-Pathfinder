using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
// ReSharper disable UnusedMember.Global


namespace ARLocation
{
    using Utils;

    [AddComponentMenu("AR+GPS/AR Location Provider")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#arlocationprovider")]
    [DisallowMultipleComponent]
    public class ARLocationProvider : Singleton<ARLocationProvider>
    {
        [Serializable]
        public class LocationEnabledUnityEvent : UnityEvent<Location> {}
        [Serializable]
        public class LocationUpdatedUnityEvent : UnityEvent<Location> {}

        [Serializable]
        public class CompassUpdatedUnityEvent: UnityEvent<HeadingReading> {}

        [FormerlySerializedAs("LocationUpdateSettings")]
        [Tooltip("The options for the Location Provider.")]
        [Header("Update Settings")]
        public LocationProviderOptions LocationProviderSettings = new LocationProviderOptions();

        [Tooltip("The data of mock location. If present, overrides the Mock Location above.")]
        [Header("Mock Data")]
        public LocationData MockLocationData;

        [Tooltip("The maximum wait time to wait for location initialization.")]
        [Header("Initialization")]
        public uint MaxWaitTime = 200;

        [Tooltip("Wait this many seconds before starting location services. Useful when using Unity Remote.")]
        public uint StartUpDelay;

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'ARLocationProvider' in the log output to see the messages.")]
        public bool DebugMode;

        [Header("Events")]

        [Tooltip("Called after the first location is read.")]
        public LocationEnabledUnityEvent OnEnabled = new LocationEnabledUnityEvent();

        [Tooltip("Called after each new location update.")]
        public LocationUpdatedUnityEvent OnLocationUpdated = new LocationUpdatedUnityEvent();

        [Tooltip("Called after each new raw device GPS data is obtained.")]
        public LocationUpdatedUnityEvent OnRawLocationUpdated = new LocationUpdatedUnityEvent();

        [Tooltip("Called after each new compass update.")]
        public CompassUpdatedUnityEvent OnCompassUpdated = new CompassUpdatedUnityEvent();

        /// <summary>
        /// Returns the current location provider.
        /// </summary>
        public ILocationProvider Provider { get; private set; }

        /// <summary>
        /// If true, the location provider has received the first location data.
        /// </summary>
        public bool IsEnabled => Provider.IsEnabled;

        /// <summary>
        /// If true, the location provider has started, but no location data has been read.
        /// </summary>
        public bool HasStarted => Provider.HasStarted;

        /// <summary>
        /// The number of location updates so far.
        /// </summary>
        public int LocationUpdateCount => Provider.LocationUpdateCount;

        /// <summary>
        /// If true, updates are paused.
        /// </summary>
        public bool IsPaused => Provider.Paused;

        /// <summary>
        /// The latest location data.
        /// </summary>
        public LocationReading CurrentLocation => Provider.CurrentLocation;

        /// <summary>
        /// The previous location data.
        /// </summary>
        public LocationReading LastLocation => Provider.LastLocation;

        /// <summary>
        /// The current heading data.
        /// </summary>
        public HeadingReading CurrentHeading => Provider.CurrentHeading;


        /// <summary>
        /// Time since the location provider has started.
        /// </summary>
        public float TimeSinceStart => Time.time - Provider.StartTime;

        /// <summary>
        /// The distance from the initial measured position.
        /// </summary>
        public double DistanceFromStartPoint => Provider.DistanceFromStartPoint;

        private int measurementCount;
        private bool mute;

        public event LocationUpdatedDelegate OnLocationUpdatedDelegate;
        public event CompassUpdateDelegate OnCompassUpdateDelegate;
        public event Action OnRestartDelegate;

        public override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            Provider = new MockLocationProvider();

            if (MockLocationData != null)
            {
                Logger.LogFromMethod("ARLocationProvider", "Awake", $"Using mock location {MockLocationData}", DebugMode);
                ((MockLocationProvider) Provider).mockLocation = MockLocationData.Location;
            }
#elif ARGPS_CUSTOM_PROVIDER
        // If you want to use a custom location provider, add 'ARGPS_CUSTOM_PROVIDER' to the define symbols in the Player
        // settings, create a implementation of ILocationProvider, and instantiate it in the line below.
        Provider = new ARGpsCustomLocationProvider();
#else
        Provider = new UnityLocationProvider();
#endif
            Logger.LogFromMethod("ARLocationProvider", "Awake",": Using provider " + Provider.Name, DebugMode);
        }



        private void InitProviderEventListeners()
        {
            Logger.LogFromMethod("ARLocationProvider", "InitProviderEventListeners","Initializing location provider listeners.", DebugMode);

            Provider.LocationUpdated += Provider_LocationUpdated;
            Provider.CompassUpdated += Provider_CompassUpdated;
            Provider.LocationUpdatedRaw += ProviderOnLocationUpdatedRaw;

            Provider.OnEnabled(OnProviderEnabledDelegate);

            if (Provider.IsEnabled)
            {
                ForceLocationUpdate();
            }
        }

        private void ProviderOnLocationUpdatedRaw(LocationReading currentLocation, LocationReading lastLocation)
        {
            OnRawLocationUpdated?.Invoke(currentLocation.ToLocation());
        }

        private void OnProviderEnabledDelegate()
        {
            Logger.LogFromMethod("ARLocationProvider", "OnProviderEnabledDelegate","Provider enabled; emitting 'OnEnabled' event.", DebugMode);
            OnEnabled?.Invoke(CurrentLocation.ToLocation());
        }

        IEnumerator Start()
        {
            InitProviderEventListeners();

            Provider.Options = LocationProviderSettings;

            Logger.LogFromMethod("ARLocationProvider", "Start","Starting the location provider", DebugMode);
            yield return StartCoroutine(Provider.Start(MaxWaitTime, StartUpDelay));
        }

        public void Mute()
        {
            Logger.LogFromMethod("ARLocationProvider", "Mute","Muting ARLocationProvider.", DebugMode);
            mute = true;
        }

        public void Unmute(bool emit = true)
        {
            Logger.LogFromMethod("ARLocationProvider", "Mute","Un-muting ARLocationProvider.", DebugMode);
            mute = false;
            if (Provider.IsEnabled && emit) ForceLocationUpdate();
        }

        private void Provider_CompassUpdated(HeadingReading heading, HeadingReading lastReading)
        {
            if (mute) return;

            OnCompassUpdateDelegate?.Invoke(heading, lastReading);
            OnCompassUpdated?.Invoke(heading);
        }

        private void Provider_LocationUpdated(LocationReading currentLocation, LocationReading lastLocation)
        {
            if (mute) return;

            measurementCount++;

            if ((LocationProviderSettings.MaxNumberOfUpdates > 0) && (measurementCount >= LocationProviderSettings.MaxNumberOfUpdates))
            {
                Provider.Pause();
            }

            Logger.LogFromMethod("ARLocationProvider", "Provider_LocationUpdated",$"New location {currentLocation}.", DebugMode);

            OnLocationUpdatedDelegate?.Invoke(currentLocation, lastLocation);
            OnLocationUpdated?.Invoke(currentLocation.ToLocation());
        }

        /// <summary>
        /// Force the provider to emit a location update event. This wont force a new read of location, just emit
        /// the last available measurement.
        /// </summary>
        public void ForceLocationUpdate()
        {
            Logger.LogFromMethod("ARLocationProvider", "ForceLocationUpdate","Emitting a forced location update", DebugMode);
            Provider.ForceLocationUpdate();
        }

        void Update()
        {
            if (Provider == null || !Provider.HasStarted)
            {
                return;
            }

            Provider.Update();
        }

        /// <summary>
        /// Pauses location updates
        /// </summary>
        public void Pause()
        {
            Logger.LogFromMethod("ARLocationProvider", "Pause","Pausing the location provider.", DebugMode);
            Provider?.Pause();
        }

        /// <summary>
        /// Resumes location updates
        /// </summary>
        public void Resume()
        {
            Logger.LogFromMethod("ARLocationProvider", "Resume","Resuming the location provider.", DebugMode);
            Provider?.Resume();
        }

        /// <summary>
        /// Resets the location provider.
        /// </summary>
        public void Restart()
        {
            Logger.LogFromMethod("ARLocationProvider", "Restart","Restarting the location provider.", DebugMode);
            Provider?.Restart();
            OnRestartDelegate?.Invoke();
        }

        /// <summary>
        /// Register a delegate to location updates.
        ///
        /// The `useRawIfEnabled` method if for situations where we want the latest data,
        /// like when we are adding objects at runtime.
        ///
        /// </summary>
        /// <param name="locationUpdatedDelegate"></param>
        /// <param name="useRawIfEnabled"></param>
        public void OnLocationUpdatedEvent(LocationUpdatedDelegate locationUpdatedDelegate, bool useRawIfEnabled = false)
        {
            if (IsEnabled)
            {
                locationUpdatedDelegate(CurrentLocation, useRawIfEnabled ? Provider.LastLocationRaw : LastLocation);
            }

            OnLocationUpdatedDelegate += locationUpdatedDelegate;
        }

        public void OnProviderRestartEvent(Action del)
        {
            OnRestartDelegate += del;
        }

        /// <summary>
        /// Register a delegate to compass/heading updates.
        /// </summary>
        /// <param name="compassUpdateDelegate"></param>
        public void OnCompassUpdatedEvent(CompassUpdateDelegate compassUpdateDelegate)
        {
            OnCompassUpdateDelegate += compassUpdateDelegate;
        }

        /// <summary>
        /// RegisterRegister delegate for when the provider enables location updates.
        /// </summary>
        /// <param name="del">Del.</param>
        public void OnEnabledEvent(LocationEnabledDelegate del)
        {
            Provider.OnEnabled(del);
        }

        /// <summary>
        /// Register a delegate for when the provider fails to initialize location services.
        /// </summary>
        /// <param name="del">Del.</param>
        public void OnFailedEvent(LocationFailedDelegate del)
        {
            Provider.OnFail(del);
        }
    }
}
