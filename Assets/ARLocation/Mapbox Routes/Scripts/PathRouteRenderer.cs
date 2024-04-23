using UnityEngine;

namespace ARLocation.MapboxRoutes
{
    public class PathRouteRenderer : AbstractRouteRenderer
    {
        [System.Serializable]
        public class SettingsData
        {
            [Tooltip("The Material used to render the line.")]
            public Material LineMaterial;
        }

        [System.Serializable]
        private class State
        {
            public GameObject Go;
            public LineRenderer LineRenderer;
            public NewPathLineRenderer PathLineRenderer;
            public LocationPath Path;
        }

        public SettingsData Settings;

        private State state = new State();

        public void OnDestroy()
        {
            if (state.Go != null)
            {
                GameObject.Destroy(state.Go);
            }
        }

        public void OnEnable()
        {
            state.Go = new GameObject("[RoutePathRenderer]");
            state.LineRenderer = state.Go.AddComponent<LineRenderer>();
            state.LineRenderer.startWidth = 0.25f;
            state.LineRenderer.useWorldSpace = true;
            state.LineRenderer.alignment = LineAlignment.TransformZ;
            state.LineRenderer.material = Settings.LineMaterial;
            state.LineRenderer.textureMode = LineTextureMode.Tile;
            state.LineRenderer.numCornerVertices = 2;
            state.LineRenderer.gameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);

            state.PathLineRenderer = state.Go.AddComponent<NewPathLineRenderer>();
            state.PathLineRenderer.MaxNumberOfUpdates = 0;
        }

        public void OnDisable()
        {
            GameObject.Destroy(state.Go);
            state.Go = null;
        }

        public override void Init(RoutePathRendererArgs args)
        {
            state.Path = ScriptableObject.CreateInstance<LocationPath>();
            state.Path.Locations = new Location[args.RouteGeometry.coordinates.Count];

            for (var i = 0; i < state.Path.Locations.Length; i++)
            {
                state.Path.Locations[i] = args.RouteGeometry.coordinates[i].Clone();
            }

            state.Path.SplineType = SplineType.LinearSpline;
            state.PathLineRenderer.Init(state.Path, state.LineRenderer);
        }

        public override void OnRouteUpdate(RoutePathRendererArgs args)
        {
            state.LineRenderer.material.SetVector("_Origin", args.UserPos);
        }

    }
}
