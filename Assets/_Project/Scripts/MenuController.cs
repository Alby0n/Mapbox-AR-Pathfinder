using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//This script, `MenuController`, manages the user interface and interactions for a sample project related to ARLocation and Mapbox routes. 
//This script orchestrates various functionalities such as searching for locations, displaying search results, initiating routes, updating map visuals, and managing UI transitions between the search menu and route view.
namespace ARLocation.MapboxRoutes.SampleProject
{
    public class MenuController : MonoBehaviour
    {
        public enum LineType
        {
            Route,
            NextTarget
        }
//   - Various fields are serialized for easy access and configuration in the Unity Editor. These include references to objects like the AR session, cameras, route renderers, materials, etc.
        public string MapboxToken = "pk.eyJ1IjoiZG1iZm0iLCJhIjoiY2tyYW9hdGMwNGt6dTJ2bzhieDg3NGJxNyJ9.qaQsMUbyu4iARFe0XB2SWg"; //   - The Mapbox API token used for location search and route calculation.
        public GameObject ARSession; 
        public GameObject ARSessionOrigin;
        public GameObject RouteContainer;
        public Camera Camera;
        public Camera MapboxMapCamera;
        public MapboxRoute MapboxRoute;
        public AbstractRouteRenderer RoutePathRenderer;
        public AbstractRouteRenderer NextTargetPathRenderer;
        public Texture RenderTexture;
        public Mapbox.Unity.Map.AbstractMap Map;
        [Range(100, 800)]
        public int MapSize = 400;
        public DirectionsFactory DirectionsFactory;
        public int MinimapLayer;
        public Material MinimapLineMaterial;
        public float BaseLineWidth = 2;
        public float MinimapStepSize = 0.5f;

        private AbstractRouteRenderer currentPathRenderer => s.LineType == LineType.Route ? RoutePathRenderer : NextTargetPathRenderer;

        public LineType PathRendererType
        { //   - Defines an enum `LineType` to distinguish between route lines and lines for the next target.
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
        { //- Contains fields to hold the current state of the menu, including the query text, search results, view mode, selected destination, line type, and error messages.
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
                _textFieldStyle.fontSize = 60;
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

        

        void Start()
        { //   - Initializes the scene by disabling route renderers and subscribing to events.
            NextTargetPathRenderer.enabled = false;
            RoutePathRenderer.enabled = false;
            ARLocationProvider.Instance.OnEnabled.AddListener(onLocationEnabled);
            Map.OnUpdated += OnMapRedrawn;
        }

        private void OnMapRedrawn()
        {
            // Debug.Log("OnMapRedrawn");
            if (currentResponse != null)
            {
                buildMinimapRoute(currentResponse);
            }
        }

        private void onLocationEnabled(Location location)
        {
            Map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Latitude, location.Longitude));
            // Map.SetZoom(18);
            Map.UpdateMap();
        }

        void OnEnable()
        {
            Debug.Log("Enable!!!!!!!!");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        { //   - Unsubscribes from events to prevent memory leaks.
            // ARLocationProvider.Instance.OnEnabled.RemoveListener(onLocationEnabled);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        { //   - Handles scene loading events.
            Debug.Log($"Scene Loaded: {scene.name}");
        }

        void drawMap()
        { //- Draws the map on the GUI using a texture.
            var tw = RenderTexture.width;
            var th = RenderTexture.height;

            var scale = MapSize / th;
            var newWidth = scale * tw;
            var x = Screen.width / 2 - newWidth / 2;
            float border;
            if (x < 0)
            {
                border = -x;
            }
            else
            {
                border = 0;
            }


            GUI.DrawTexture(new Rect(x, Screen.height - MapSize, newWidth, MapSize), RenderTexture, ScaleMode.ScaleAndCrop);
            GUI.DrawTexture(new Rect(0, Screen.height - MapSize - 20, Screen.width, 20), separatorTexture, ScaleMode.StretchToFill, false);

            var newZoom = GUI.HorizontalSlider(new Rect(0, Screen.height - 60, Screen.width, 60), Map.Zoom, 10, 22);

            if (newZoom != Map.Zoom)
            {
                Map.SetZoom(newZoom);
                Map.UpdateMap();
                // buildMinimapRoute(currentResponse);
            }
        }

        void OnGUI()
        {//   - Handles GUI rendering for location search, search results, and map display.
            if (s.View == View.Route)
            {
                drawMap();
                return;
            }

            float h = Screen.height - MapSize;
            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 20, 20) }, GUILayout.MaxHeight(h), GUILayout.Height(h));

            var w = Screen.width;

