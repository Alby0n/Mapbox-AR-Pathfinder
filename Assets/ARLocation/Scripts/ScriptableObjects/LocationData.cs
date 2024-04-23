using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation
{
    /// <summary>
    /// Data used to construct a spline passing trough a set of geographical
    /// locations.
    /// </summary>
    [CreateAssetMenu(fileName = "AR Location Data", menuName = "AR+GPS/Location")]
    public class LocationData : ScriptableObject
    {
        /// <summary>
        /// The geographical locations that the path will interpolate.
        /// </summary>
        [FormerlySerializedAs("location")] [Tooltip("The geographical locations that the path will interpolate.")]
        public Location Location;

        public static LocationData FromLocation(Location location) {
            var data = CreateInstance<LocationData>();
            data.Location = location;

            return data;
        }

        public override string ToString()
        {
            return Location.ToString();
        }
    }
}
