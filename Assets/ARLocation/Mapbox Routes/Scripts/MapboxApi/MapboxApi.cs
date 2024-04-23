using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Globalization;

namespace ARLocation.MapboxRoutes
{
    using Vendor.SimpleJSON;

    [System.Serializable]
    public class MapboxApi
    {
        public string AccessToken;

        private RouteResponse queryRouteResult;
        public RouteResponse QueryRouteResult => queryRouteResult;

        private GeocodingResponse queryLocalResult;
        public GeocodingResponse QueryLocalResult => queryLocalResult;

        public string errorMessage;
        public string ErrorMessage => errorMessage;

        public MapboxApi(string token)
        {
            AccessToken = token;
        }

        public IEnumerator QueryLocal(string text, bool verbose = false)
        {
            var term = text;
            var url = Uri.EscapeUriString($"https://api.mapbox.com/geocoding/v5/mapbox.places/{term}.json?access_token={AccessToken}");

            errorMessage = null;
            queryLocalResult = null;

            if (verbose)
            {
                Debug.Log($"[MapboxApi#QueryLocal]: {url}");
            }

            using (var req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (Utils.Misc.WebRequestResultIsError(req))
                {
                    if (verbose)
                    {
                        Debug.LogError("[MapboxApi#QueryLocal]: Error -> " + req.error);
                    }

                    errorMessage = req.error;
                }
                else
                {
                    if (req.responseCode != 200)
                    {
                        if (verbose)
                        {
                            Debug.LogError("[MapboxApi#QueryLocal]: Error -> " + req.downloadHandler.text);
                            var node = JSON.Parse(req.downloadHandler.text);
                            errorMessage = node["message"].Value; //req.downloadHandler.text;
                            queryLocalResult = null;
                        }
                    }
                    else
                    {
                        if (verbose)
                        {
                            Debug.Log("[MapboxApi#QueryLocal]: Success -> " + req.downloadHandler.text);
                        }

                        queryLocalResult = GeocodingResponse.Parse(req.downloadHandler.text);
                    }
                }
            }
        }

        public IEnumerator QueryRoute(Location from, Location to, bool alternatives = false, bool verbose = false)
        {
            string alt = alternatives ? "true" : "false";

            var fromLat = from.Latitude.ToString(CultureInfo.InvariantCulture);
            var fromLon = from.Longitude.ToString(CultureInfo.InvariantCulture);
            var toLat = to.Latitude.ToString(CultureInfo.InvariantCulture);
            var toLon = to.Longitude.ToString(CultureInfo.InvariantCulture);
            
            string url = $"https://api.mapbox.com/directions/v5/mapbox/walking/{fromLon}%2C{fromLat}%3B{toLon}%2C{toLat}?alternatives={alt}&geometries=geojson&steps=true&access_token={AccessToken}";
            
            errorMessage = null;
            queryRouteResult = null;

            if (verbose)
            {
                Debug.Log($"[MapboxApi#QueryRoute]: {url}");
            }

            using (var req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (Utils.Misc.WebRequestResultIsError(req))
                {
                    if (verbose)
                    {
                        Debug.LogError("[MapboxApi#QueryRoute]: Error -> " + req.error);
                    }

                    errorMessage = req.error;
                }
                else
                {
                    if (verbose)
                    {
                        Debug.Log("[MapboxApi#QueryRoute]: Success -> " + req.downloadHandler.text);
                        Debug.Log("[MapboxApi#QueryRoute]: Success -> " + req.responseCode);
                    }

                    queryRouteResult = RouteResponse.Parse(req.downloadHandler.text);

                    if (queryRouteResult.Code != "Ok")
                    {
                        errorMessage = queryRouteResult.Code;
                        queryRouteResult = null;
                    }
                    else
                    {
                        if (verbose)
                        {
                            Debug.Log("[MapboxApi#QueryRoute]: Parsed result -> " + queryRouteResult);
                        }
                    }
                }
            }
        }
    }
}