            GUILayout.BeginVertical(GUILayout.MaxHeight(100));
            GUILayout.Label("Location Search", textStyle());
            GUILayout.BeginHorizontal(GUILayout.MaxHeight(100), GUILayout.MinHeight(100));
            s.QueryText = GUILayout.TextField(s.QueryText, textFieldStyle(), GUILayout.MinWidth(0.76f * w), GUILayout.MaxWidth(0.76f * w),GUILayout.MaxHeight(100),GUILayout.MinHeight(100));
            string lastSearchQuery = "";  
            if (GUILayout.Button("SEARCH", buttonStyle(), GUILayout.MinWidth(0.2f * w), GUILayout.MaxWidth(0.2f * w),GUILayout.MaxHeight(100),GUILayout.MinHeight(100)))
            {
                s.ErrorMessage = null;
                StartCoroutine(search());
            }

           // if (!string.IsNullOrEmpty(s.QueryText)&& s.QueryText != lastSearchQuery) //recommendation code
            //{
              //  s.ErrorMessage = null;
               // StartCoroutine(search());
                //lastSearchQuery = s.QueryText;
            //}

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
            // GUILayout.Label(RenderTexture);
            GUILayout.EndVertical();
            // GUILayout.Label(RenderTexture, GUILayout.Height(mapSize));
            drawMap();
        }

        private Texture2D _separatorTexture;
        private Texture2D separatorTexture
        {
            get
            {
                if (_separatorTexture == null)
                {
                    _separatorTexture = new Texture2D(1, 1);
                    _separatorTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));
                    _separatorTexture.Apply();
                }

                return _separatorTexture;
            }
        }

        public void StartRoute(Location dest)
        { //    - Initiates the route calculation and AR session activation based on the selected destination.
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
        { //- Ends the route by deactivating the AR session and switching to the camera view.
            ARLocationProvider.Instance.OnEnabled.RemoveListener(loadRoute);
            ARSession.SetActive(false);
            ARSessionOrigin.SetActive(false);
            RouteContainer.SetActive(false);
            Camera.gameObject.SetActive(true);
            s.View = View.SearchMenu;
        }

        private void loadRoute(Location _)
        { //    - Loads the route based on the provided destination and updates the map accordingly.
            if (s.destination != null)
            {
                var api = new MapboxApi(MapboxToken);
                var loader = new RouteLoader(api);
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

                                currentPathRenderer.enabled = true;
                                MapboxRoute.RoutePathRenderer = currentPathRenderer;
                                MapboxRoute.BuildRoute(res);
                                currentResponse = res;
                                buildMinimapRoute(res);
                            }));
            }
        }

        private GameObject minimapRouteGo;
        private RouteResponse currentResponse;

        private void buildMinimapRoute(RouteResponse res)
        { //    - Constructs a minimap route based on the route response data.
            var geo = res.routes[0].geometry;
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var worldPositions = new List<Vector2>();

            foreach (var p in geo.coordinates)
            {
                /* var pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(
                        p.Latitude,
                        p.Longitude,
                        Map.CenterMercator,
                        Map.WorldRelativeScale
                        ); */

                // Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition
                var pos = Map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(p.Latitude, p.Longitude), true);
                worldPositions.Add(new Vector2(pos.x, pos.z));
                // worldPositions.Add(new Vector2((float)pos.x, (float)pos.y));
            }

            if (minimapRouteGo != null)
            {
                minimapRouteGo.Destroy();
            }

            minimapRouteGo = new GameObject("minimap route game object");
            minimapRouteGo.layer = MinimapLayer;

            var mesh = minimapRouteGo.AddComponent<MeshFilter>().mesh;

            var lineWidth = BaseLineWidth * Mathf.Pow(2.0f, Map.Zoom - 18);
            LineBuilder.BuildLineMesh(worldPositions, mesh, lineWidth);

            var meshRenderer = minimapRouteGo.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = MinimapLineMaterial;
        }

        IEnumerator search()
        {
            var api = new MapboxApi(MapboxToken);

            yield return api.QueryLocal(s.QueryText, true);

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

        Vector3 lastCameraPos;
        void Update()
        { //    - Updates the map camera's orientation based on the AR camera's position and handles map updates based on camera movement.
            if (s.View == View.Route)
            {
                var cameraPos = Camera.main.transform.position;

                var arLocationRootAngle = ARLocationManager.Instance.gameObject.transform.localEulerAngles.y;
                var cameraAngle = Camera.main.transform.localEulerAngles.y;
                var mapAngle = cameraAngle - arLocationRootAngle;

                MapboxMapCamera.transform.eulerAngles = new Vector3(90, mapAngle, 0);

                if ((cameraPos - lastCameraPos).magnitude < MinimapStepSize) {
                    return;
                }

                lastCameraPos = cameraPos;

                var location = ARLocationManager.Instance.GetLocationForWorldPosition(Camera.main.transform.position);

                Map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Latitude, location.Longitude));
                Map.UpdateMap();

            }
            else
            {
                MapboxMapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            }
        }
    }
}
