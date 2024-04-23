// Copyright (C) 2018 Daniel Fortes <daniel.fortes@gmail.com>
// All rights reserved.
//
// See LICENSE.TXT for more info

using System;
using ARLocation.Session;
using UnityEngine;
using UnityEngine.Events;

#if !ARGPS_USE_VUFORIA
using UnityEngine.XR.ARFoundation;
#endif

namespace ARLocation
{
    using Utils;

    /// <summary>
    /// This Component manages all positioned GameObjects, synchronizing their world position in the scene
    /// with their geographical coordinates. This is done by calculating their position relative to the device's position.
    ///
    /// Should be placed in a GameObject called "ARLocationRoot", whose parent is the "AR Session Origin".
    /// </summary>
    [RequireComponent(typeof(ARLocationOrientation))]
    [DisallowMultipleComponent]
    [AddComponentMenu("AR+GPS/AR Location Manager")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#arlocationmanager")]
    public class ARLocationManager : Singleton<ARLocationManager>
    {
        [Tooltip("The AR Camera that will be used for rendering the AR content. If this is not set, the camera tagger as 'MainCamera' will be used." +
                 " Make sure either this is set, or a camera is tagged as 'MainCamera', or an error will be thrown.")]
        public Camera Camera;

        [Tooltip("If true, wait until the AR Tracking starts to start with location and orientation updates and object placement.")]
        public bool WaitForARTrackingToStart = true;

        [Tooltip("If true, every time the AR tracking is lost and regained, the AR+GPS system is restarted, repositioning all the objects.")]
        public bool RestartWhenARTrackingIsRestored;

        [Tooltip("If true, the manager will set 'Application.targetFrameRate' to 60.")]
        public bool SetTargetFrameRateTo60Mhz = true;

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'ARLocationManager' in the log output to see the messages.")]
        public bool DebugMode;

        [Header("AR Session Events")]
        public UnityEvent OnTrackingStarted = new UnityEvent();
        public UnityEvent OnTrackingLost = new UnityEvent();
        public UnityEvent OnTrackingRestored = new UnityEvent();

        /// <summary>
        /// The instance of the 'IARSessionManager'. Handles the interface with the underlying AR session (i.e., Vuforia or AR Foundation).
        /// </summary>
        public IARSessionManager SessionManager { get; private set; }

        /// <summary>
        /// The 'MainCamera' that is being used for rendering the AR content.
        /// </summary>
        public Camera MainCamera { get; private set; }

        /// <summary>
        /// Returns the Y world-coordinate of the detected plane which is nearest to the user/camera.
        /// </summary>
        public float CurrentGroundY => groundHeight ? groundHeight.CurrentGroundY : -ARLocation.Config.InitialGroundHeightGuess;

        private ARLocationOrientation arLocationOrientation;
        private ARLocationProvider arLocationProvider;
        private Action onARTrackingStartedAction;
        private GameObject groundHeightDummy;
        private GroundHeight groundHeight;
        private Vector3 cameraPositionAtLastUpdate;



        public override void Awake()
        {
            base.Awake();

            if (SetTargetFrameRateTo60Mhz)
            {
                Logger.LogFromMethod("ARLocationManager", "Awake", "Setting 'Application.targetFrameRate' to 60", DebugMode);
                Application.targetFrameRate = 60;
            }

            MainCamera = Camera ? Camera : Camera.main;

            if (MainCamera == null)
            {
                throw new NullReferenceException("[AR+GPS][ARLocationManager#Start]: Missing Camera. " +
                                                 "Either set the 'Camera' property to the AR Camera, or tag it as a 'MainCamera'.");
            }

            transform.localPosition = new Vector3(0, 0, 0);
        }

        private void Start()
        {
            arLocationOrientation = GetComponent<ARLocationOrientation>();
            arLocationProvider = ARLocationProvider.Instance;

            if (WaitForARTrackingToStart)
            {
                arLocationProvider.Mute();
            }

#if !ARGPS_USE_VUFORIA
            var arSession = FindObjectOfType<ARSession>();

            if (!arSession)
            {
                throw new NullReferenceException("[AR+GPS][ARLocationManager#Start]: No ARSession found in the scene!");
            }

            SessionManager = new ARFoundationSessionManager(arSession);
#else
            SessionManager = new VuforiaSessionManager();
#endif

            SessionManager.DebugMode = DebugMode;

            SessionManager.OnARTrackingStarted(ARTrackingStartedCallback);
            SessionManager.OnARTrackingRestored(ARTrackingRestoredCallback);
            SessionManager.OnARTrackingLost(ARTrackingLostCallback);

            groundHeightDummy = new GameObject("ARLocationGroundHeight");
            groundHeightDummy.transform.SetParent(MainCamera.transform);
            groundHeightDummy.transform.localPosition = new Vector3();
            groundHeight = groundHeightDummy.AddComponent<GroundHeight>();

            arLocationProvider.OnLocationUpdatedEvent(onLocationUpdated);
        }

        private void onLocationUpdated(LocationReading currentLocation, LocationReading lastLocation)
        {
            cameraPositionAtLastUpdate = MainCamera.transform.position;
        }

        private void ARTrackingLostCallback()
        {
            Logger.LogFromMethod("ARLocationManager", "ARTrackingLostCallback", "'ARTrackingLost' event received.", DebugMode);
            OnTrackingLost?.Invoke();
        }

        private void ARTrackingRestoredCallback()
        {
            Logger.LogFromMethod("ARLocationManager", "ARTrackingRestoredCallback", "'ARTrackingRestore' event received.", DebugMode);

            if (RestartWhenARTrackingIsRestored)
            {
                Logger.LogFromMethod("ARLocationManager", "ARTrackingRestoredCallback", "'RestartWhenARTrackingIsRestored' is enabled; restarting.", DebugMode);
                Restart();
            }

            OnTrackingRestored?.Invoke();
        }

        private void ARTrackingStartedCallback()
        {
            Logger.LogFromMethod("ARLocationManager", "ARTrackingStartedCallback", "'OnARTrackingStarted' event received.", DebugMode);

            if (WaitForARTrackingToStart)
            {
                arLocationProvider.Unmute();
            }

            OnTrackingStarted?.Invoke();
            onARTrackingStartedAction?.Invoke();
        }

        /// <summary>
        /// This will reset the AR Session and the AR+GPS system, repositioning all objects.
        /// </summary>
        /// <param name="cb">Optional callback, called when the system has restarted.</param>
        public void ResetARSession(Action cb = null)
        {
            Logger.LogFromMethod("ARLocationManager", "ResetARSession", "Resetting the AR Session...", DebugMode);

            SessionManager?.Reset(() =>
            {
                Logger.LogFromMethod("ARLocationManager", "ResetARSession", "ARSession restarted. Resetting AR+GPS location...", DebugMode);
                Restart();
                cb?.Invoke();
            });
        }

        /// <summary>
        /// This will restart the AR+GPS system, repositioning all the objects.
        /// </summary>
        public void Restart()
        {
            Logger.LogFromMethod("ARLocationManager", "Restart", "Resetting AR+GPS location...", DebugMode);

            arLocationOrientation.Restart();
            arLocationProvider.Restart();

            Logger.LogFromMethod("ARLocationManager", "Restart", "Done.", DebugMode);
        }

        /// <summary>
        /// Returns a string describing the current AR session tracking status
        /// </summary>
        /// <returns></returns>
        public string GetARSessionInfoString()
        {
            return SessionManager != null ? SessionManager.GetSessionInfoString() : "None";
        }

        /// <summary>
        /// Returns a string describing the current AR Session provider, e.g., AR Foundation or Vuforia.
        /// </summary>
        /// <returns></returns>
        public string GetARSessionProviderString()
        {
            return SessionManager != null ? SessionManager.GetProviderString() : "None";
        }

        /// <summary>
        /// Add a event listener for when the AR Tracking starts.
        /// </summary>
        /// <param name="o"></param>
        public void OnARTrackingStarted(Action o)
        {
            if (SessionManager == null)
            {
                onARTrackingStartedAction += o;
            }
            else
            {
                SessionManager.OnARTrackingStarted(o);
            }
        }

        /// <summary>
        /// Add a event listener for when the AR Tracking regained after it was lost.
        /// </summary>
        /// <param name="callback"></param>
        public void OnARTrackingRestored(Action callback)
        {
            SessionManager?.OnARTrackingRestored(callback);
        }

        /// <summary>
        /// Add a event listener for when the AR Tracking is lost.
        /// </summary>
        /// <param name="callback"></param>
        public void OnARTrackingLost(Action callback)
        {
            SessionManager?.OnARTrackingLost(callback);
        }

        /// <summary>
        /// Given a world position vector, return the corresponding geographical Location.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Location GetLocationForWorldPosition(Vector3 position)
        {
            return Location.GetLocationForWorldPosition(gameObject.transform, cameraPositionAtLastUpdate, arLocationProvider.CurrentLocation.ToLocation(), position);
        }

        /// <summary>
        /// Returns the worl-position for a given geographical location.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 GetWorldPositionForLocation(Location location, bool heightRelative = true)
        {
            return Location.GetGameObjectPositionForLocation(gameObject.transform, cameraPositionAtLastUpdate, arLocationProvider.CurrentLocation.ToLocation(), location, heightRelative);
        }
    }
}
