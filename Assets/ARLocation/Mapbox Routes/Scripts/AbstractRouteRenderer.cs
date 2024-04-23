using UnityEngine;
using System.Collections.Generic;

namespace ARLocation.MapboxRoutes
{
    public struct RoutePathRendererArgs
    {
        public MapboxRoute Route;
        public List<Route.Step> RouteSteps;
        public Route.Geometry RouteGeometry;
        public List<Vector3> StepPositions;
        public int StepIndex;
        public Vector3 UserPos;
        public Vector3 TargetPos;
        public float Distance;
    }

    public abstract class AbstractRouteRenderer : MonoBehaviour
    {
        public abstract void Init(RoutePathRendererArgs args);
        public abstract void OnRouteUpdate(RoutePathRendererArgs args);
    }
}

