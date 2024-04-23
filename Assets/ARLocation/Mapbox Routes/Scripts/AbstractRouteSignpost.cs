using UnityEngine;

namespace ARLocation.MapboxRoutes
{
    public class SignPostEventArgs
    {
        public MapboxRoute Route;
        public Vector3 TargetPos;
        public Vector3? NextTargetPos;
        public Vector3? PrevTargetPos;
        public Vector3 UserPos;
        public float Distance;
        public bool IsCurrentTarget;
        public int StepIndex;

        public string Instruction { get; internal set; }
        public string Name { get; internal set; }
    }

    /// <summary>
    /// This abstract class should implement all the behaviour to guide the user along the route.
    ///
    /// Each "Step" of the route will instantiate an instance of
    /// `AbstractRouteSignpost` associated with a step/target of the route.
    ///
    /// For a reference implementation see the `SignPost` class.
    ///
    /// </summary>
    public abstract class AbstractRouteSignpost : MonoBehaviour
    {
        /// <summary>
        /// Initialization method. Called when the route is built the `AbstractRouteSignpost` is instantiated.
        /// </summary>
        public abstract void Init(MapboxRoute route);

        /// <summary>
        /// Called by `MapboxRoute` in the middle of its `Update` method. If this function returns `false` when the sign post's target
        /// is the current one, then `MapboxRoute` will set the current target to the next target.
        /// </summary>
        public abstract bool UpdateSignPost(SignPostEventArgs args);

        /// <summary>
        /// Called when the the sign post's target becomes the current target.
        /// </summary>
        public abstract void OnCurrentTarget(SignPostEventArgs args);

        /// <summary>
        /// Called when the the sign post's target no longer is the current target.
        /// </summary>
        public abstract void OffCurrentTarget(SignPostEventArgs args);
    }
}

