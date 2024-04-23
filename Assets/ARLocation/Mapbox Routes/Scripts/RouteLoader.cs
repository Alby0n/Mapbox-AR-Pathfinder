using UnityEngine;
using System;
using System.Collections;

namespace ARLocation.MapboxRoutes
{
    public class RouteLoader
    {
        MapboxApi mapbox;
        bool verbose;

        private string error;
        private RouteResponse result;

        public string Error => error;
        public RouteResponse Result => result;

        public RouteLoader(MapboxApi api, bool verboseMode = false)
        {
            mapbox = api;
            verbose = verboseMode;

            if (api == null)
            {
                Debug.LogError("[RouteLoader]: api is null.");
            }

            if (mapbox == null)
            {
                Debug.LogError("[RouteLoader]: mapbox is null.");
            }
        }

        public IEnumerator LoadRoute(RouteWaypoint start, RouteWaypoint end, Action<string, RouteResponse> callback)
        {
            yield return LoadRoute(start, end);

            callback?.Invoke(error, result);
        }

        public IEnumerator LoadRoute(RouteWaypoint start, RouteWaypoint end)
        {
            Debug.Assert(mapbox != null);

            if (verbose)
            {
                Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", $"Loading route from {start} to {end}", verbose);
            }

            // Resolve start location
            var resolver = new RouteWaypointResolveLocation(mapbox, start);
            yield return resolver.Resolve();

            if (resolver.IsError)
            {
                if (verbose)
                {
                    Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", $"Failed to resolve start waypoint: {resolver.ErrorMessage}", verbose);
                }

                error = resolver.ErrorMessage;
                result = null;

                yield break;
            }

            Location startLocation = resolver.result;

            // Resolve end location
            resolver = new RouteWaypointResolveLocation(mapbox, end);
            yield return resolver.Resolve();

            if (resolver.IsError)
            {
                if (verbose)
                {
                    Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", $"Failed to resolve end waypoint: {resolver.ErrorMessage}", verbose);
                }

                error = resolver.ErrorMessage;
                result = null;

                yield break;
            }

            Location endLocation = resolver.result;

            if (verbose)
            {
                Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", "Querying route...", verbose);
            }

            // Query the route from startLocation to endLocation
            yield return mapbox.QueryRoute(startLocation, endLocation, false, verbose);

            if (mapbox.errorMessage != null)
            {
                if (verbose)
                {
                    Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", $"Route query failed: {mapbox.errorMessage}", verbose);
                }

                error = resolver.ErrorMessage;
                result = null;

                yield break;
            }

            if (verbose)
            {
                Utils.Logger.LogFromMethod("RouteLoader", "LoadRoute", $"Done! {mapbox.QueryLocalResult}", verbose);
            }

            error = null;
            result = mapbox.QueryRouteResult;
        }
    }
}
