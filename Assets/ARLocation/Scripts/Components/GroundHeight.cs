using System;
using UnityEngine;


#if !ARGPS_USE_VUFORIA
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

#pragma warning disable
using Logger = ARLocation.Utils.Logger;
#pragma warning enable

#if ARGPS_USE_VUFORIA
using Vuforia;
#endif

namespace ARLocation
{
    /// <summary>
    /// This component will change the Y component of the GameObject's position,
    /// so that it is set to the level of the nearest detected ground plane.
    /// </summary>
    [DisallowMultipleComponent]
    public class GroundHeight : MonoBehaviour
    {
        [Serializable]
        public class SettingsData
        {
            [Range(0, 10)]
            public float InitialGroundHeightGuess = 1.4f;
            [Range(0, 10)]
            public float MinGroundHeight = 0.4f;
            [Range(0, 10)]
            public float MaxGroundHeight = 3.0f;
            [Range(0, 1)]
            public float Smoothing = 0.05f;

            public float Altitude;

            public bool DisableUpdate;

            [Range(0, 0.1f)]
            public float Precision = 0.005f;
            public bool UseArLocationConfigSettings = true;

#if ARGPS_USE_VUFORIA
            public float MinHitDistance = 0.5f;
#endif
        }

        [Serializable]
        public class StateData
        {
            public float CurrentGroundY;
            public float CurrentPlaneDistance = -1.0f;
            public Vector3 CurrentPlaneCenter;
            public bool NeedsUpdate = true;
        }

        public float CurrentGroundY => state.CurrentGroundY;

        public SettingsData Settings = new SettingsData();
        private readonly StateData state = new StateData();
        private Camera mainCamera;

#if !ARGPS_USE_VUFORIA
        private ARPlaneManager arPlaneManager;
        private float targetY;

        void Start()
        {
            arPlaneManager = FindObjectOfType<ARPlaneManager>();
            var arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            mainCamera = ARLocationManager.Instance.MainCamera;

            if (arPlaneManager == null)
            {
                if (arSessionOrigin == null)
                {
                    Debug.LogWarning("[AR+GPS][GroundHeight#Start]: ARSessionOrigin not present in the scene!");
                    return;
                }

                arPlaneManager = arSessionOrigin.gameObject.AddComponent<ARPlaneManager>();
                Utils.Misc.RequestPlaneDetectionMode(arPlaneManager, PlaneDetectionMode.Horizontal);
            }

            if (Settings.UseArLocationConfigSettings)
            {
                Settings.MaxGroundHeight = ARLocation.Config.MaxGroundHeight;
                Settings.MinGroundHeight = ARLocation.Config.MinGroundHeight;
                Settings.InitialGroundHeightGuess = ARLocation.Config.InitialGroundHeightGuess;
                Settings.Smoothing = ARLocation.Config.GroundHeightSmoothingFactor;
            }

            state.CurrentGroundY = -Settings.InitialGroundHeightGuess;

            arPlaneManager.planesChanged += ArPlaneManagerOnPlanesChanged;

            UpdateObjectHeight();
        }

        void OnEnable()
        {
            if (arPlaneManager)
            {
                arPlaneManager.planesChanged += ArPlaneManagerOnPlanesChanged;
            }
        }

        void OnDisable()
        {
            if (arPlaneManager)
            {
                arPlaneManager.planesChanged -= ArPlaneManagerOnPlanesChanged;
            }
        }

        private void ArPlaneManagerOnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
        {
            var addedPlanes = eventArgs.added;
            var updatedPlanes = eventArgs.updated;

            if (addedPlanes.Count <= 0 && updatedPlanes.Count <= 0)
            {
                // Debug.Log("[AR+GPS][GroundHeight#ArPlaneManagerOnPlanesChanged]: No added or modified planes!");
                return;
            }

            foreach (ARPlane plane in addedPlanes)
            {
                ProcessPlane(plane);
            }

            foreach (ARPlane plane in updatedPlanes)
            {
                ProcessPlane(plane);
            }

            UpdateObjectHeight();
        }

        private void ProcessPlane(ARPlane plane)
        {
            // Debug.Log("[AR+GPS][GroundHeight#ProcessPlane]: Processing plane " + plane.trackableId.subId1 + ", " + plane.trackableId.subId2);

            if (plane.alignment != PlaneAlignment.HorizontalDown && plane.alignment != PlaneAlignment.HorizontalUp)
            {
                // Debug.LogWarning("[AR+GPS][GroundHeight#ProcessPlane]: Wrong plane alignment!");
                return;
            }

            if (!IsValidHeightForGround(plane.center.y))
            {
                // Debug.LogWarning("[AR+GPS][GroundHeight#ProcessPlane]: Invalid plane height!");
                return;
            }

            var distance = MathUtils.HorizontalDistance(transform.position, plane.center);

            if (!(state.CurrentPlaneDistance < 0) && (distance >= state.CurrentPlaneDistance))
            {
                // Debug.LogWarning("[AR+GPS][GroundHeight#ProcessPlane]: Plane too far!");
                return;
            }

            // Debug.Log("[AR+GPS][GroundHeight#ProcessPlane]: New plane Y: " + plane.center.y);

            state.CurrentPlaneDistance = distance;
            state.CurrentGroundY = plane.center.y;
            state.CurrentPlaneCenter = plane.center;

            state.NeedsUpdate = true;
        }
#else
        private PlaneFinderBehaviour planeFinderBehaviour;

