using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;


namespace ARLocation.MapboxRoutes
{
    public class MapboxRoute : MonoBehaviour
    {
        // ================================================================================ //
        //  Public Classes                                                                  //
        // ================================================================================ //

        [Serializable]
        public class MapboxRouteLoadErrorEvent : UnityEvent<string> { }

        public enum RouteType
        {
            Mapbox,
            CustomRoute,
        }

        [Serializable]
        public class RouteSettings
        {
            public RouteType RouteType;

            [Tooltip("The route's starting point.")]
            public RouteWaypoint From;

            [Tooltip("The route's end point.")]
            public RouteWaypoint To = new RouteWaypoint { Type = RouteWaypointType.Query };

            public CustomRoute CustomRoute;
        }

        [Serializable]
        public class SettingsData
        {
            [Header("Prefabs")]
            [Tooltip("The \"Sign Post\" prefab implements the behaviour of a GameObject which is attached to each route maneuver point. It must contain a component which " +
                    "implements the abstract class 'AbstractSignPost'.")]
            public List<AbstractRouteSignpost> SignpostPrefabs = new List<AbstractRouteSignpost>();

            [Tooltip("The \"Path Renderer\" is resposible for drawing the line on the ground showing the user how to follow the route. It must implement the abstrac class 'AbstractRouteRenderer'")]
            public AbstractRouteRenderer PathRenderer;

            [Tooltip("The \"On Screen Indicator\" is responsable for drawing a 2D icon on the screen showing the direction of the next route target." +
                    "it must implement the abstract class 'AbstractOnScreenTargetIndicator'.")]
            public AbstractOnScreenTargetIndicator OnScreenIndicator;

            [Header("Mapbox")]
            [Tooltip("The Mapbox API token, used for accessing the Mapbox REST API.")]
            public string MapboxToken = "";

            [Header("Route")]
            public RouteSettings RouteSettings;

            [Header("Other settings")]
            [Tooltip("If true, load the route as soon as the component's \"Start\" method is called.")]
            public bool LoadRouteAtStartup = true;

            [Tooltip("The assumed height of the device from the ground.")]
            public float GroundHeight = 1.4f;

            [Header("Events")]
            [Tooltip("Event listener called whenever there is an error loading a route.")]
            public MapboxRouteLoadErrorEvent OnMapboxRouteLoadError;

            public bool DebugMode;
        }

        // ================================================================================ //
        //  Public Properties                                                               //
        // ================================================================================ //

        [Tooltip("Main settings for this component.")]
        public SettingsData Settings = new SettingsData();

        // ================================================================================ //
        //  Setters and Getters                                                             //
        // ================================================================================ //

        /// If there was an error when loading the route, this will return a
        /// string value with an error message. Otherwise it will return null
        /// on success.
        public string LoadRouteError => s.LoadRouteError;

        /// Returns the number of steps in the current Route.
        public int NumberOfSteps => s.RouteSteps.Count;

        // Gets/sets the current `RoutePathRenderer`.
        public AbstractRouteRenderer RoutePathRenderer
        {
            get => Settings.PathRenderer;
            set
            {
                if (value != Settings.PathRenderer)
                {
                    Settings.PathRenderer = value;
                    if (NumberOfSteps > 0)
                    {
                        Settings.PathRenderer.Init(createRoutePathRendererArgs());
                    }
                }
            }
        }

        // ================================================================================ //
        //  Private classes                                                                 //
        // ================================================================================ //

        [Serializable]
        class State
        {
            public string LoadRouteError = null;
            public List<List<AbstractRouteSignpost>> SignPostInstances = new List<List<AbstractRouteSignpost>>();
            public List<PlaceAtLocation> StepsPlaceAtInstances = new List<PlaceAtLocation>();
            public List<Route.Step> RouteSteps = new List<Route.Step>();
            public float RouteDistance;
            public Route.Geometry RouteGeometry;
            public int CurrentTargetIndex = -1;
        }

        // ================================================================================ //
        //  Private fields                                                                  //
        // ================================================================================ //

        private MapboxApi mapbox;
        private State s = new State();

        // ================================================================================ //
        //  Monobehaviour methods                                                           //
        // ================================================================================ //

