using System;
using UnityEngine;

namespace ARLocation
{
    /// <summary>
    /// This component renders a LocationPath using a given LineRenderer.
    /// </summary>
    [AddComponentMenu("AR+GPS/Render Path Line")]
    [HelpURL("https://http://docs.unity-ar-gps-location.com/guide/#renderpathline")]
    public class RenderPathLine : MonoBehaviour
    {
        public MoveAlongPath.PathSettingsData PathSettings;
        public MoveAlongPath.PlacementSettingsData PlacementSettings;

        [HideInInspector] public Transform arLocationRoot;
        [HideInInspector] public MoveAlongPath moveAlongPath;

        public void Start()
        {
            if (PathSettings.LineRenderer == null)
            {
                var lineRenderer = gameObject.GetComponent<LineRenderer>();

                if (!lineRenderer)
                {
                    throw new NullReferenceException("[AR+GPS][RenderPathLine#Start]: No Line Renderer!");
                }

                PathSettings.LineRenderer = lineRenderer;
            }

            arLocationRoot = ARLocationManager.Instance.gameObject.transform;

            var pathGameObject = new GameObject($"{gameObject.name} - RenderPathLine");

            moveAlongPath = pathGameObject.AddComponent<MoveAlongPath>();
            moveAlongPath.PathSettings = PathSettings;
            moveAlongPath.PlacementSettings = PlacementSettings;
        }

        public void SetLocationPath(LocationPath path)
        {
            if (moveAlongPath != null)
            {
                moveAlongPath.SetLocationPath(path);
            }
        }
    }
}
