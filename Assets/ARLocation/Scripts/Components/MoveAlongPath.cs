using System;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable UnusedMember.Global

namespace ARLocation
{
    using Utils;

    /// <summary>
    /// This component, when attached to a GameObject, makes it traverse a
    /// path that interpolates a given set of geographical locations.
    /// </summary>
    [AddComponentMenu("AR+GPS/Move Along Path")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#movealongpath")]
    [DisallowMultipleComponent]
    public class MoveAlongPath : MonoBehaviour
    {
        [Serializable]
        public class PathSettingsData
        {
            /// <summary>
            /// The LocationPath describing the path to be traversed.
            /// </summary>
            [Tooltip("The LocationPath describing the path to be traversed.")]
            public LocationPath LocationPath;

            /// <summary>
            /// The number of points-per-segment used to calculate the spline.
            /// </summary>
            [Tooltip("The number of points-per-segment used to calculate the spline.")]
            public int SplineSampleCount = 250;

            /// <summary>
            /// If present, renders the spline in the scene using the given line renderer.
            /// </summary>
            [FormerlySerializedAs("lineRenderer")] [Tooltip("If present, renders the spline in the scene using the given line renderer.")]
            public LineRenderer LineRenderer;
        }

        [Serializable]
        public class PlaybackSettingsData
        {
            /// <summary>
            /// The speed along the path.
            /// </summary>
            [Tooltip("The speed along the path.")]
            public float Speed = 1.0f;

            /// <summary>
            /// The up direction to be used for orientation along the path.
            /// </summary>
            [Tooltip("The up direction to be used for orientation along the path.")]
            public Vector3 Up = Vector3.up;

            /// <summary>
            /// If true, play the path traversal in a loop.
            /// </summary>
            [Tooltip("If true, play the path traversal in a loop.")]
            public bool Loop = true;

            /// <summary>
            /// If true, start playing automatically.
            /// </summary>
            [Tooltip("If true, start playing automatically.")]
            public bool AutoPlay = true;

            [FormerlySerializedAs("offset")] [Tooltip("The parameters offset; marks the initial position of the object along the curve.")]
            public float Offset;
        }

        [Serializable]
        public class PlacementSettingsData
        {
            [Tooltip("The altitude mode. The altitude modes of the individual path locations are ignored, and this will be used instead.")]
            public AltitudeMode AltitudeMode = AltitudeMode.DeviceRelative;

            [Tooltip(
                "The maximum number of times this object will be affected by GPS location updates. Zero means no limits are imposed.")]
            public uint MaxNumberOfLocationUpdates = 4;
        }

        [Serializable]
        public class StateData
        {
            public uint UpdateCount;
            public Vector3[] Points;
            public int PointCount;
            public bool Playing;
            public Spline Spline;
            public Vector3 Translation;
            public float Speed;
        }

        public PathSettingsData PathSettings = new PathSettingsData();
        public PlaybackSettingsData PlaybackSettings = new PlaybackSettingsData();
        public PlacementSettingsData PlacementSettings = new PlacementSettingsData();

        public float Speed
        {
            get => state.Speed;
            set => state.Speed = value;
        }

        [Space(4.0f)]

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'MoveAlongPath' in the log output to see the messages.")]
        public bool DebugMode;

        [Space(4.0f)]

        private StateData state = new StateData();
        private ARLocationProvider locationProvider;
        private float u;
        private GameObject arLocationRoot;
        private Transform mainCameraTransform;
        private bool useLineRenderer;
        private bool hasInitialized;
        private GroundHeight groundHeight;

        private bool HeightRelativeToDevice => PlacementSettings.AltitudeMode == AltitudeMode.DeviceRelative;
        private bool HeightGroundRelative => PlacementSettings.AltitudeMode == AltitudeMode.GroundRelative;

        /// <summary>
        /// Change the `LocationPath` the GameObject will traverse. This will
        /// have the effect of reseting the movement to the start of the path.
        /// </summary>
        /// <param name="path"></param>
        public void SetLocationPath(LocationPath path)
        {
            PathSettings.LocationPath = path;

            state.PointCount = PathSettings.LocationPath.Locations.Length;
            state.Points = new Vector3[state.PointCount];
            u = 0;

            BuildSpline(locationProvider.CurrentLocation.ToLocation());
        }


        void Start()
        {
            if (PathSettings.LocationPath == null)
            {
                throw new NullReferenceException("[AR+GPS][MoveAlongPath]: Null Path! Please set the 'LocationPath' property!");
            }

            locationProvider = ARLocationProvider.Instance;

            mainCameraTransform = ARLocationManager.Instance.MainCamera.transform;
            arLocationRoot = ARLocationManager.Instance.gameObject; // Misc.FindAndLogError("ARLocationRoot", "[ARLocationMoveAlongPath]: ARLocationRoot GameObject not found.");

            Initialize();
            hasInitialized = true;
        }

        private void Initialize()
        {
            state.PointCount = PathSettings.LocationPath.Locations.Length;
            state.Points = new Vector3[state.PointCount];
            state.Speed = PlaybackSettings.Speed;

            Debug.Log(state.PointCount);
            Debug.Log(state.Points);

            useLineRenderer = PathSettings.LineRenderer != null;

            transform.SetParent(arLocationRoot.transform);

            state.Playing = PlaybackSettings.AutoPlay;

            u += PlaybackSettings.Offset;

            groundHeight = GetComponent<GroundHeight>();
            if (PlacementSettings.AltitudeMode == AltitudeMode.GroundRelative)
            {
                if (!groundHeight)
                {
                    groundHeight = gameObject.AddComponent<GroundHeight>();
                    groundHeight.Settings.DisableUpdate = true;
                }
            }
            else
            {
                if (groundHeight)
                {
                    Destroy(groundHeight);
                    groundHeight = null;
                }
            }

            if (!hasInitialized)
            {
                locationProvider.OnProviderRestartEvent(ProviderRestarted);
            }

            locationProvider.OnLocationUpdatedEvent(LocationUpdated);

            //if (locationProvider.IsEnabled)
            // {
            //     LocationUpdated(locationProvider.CurrentLocation, locationProvider.LastLocation);
            //}
        }

        private void ProviderRestarted()
        {
            state.UpdateCount = 0;
        }

        public void Restart()
        {
            state = new StateData();
            Initialize();
        }


        /// <summary>
        /// Starts playing or resumes the playback.
        /// </summary>
        public void Play()
        {
            state.Playing = true;
        }

        /// <summary>
        /// Moves the object to the spline point corresponding
        /// to the given parameter.
        /// </summary>
        /// <param name="t">Between 0 and 1</param>
        public void GoTo(float t)
        {
            u = Mathf.Clamp(t, 0, 1);
        }

        /// <summary>
        /// Pauses the movement along the path.
        /// </summary>
        public void Pause()
        {
            state.Playing = false;
        }

        /// <summary>
        /// Stops the movement along the path.
        /// </summary>
        public void Stop()
        {
            state.Playing = false;
            u = 0;
        }

        private void BuildSpline(Location location)
        {
            for (var i = 0; i < state.PointCount; i++)
            {
                var loc = PathSettings.LocationPath.Locations[i];

                state.Points[i] = Location.GetGameObjectPositionForLocation(arLocationRoot.transform,
                    mainCameraTransform, location, loc, HeightRelativeToDevice || HeightGroundRelative);

                Logger.LogFromMethod("MoveAlongPath", "BuildSpline", $"({gameObject.name}): Points[{i}] = {state.Points[i]},  geo-location = {loc}", DebugMode);
            }

            state.Spline = Misc.BuildSpline(PathSettings.LocationPath.SplineType, state.Points, PathSettings.SplineSampleCount, PathSettings.LocationPath.Alpha);
        }

        private void LocationUpdated(LocationReading location, LocationReading _)
        {
            Logger.LogFromMethod("MoveAlongPath", "LocationUpdated", $"({gameObject.name}): New device location {location}", DebugMode);

            if (PlacementSettings.MaxNumberOfLocationUpdates > 0 && state.UpdateCount > PlacementSettings.MaxNumberOfLocationUpdates)
            {
                Logger.LogFromMethod("MoveAlongPath", "LocationUpdated", $"({gameObject.name}): Max number of updates reached! returning", DebugMode);
                return;
            }

            BuildSpline(location.ToLocation());
            state.Translation = new Vector3(0, 0, 0);

            state.UpdateCount++;
        }

        private void Update()
        {
            if (!state.Playing)
            {
                return;
            }

            // If there is no location provider, or spline, do nothing
            if (state.Spline == null || !locationProvider.IsEnabled)
            {
                return;
            }

            // Get spline point at current parameter
            var s = state.Spline.Length * u;

            var data = state.Spline.GetPointAndTangentAtArcLength(s);
            var tan = arLocationRoot.transform.InverseTransformVector(data.tangent);

            transform.position = data.point;

            var groundY = 0.0f;
            if (groundHeight)
            {
                var position = transform.position;
                groundY = groundHeight.CurrentGroundY;
                position = MathUtils.SetY(position, position.y + groundY);
                transform.position = position;
            }

            // Set orientation
            transform.localRotation = Quaternion.LookRotation(tan, PlaybackSettings.Up);

            // Check if we reached the end of the spline
            u = u + (state.Speed * Time.deltaTime) / state.Spline.Length;
            if (u >= 1 && !PlaybackSettings.Loop)
            {
                u = 0;
                state.Playing = false;
            }
            else
            {
                u = u % 1.0f;
            }

            // If there is a line renderer, render the path
            if (useLineRenderer)
            {
                PathSettings.LineRenderer.useWorldSpace = true;
                var t = arLocationRoot.transform;
                state.Spline.DrawCurveWithLineRenderer(PathSettings.LineRenderer,
                    p => MathUtils.SetY(p, p.y + groundY)); //t.TransformVector(p - state.Translation));
            }
        }

        private void OnDestroy()
        {
            locationProvider.OnLocationUpdatedDelegate -= LocationUpdated;
            locationProvider.OnRestartDelegate -= ProviderRestarted;
        }
    }
}