        private void Start()
        {
            planeFinderBehaviour = FindObjectOfType<PlaneFinderBehaviour>();
            mainCamera = ARLocationManager.Instance.MainCamera;

            if (planeFinderBehaviour == null)
            {
                Logger.WarnFromMethod("VuforiaGroundHeight", "Start", "No planeFinderBehaviour!");
            }

            if (Settings.UseArLocationConfigSettings)
            {
                Settings.MaxGroundHeight = ARLocation.Config.MaxGroundHeight;
                Settings.MinGroundHeight = ARLocation.Config.MinGroundHeight;
                Settings.InitialGroundHeightGuess = ARLocation.Config.InitialGroundHeightGuess;
                Settings.MinHitDistance = ARLocation.Config.VuforiaGroundHitTestDistance;
                Settings.Smoothing = ARLocation.Config.GroundHeightSmoothingFactor;
            }

            state.CurrentGroundY = -Settings.InitialGroundHeightGuess;

            planeFinderBehaviour.Height = Settings.InitialGroundHeightGuess;
            planeFinderBehaviour.HitTestMode = HitTestMode.AUTOMATIC;
            planeFinderBehaviour.OnAutomaticHitTest.AddListener(HitTestHandler);
            planeFinderBehaviour.OnInteractiveHitTest.AddListener(HitTestHandler);

            UpdateObjectHeight();
        }

        private void HitTestHandler(HitTestResult result)
        {
            //Logger.LogFromMethod("VuforiaGroundHeight", "HitTestHandler", $"result.Position = {result.Position}");

            // If the ground height is not in range, reject
            // var height = -1.0f * result.Position.y;
            if (!IsValidHeightForGround(result.Position.y))
            {
                //Logger.LogFromMethod("VuforiaGroundHeight", "HitTestHandler", $"Not in range: {result.Position.y} {height}) > {Settings.MinGroundHeight}");
                return;
            }

            var distanceToObject = MathUtils.HorizontalDistance(transform.position, result.Position);

            // If hit to close to previous hit do nothing
            if (state.CurrentPlaneDistance >= 0 && distanceToObject <= Settings.MinHitDistance)
            {
                //Logger.LogFromMethod("VuforiaGroundHeight", "HitTestHandler", $"Too close :{distanceToObject}");
                return;
            }

            // If there is no previous hit, or if the new hit is closes to the object, apply new
            // hit point.
            if (state.CurrentPlaneDistance < 0 || distanceToObject < state.CurrentPlaneDistance)
            {
                state.CurrentPlaneDistance = distanceToObject;
                state.CurrentGroundY = result.Position.y;
                state.NeedsUpdate = true;

                UpdateObjectHeight();

                // Logger.LogFromMethod("VuforiaGroundHeight", "HitTestHandler", $"New ground Y = {state.CurrentGroundY}");
            }
        }

#endif
        public void UpdateObjectHeight(bool force = false)
        {
            if (!state.NeedsUpdate && !force) return;

            // Debug.Log("[AR+GPS][GroundHeight#UpdateObjectHeight]: Setting Y to " + state.CurrentGroundY);

            if (Settings.Smoothing <= 0)
            {
                transform.position = MathUtils.SetY(transform.position, state.CurrentGroundY + Settings.Altitude);
            }

            state.NeedsUpdate = false;
        }

        private bool IsValidHeightForGround(float y)
        {
            var diff = (mainCamera.transform.position.y - y);

            return (diff >= Settings.MinGroundHeight) && (diff <= Settings.MaxGroundHeight);
        }

        public void Update()
        {
            if (Settings.Smoothing <= 0 || Settings.DisableUpdate) return;

            if (Mathf.Abs(transform.position.y - (state.CurrentGroundY + Settings.Altitude)) <= Settings.Precision)
            {
                transform.position = MathUtils.SetY(transform.position, (state.CurrentGroundY + Settings.Altitude));
                return;
            }

            var t = 1.0f - Mathf.Pow(Settings.Smoothing, Time.deltaTime);
            var position = transform.position;
            var value = Mathf.Lerp(position.y, (state.CurrentGroundY + Settings.Altitude), t);

            position = MathUtils.SetY(position, value);
            transform.position = position;
        }
    }
}
