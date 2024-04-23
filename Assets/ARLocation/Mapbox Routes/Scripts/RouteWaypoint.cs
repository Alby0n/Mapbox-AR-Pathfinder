using System.Collections;

namespace ARLocation.MapboxRoutes
{
    public enum RouteWaypointType
    {
        UserLocation,
        Location,
        Query
    };

    [System.Serializable]
    public class RouteWaypoint
    {
        public RouteWaypointType Type;
        public Location Location = new Location();
        public string Query;

        public override string ToString()
        {
            return "RouteWaypoint{ \n" +
                $"Type = {Type}\n" +
                $"Location = {Location}\n" +
                $"Query = {Query}\n" +
                "}";
        }
    }

    public class RouteWaypointResolveLocation
    {
        public Location result;
        public bool IsError;
        public string ErrorMessage;

        private RouteWaypoint w;
        private MapboxApi api;

        public RouteWaypointResolveLocation(MapboxApi mapboxApi, RouteWaypoint waypoint)
        {
            w = waypoint;
            api = mapboxApi;
        }

        public IEnumerator Resolve()
        {
            switch (w.Type)
            {
                case RouteWaypointType.Location:
                    result = w.Location;
                    IsError = false;
                    ErrorMessage = null;
                    yield break;

                case RouteWaypointType.UserLocation:
                    result = ARLocationProvider.Instance.CurrentLocation.ToLocation();
                    IsError = false;
                    ErrorMessage = null;
                    yield break;

                case RouteWaypointType.Query:
                    yield return api.QueryLocal(w.Query);

                    if (api.ErrorMessage != null)
                    {
                        result = null;
                        IsError = true;
                        ErrorMessage = api.ErrorMessage;
                    }
                    else
                    {
                        result = api.QueryLocalResult.features[0].geometry.coordinates[0];
                        IsError = false;
                        ErrorMessage = null;
                    }

                    yield break;
            }
        }
    }
}