        void Awake()
        {
            if (Settings.MapboxToken == "")
            {
                Utils.Logger.WarnFromMethod("MapboxRoute", "Awake",
                        "Please insert a Mapbox Token on the inspector panel for the 'MapboxRoutes' component!");
            }

            mapbox = new MapboxApi(Settings.MapboxToken);
        }

        void Start()
        {
            if (Settings.LoadRouteAtStartup)
            {
                ARLocationProvider.Instance.OnEnabled.AddListener(onLocationEnabled);
            }
        }

        void Update()
        {
            if (NumberOfSteps > 0)
            {
                bool shouldGotoNextTarget = false;

                for (int i = 0; i < NumberOfSteps; i++)
                {
                    var signpostInstances = s.SignPostInstances[i];
                    var signpostEventArgs = createSignPostEventArgs(i);
                    for (int j = 0; j < signpostInstances.Count; j++)
                    {
                        var instance = signpostInstances[j];
                        var result = instance.UpdateSignPost(signpostEventArgs);
                        if (i == s.CurrentTargetIndex && !result)
                        {
                            Utils.Logger.LogFromMethod("MapboxRoute", "Update", "NextTarget", Settings.DebugMode);
                            shouldGotoNextTarget = true;
                        }
                    }

                }

                if (shouldGotoNextTarget)
                {
                    NextTarget();
                }

                if (Settings.PathRenderer != null)
                {
                    Settings.PathRenderer.OnRouteUpdate(createRoutePathRendererArgs());
                }

                if (Settings.OnScreenIndicator != null)
                {
                    Settings.OnScreenIndicator.OnRouteUpdate(createSignPostEventArgs(s.CurrentTargetIndex));
                }
            }

        }

        // ================================================================================ //
        //  Private methods                                                                 //
        // ================================================================================ //

        private RoutePathRendererArgs createRoutePathRendererArgs()
        {
            var index = s.CurrentTargetIndex;
            var user = Camera.main.transform.position;
            var target = s.StepsPlaceAtInstances[index].transform.position;
            var distance = MathUtils.HorizontalDistance(user, target);

            var positions = new List<Vector3>(NumberOfSteps);
            for (int i = 0; i < NumberOfSteps; i++)
            {
                positions.Add(s.StepsPlaceAtInstances[i].transform.position);
            }

            return new RoutePathRendererArgs
            {
                Route = this,
                RouteSteps = s.RouteSteps,
                RouteGeometry = s.RouteGeometry,
                StepIndex = index,
                UserPos = user,
                TargetPos = target,
                Distance = distance,
                StepPositions = positions,
            };
        }

        private void onLocationEnabled(Location location)
        {
            if (Settings.LoadRouteAtStartup)
            {
                if (Settings.RouteSettings.RouteType == RouteType.CustomRoute)
                {
                    if (Settings.RouteSettings.CustomRoute != null)
                    {
                        LoadCustomRoute(Settings.RouteSettings.CustomRoute);
                    }
                    else
                    {
                        Utils.Logger.ErrorFromMethod("MapboxRoute", "onLocationEnabled", "RouteType is 'Custom Route' but 'CustomRoute' is null; please set the 'Custom Route' property on the inspector panel.");
                        return;
                    }
                }
                else
                {
                    StartCoroutine(LoadRoute());
                }
            }
        }

        private void clearRoute()
        {
            foreach (var signpostInstances in s.SignPostInstances)
            {
                foreach (var instance in signpostInstances)
                {
                    Destroy(instance.gameObject);
                }
            }

            foreach (var e in s.StepsPlaceAtInstances)
            {
                Destroy(e.gameObject);
            }

            s = new State();
        }

        private SignPostEventArgs createSignPostEventArgs()
        {
            return createSignPostEventArgs(s.CurrentTargetIndex);
        }

        private SignPostEventArgs createSignPostEventArgs(int index)
        {
            var user = Camera.main.transform.position;
            var target = s.StepsPlaceAtInstances[index].transform.position;
            var instruction = s.RouteSteps[index].maneuver.instruction;
            var name = s.RouteSteps[index].name;

            return new SignPostEventArgs
            {
                Route = this,
                UserPos = user,
                TargetPos = target,
                NextTargetPos = (index + 1) < NumberOfSteps ? s.StepsPlaceAtInstances[index + 1].transform.position : (Vector3?)null,
                PrevTargetPos = (index) > 0 ? s.StepsPlaceAtInstances[index - 1].transform.position : (Vector3?)null,
                Distance = MathUtils.HorizontalDistance(user, target),
                IsCurrentTarget = (index == s.CurrentTargetIndex),
                StepIndex = index,
                Instruction = instruction,
                Name = name,
            };
        }

