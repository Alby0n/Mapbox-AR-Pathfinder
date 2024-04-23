using UnityEngine;

namespace ARLocation.MapboxRoutes
{
    public class NextStepRoutePathRenderer : AbstractRouteRenderer
    {
        [System.Serializable]
        public class SettingsData
        {
            [Tooltip("The material used to render the line.")]
            public Material LineMaterial;

            [Tooltip("A texture offset factor used to move the line's texture as to appear that a pattern is moving." +
                     " If you don't need this just set it to '0'.")]
            public float TextureOffsetFactor = -4.0f;
        }

        [System.Serializable]
        private class State
        {
            public GameObject Go;
            public LineRenderer LineRenderer;
        }

        public SettingsData Settings = new SettingsData{};

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
            state.Go = new GameObject("[NextStepRoutePathRenderer]");
            state.LineRenderer = state.Go.AddComponent<LineRenderer>();
            state.LineRenderer.startWidth = 0.25f;
            state.LineRenderer.useWorldSpace = true;
            state.LineRenderer.alignment = LineAlignment.View;
            state.LineRenderer.material = Settings.LineMaterial;
            state.LineRenderer.textureMode = LineTextureMode.Tile;
            state.LineRenderer.numCornerVertices = 2;
        }

        public void OnDisable()
        {
            GameObject.Destroy(state.Go);
            state.Go = null;
        }

        public override void Init(RoutePathRendererArgs args)
        {
        }

        public override void OnRouteUpdate(RoutePathRendererArgs args)
        {
            var groundHeight = args.Route.Settings.GroundHeight;
            var y = Camera.main.transform.position.y;
            var userPos = MathUtils.SetY(args.UserPos, y-groundHeight);
            var targetPos = MathUtils.SetY(args.TargetPos, y-groundHeight);

            state.LineRenderer.material.SetVector("_Origin", args.UserPos);
            state.LineRenderer.positionCount = 2;
            state.LineRenderer.SetPosition(0, userPos);
            state.LineRenderer.SetPosition(1, targetPos);
            state.LineRenderer.material.SetTextureOffset("_MainTex", Settings.TextureOffsetFactor * new Vector2(args.Distance, 0));
        }
    }
}
