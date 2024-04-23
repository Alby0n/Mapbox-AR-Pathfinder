using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation
{
    using Utils;

    /// <summary>
    /// This component places instances of a given prefab/GameObject along
    /// equally spaced positions in a LocationPath. Should be placed in
    /// the ARLocationRoot GameObject.
    /// </summary>
    [AddComponentMenu("AR+GPS/Place Along Path")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#placealongpath")]
    public class PlaceAlongPath : MonoBehaviour
    {

        /// <summary>
        /// The path to place the prefab instances on.
        /// </summary>
        [Header("Path Settings")]
        [FormerlySerializedAs("path")] [Tooltip("The path to place the prefab instances on.")]
        public LocationPath Path;

        /// <summary>
        /// The prefab/GameObject to be palced along the path.
        /// </summary>
        [FormerlySerializedAs("prefab")] [Tooltip("The prefab/GameObject to be palced along the path.")]
        public GameObject Prefab;

        /// <summary>
        /// The number of object instances to be placed, excluding the endpoints. That is,
        /// the total number of instances is equal to objectCount + 2
        /// </summary>
        [FormerlySerializedAs("objectCount")] [Tooltip("The number of object instances to be placed, excluding the endpoints. That is, the total number of instances is equal to objectCount + 2")]
        public int ObjectCount = 10;

        /// <summary>
        /// The size of the sample used to calculate the spline.
        /// </summary>
        [FormerlySerializedAs("splineSampleSize")] [Tooltip("The size of the sample used to calculate the spline.")]
        public int SplineSampleSize = 200;

        public PlaceAtLocation.PlaceAtOptions PlacementSettings;

        public AltitudeMode AltitudeMode  = AltitudeMode.DeviceRelative;

        [Space(4.0f)]

        [Header("Debug")]
        [Tooltip("When debug mode is enabled, this component will print relevant messages to the console. Filter by 'PlaceAlongPath' in the log output to see the messages.")]
        public bool DebugMode;

        [Space(4.0f)]

        private  Spline spline;

        private Vector3[] points;

        private void Start()
        {
            points = new Vector3[Path.Locations.Length];

            for (var i = 0; i < points.Length; i++)
            {
                points[i] = Path.Locations[i].ToVector3();
            }

            spline = Misc.BuildSpline(Path.SplineType, points, SplineSampleSize, Path.Alpha);

            var sample = spline.SamplePoints(ObjectCount);


            for (var i = 0; i < sample.Length; i++)
            {
                var location = new Location()
                {
                    Latitude = sample[i].z,
                    Longitude = sample[i].x,
                    Altitude = sample[i].y,
                    AltitudeMode = AltitudeMode
                };
                var instance = PlaceAtLocation.CreatePlacedInstance(Prefab, location, PlacementSettings, DebugMode);

                instance.name = $"{gameObject.name} - {i}";
            }
        }
    }
}