        private bool isValidTargetIndex(int index)
        {
            return index >= 0 && index < s.RouteSteps.Count;
        }

        // ================================================================================ //
        //  Public methods                                                                  //
        // ================================================================================ //

        /// <summary>
        ///
        /// Given a `RouteResponse` form the `MapboxApi` class, builds the
        /// corresponding route. By building we mean that it will place all the
        /// AR+GPS objects, initialize the path rendering, and so on.
        ///
        /// </summary>
        public bool BuildRoute(RouteResponse result)
        {
            clearRoute();

            if (result.routes.Count == 0)
            {
                return false;
            }

            // We only support one route
            var route = result.routes[0];

            if (route.legs.Count == 0)
            {
                return false;
            }

            // Also only one leg. (Leg = a route from A to B)
            var leg = route.legs[0];


            // Go trough each of the leg's maneuvers
            int c = 0;
            foreach (var step in leg.steps)
            {
                var loc = step.maneuver.location;

                // Create a PlaceAtLocation gameObject for this step
                var go = new GameObject($"PlaceAt_{c}");

                var opt = new PlaceAtLocation.PlaceAtOptions { };
                opt.MaxNumberOfLocationUpdates = 0;

                var placeAt = PlaceAtLocation.AddPlaceAtComponent(go, loc, opt);
                s.StepsPlaceAtInstances.Add(placeAt);

                s.SignPostInstances.Add(new List<AbstractRouteSignpost>());

                // Create a signpost prefab instace for this step
                for (var i = 0; i < Settings.SignpostPrefabs.Count; i++)
                {
                    var prefab = Settings.SignpostPrefabs[i];
                    var signPostInstance = Instantiate(prefab);
                    signPostInstance.gameObject.SetActive(false);
                    s.SignPostInstances[c].Add(signPostInstance);
                    signPostInstance.Init(this);
                }

                c++;
            }

            s.RouteSteps = leg.steps;
            s.RouteDistance = leg.distance;
            s.RouteGeometry = route.geometry;

            // Set the first step as the current target
            SetTarget(0);

            if (Settings.PathRenderer != null)
            {
                Settings.PathRenderer.Init(createRoutePathRendererArgs());
            }

            if (Settings.OnScreenIndicator != null)
            {
                Settings.OnScreenIndicator.Init(this);
            }

            return true;
        }

        /// <summary>
        /// Sets the current target to `index`.
        ///
        /// In the context of the `MapboxRoute` class, a "target" is location
        /// of the route where a maneuver is expected to happen, e.g. "Turn
        /// right", "Keep left", "You have arrived at your destination".
        ///
        /// </summar>
        public void SetTarget(int index)
        {
            Utils.Logger.LogFromMethod("MapboxRoute", "SetTarget", $"index = {index}", Settings.DebugMode);

            if (index == s.CurrentTargetIndex)
            {
                return;
            }

            if (isValidTargetIndex(index))
            {
                SignPostEventArgs args;
                if (isValidTargetIndex(s.CurrentTargetIndex))
                {
                    args = createSignPostEventArgs();
                    foreach (var instance in s.SignPostInstances[s.CurrentTargetIndex])
                    {
                        instance.OffCurrentTarget(createSignPostEventArgs());
                    }
                }

                s.CurrentTargetIndex = index;
                args = createSignPostEventArgs();
                foreach (var instance in s.SignPostInstances[s.CurrentTargetIndex])
                {
                    instance.OnCurrentTarget(args);
                }
            }
        }

