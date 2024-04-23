using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation
{
    /// <summary>
    /// Data used to construct a spline passing trough a set of geographical
    /// locations.
    /// </summary>
    [CreateAssetMenu(fileName = "AR Location Path", menuName = "AR+GPS/Path")]
    public class LocationPath : ScriptableObject
    {
        /// <summary>
        /// The geographical locations that the path will interpolate.
        /// </summary>
        [FormerlySerializedAs("locations")] [Tooltip("The geographical locations that the path will interpolate.")]
        public Location[] Locations;

        [FormerlySerializedAs("splineType")] [Tooltip("The type of the spline used")]
        public SplineType SplineType = SplineType.CatmullromSpline;

        /// <summary>
        /// The path's alpha/tension factor.
        /// </summary>
        [FormerlySerializedAs("alpha")] [Tooltip("The path's alpha/tension factor.")]
        public float Alpha = 0.5f;

        /// <summary>
        /// The scale used in the editor scene viewer for drawing the path.
        /// </summary>
        [FormerlySerializedAs("sceneViewScale")] public float SceneViewScale = 1.0f;
    }
}
