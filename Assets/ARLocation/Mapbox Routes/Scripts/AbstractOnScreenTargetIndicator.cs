using UnityEngine;

namespace ARLocation.MapboxRoutes
{
    public abstract class AbstractOnScreenTargetIndicator : MonoBehaviour
    {
        public abstract void Init(MapboxRoute route);
        public abstract void OnRouteUpdate(SignPostEventArgs args);
    }
}
