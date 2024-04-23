using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARLocation.MapboxRoutes.Examples.Search
{
    public class MenuController : MonoBehaviour
    {
        public enum LineType
        {
            Route,
            NextTarget
        }

        public string MapboxToken = "pk.eyJ1IjoiZG1iZm0iLCJhIjoiY2tyYW9hdGMwNGt6dTJ2bzhieDg3NGJxNyJ9.qaQsMUbyu4iARFe0XB2SWg";
        public GameObject ARSession;
        public GameObject ARSessionOrigin;
        public GameObject RouteContainer;
        public Camera Camera;
        public MapboxRoute MapboxRoute;
        public AbstractRouteRenderer RoutePathRenderer;
        public AbstractRouteRenderer NextTargetPathRenderer;
        public bool DebugMode = true;

        private AbstractRouteRenderer currentPathRenderer => s.LineType == LineType.Route ? RoutePathRenderer : NextTargetPathRenderer;

        public LineType PathRendererType
        {
            get => s.LineType;
            set
            {
                if (value != s.LineType)
                {
                    currentPathRenderer.enabled = false;
                    s.LineType = value;
                    currentPathRenderer.enabled = true;

                    if (s.View == View.Route)
                    {
                        MapboxRoute.RoutePathRenderer = currentPathRenderer;
                    }
                }
            }
        }

        enum View
        {
            SearchMenu,
            Route,
        }

        [System.Serializable]
        private class State
        {
            public string QueryText = "";
            public List<GeocodingFeature> Results = new List<GeocodingFeature>();
            public View View = View.SearchMenu;
            public Location destination;
            public LineType LineType = LineType.NextTarget;
            public string ErrorMessage;
        }

        private State s = new State();

        private GUIStyle _textStyle;
        GUIStyle textStyle()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.label);
                _textStyle.fontSize = 48;
                _textStyle.fontStyle = FontStyle.Bold;
            }

            return _textStyle;
        }

        private GUIStyle _textFieldStyle;
        GUIStyle textFieldStyle()
        {
            if (_textFieldStyle == null)
            {
                _textFieldStyle = new GUIStyle(GUI.skin.textField);
                _textFieldStyle.fontSize = 48;
            }
            return _textFieldStyle;
        }

        private GUIStyle _errorLabelStyle;
        GUIStyle errorLabelSytle()
        {
            if (_errorLabelStyle == null)
            {
                _errorLabelStyle = new GUIStyle(GUI.skin.label);
                _errorLabelStyle.fontSize = 24;
                _errorLabelStyle.fontStyle = FontStyle.Bold;
                _errorLabelStyle.normal.textColor = Color.red;
            }

            return _errorLabelStyle;
        }


        private GUIStyle _buttonStyle;
        GUIStyle buttonStyle()
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 48;
            }

            return _buttonStyle;
        }

        void OnGUI()
        {
            if (s.View == View.Route)
            {
                return;
            }

            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 20, 20) });

            var w = Screen.width;

            GUILayout.BeginVertical(GUILayout.MaxHeight(100));
            GUILayout.Label("Location Search", textStyle());
            GUILayout.BeginHorizontal(GUILayout.MaxHeight(100), GUILayout.MinHeight(100));
            s.QueryText = GUILayout.TextField(s.QueryText, textFieldStyle(), GUILayout.MinWidth(0.8f * w), GUILayout.MaxWidth(0.8f * w));

            if (GUILayout.Button("OK", buttonStyle(), GUILayout.MinWidth(0.15f * w), GUILayout.MaxWidth(0.15f * w)))
            {
                s.ErrorMessage = null;
                StartCoroutine(search());
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            if (s.ErrorMessage != null)
            {
                GUILayout.Label(s.ErrorMessage, errorLabelSytle());
            }

            foreach (var r in s.Results)
            {
                if (GUILayout.Button(r.place_name, new GUIStyle(buttonStyle()) { alignment = TextAnchor.MiddleLeft, fontSize = 24, fixedHeight = 0.05f * Screen.height }))
                {
                    StartRoute(r.geometry.coordinates[0]);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        public void StartRoute(Location dest)
        {
            s.destination = dest;

            if (ARLocationProvider.Instance.IsEnabled)
            {
                loadRoute(ARLocationProvider.Instance.CurrentLocation.ToLocation());
            }
            else
            {
                ARLocationProvider.Instance.OnEnabled.AddListener(loadRoute);
            }
        }

        public void EndRoute()
        {
            ARLocationProvider.Instance.OnEnabled.RemoveListener(loadRoute);
            ARSession.SetActive(false);
            ARSessionOrigin.SetActive(false);
            RouteContainer.SetActive(false);
            Camera.gameObject.SetActive(true);
            s.View = View.SearchMenu;
        }

        private void loadRoute(Location _)
        {
            if (s.destination != null)
            {
                var api = new MapboxApi(MapboxToken);
                var loader = new RouteLoader(api, DebugMode);
                StartCoroutine(
                        loader.LoadRoute(
                            new RouteWaypoint { Type = RouteWaypointType.UserLocation },
                            new RouteWaypoint { Type = RouteWaypointType.Location, Location = s.destination },
                            (err, res) =>
                            {
                                if (err != null)
                                {
                                    s.ErrorMessage = err;
                                    s.Results = new List<GeocodingFeature>();
                                    return;
                                }

                                ARSession.SetActive(true);
                                ARSessionOrigin.SetActive(true);
                                RouteContainer.SetActive(true);
                                Camera.gameObject.SetActive(false);
                                s.View = View.Route;

                                MapboxRoute.RoutePathRenderer = currentPathRenderer;
                                MapboxRoute.BuildRoute(res);
                            }));
            }
        }

        IEnumerator search()
        {
            var api = new MapboxApi(MapboxToken);

            yield return api.QueryLocal(s.QueryText, DebugMode);

            if (api.ErrorMessage != null)
            {
                s.ErrorMessage = api.ErrorMessage;
                s.Results = new List<GeocodingFeature>();
            }
            else
            {
                s.Results = api.QueryLocalResult.features;
            }
        }
    }
}