        /// <summary>
        /// Sets the current target to be the closest one.
        ///
        /// To be more precise, this will calculate the distances from the user
        /// position to each line segment constructed from one target to the
        /// next one: Target0->Target1, Target1->Target2, ..., TargetN-1->TargetN.
        ///
        /// Then it will pick the closes segment, e.g., TargetK->TargetK+1, and
        /// will set the current target to "K+1" (unless the user is positioned
        /// before the first target, in which case the target willi be set to "0".)
        ///
        /// </summary>
        public void ClosestTarget()
        {
            var position = MathUtils.HorizontalVector(Camera.main.transform.position);

            MathUtils.PointLineSegmentDistanceResult current = null;
            var currentIndex = -1;

            for (var i = 0; i < NumberOfSteps - 1; i++)
            {
                var a = MathUtils.HorizontalVector(s.StepsPlaceAtInstances[i].transform.position);
                var b = MathUtils.HorizontalVector(s.StepsPlaceAtInstances[i + 1].transform.position);

                var result = MathUtils.PointLineSegmentDistance(position, a, b);

                if (current == null || result.Distance < current.Distance)
                {
                    current = result;
                    currentIndex = i;
                }
            }

            if (currentIndex < 0)
            {
                Utils.Logger.WarnFromMethod("MapboxRoute", "ClosestTargetByUserPosition", "currentIndex < 0");
                return;
            }

            Utils.Logger.LogFromMethod("MapboxRoute", "ClosestTargetByUserPosition", $"{currentIndex}");

            switch (current.Region)
            {
                case MathUtils.LineSegmentRegion.Start:
                    if (currentIndex == 0)
                    {
                        SetTarget(currentIndex);
                    }
                    else
                    {
                        SetTarget(currentIndex + 1);
                    }
                    break;

                case MathUtils.LineSegmentRegion.Middle:
                    SetTarget(currentIndex + 1);
                    break;

                case MathUtils.LineSegmentRegion.End:
                    SetTarget(currentIndex + 1);
                    break;
            }
        }

        /// <summary>
        /// Sets the next target as the current one.
        /// </summary>
        public void NextTarget()
        {
            SetTarget(s.CurrentTargetIndex + 1);
        }

        /// <summary>
        /// Sets the previous target as the current one.
        /// </summary>
        public void PrevTarget()
        {
            SetTarget(s.CurrentTargetIndex - 1);
        }

        /// <summary>
        /// Sets the first/initial route target as the current one.
        /// </summary>
        public void FirstTarget()
        {
            SetTarget(0);
        }

        /// <summary>
        /// Sets the final route target as the current one.
        /// </summary>
        public void LastTarget()
        {
            SetTarget(NumberOfSteps - 1);
        }

        /// <summary>
        /// Reloads the current route.
        /// </summary>
        public void ReloadRoute()
        {
            onLocationEnabled(ARLocationProvider.Instance.CurrentLocation.ToLocation());
        }

        /// <summary>
        /// Loads a custom route defined by a `CustomRoute` ScriptableObject.
        /// </summary>
        public void LoadCustomRoute(CustomRoute route)
        {
            var res = new RouteResponse();
            res.routes = new List<Route> { route.ToMapboxRoute() };
            res.waypoints = route.GetWaypoints();

            BuildRoute(res);
        }

        /// <summary>
        /// Loads a route defined a start and end `Waypoint`s, and calls a given callback when the route is loaded.
        /// </summary>
        public System.Collections.IEnumerator LoadRoute(RouteWaypoint start, RouteWaypoint end, Action<string> callback)
        {
            yield return LoadRoute(start, end);

            callback(s.LoadRouteError);
        }

        /// <summary>
        /// Loads a route defined a start and end `Waypoint`s.
        /// </summary>
        public System.Collections.IEnumerator LoadRoute(RouteWaypoint start, RouteWaypoint end)
        {
            Debug.Assert(mapbox != null);

            var loader = new RouteLoader(mapbox);

            yield return loader.LoadRoute(start, end);

            if (loader.Error != null)
            {
                s.LoadRouteError = loader.Error;
                Settings.OnMapboxRouteLoadError?.Invoke(loader.Error);
            }
            else
            {
                s.LoadRouteError = null;
                BuildRoute(loader.Result);
            }
        }

        /// <summary>
        /// Loads a route from the current user location to a given `Waypoint`.
        /// </summary>
        public System.Collections.IEnumerator LoadRoute(RouteWaypoint routeWaypoint)
        {
            yield return LoadRoute(new RouteWaypoint { Type = RouteWaypointType.UserLocation }, routeWaypoint);
        }

        /// <summary>
        /// Loads the route defined by the waypoints given in the "RouteSettings".
        /// </summary>
        public System.Collections.IEnumerator LoadRoute()
        {
            yield return LoadRoute(Settings.RouteSettings.From, Settings.RouteSettings.To);
        }
    }
}
