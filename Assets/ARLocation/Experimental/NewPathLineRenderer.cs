using UnityEngine;

namespace ARLocation
{
    public class NewPathLineRenderer : MonoBehaviour
    {
        public LocationPath Path;
        public int MaxNumberOfUpdates = 4;
        private ARLocationProvider locationProvider;
        private LineRenderer lineRenderer;
        private Transform arLocationRoot;
        bool initialized;
        int updateCount;

        void Start()
        {
#if UNITY_EDITOR
            MaxNumberOfUpdates = 1;
#endif
        }

        public void Init(LocationPath path, LineRenderer renderer)
        {
            Path = path;
            lineRenderer = renderer;

            arLocationRoot = ARLocationManager.Instance.gameObject.transform;

            initialized = true;

            locationProvider = ARLocationProvider.Instance;
            locationProvider.OnLocationUpdatedDelegate += locationUpdatedHandler;
            if (locationProvider.IsEnabled) {
                locationUpdatedHandler(locationProvider.CurrentLocation, locationProvider.CurrentLocation);
            }
        }

        private void locationUpdatedHandler(LocationReading locationReading, LocationReading _)
        {
            if (!initialized || updateCount >= MaxNumberOfUpdates)
            {
                return;
            }

            var points = new Vector3[Path.Locations.Length];
            var cameraTransform = ARLocationManager.Instance.MainCamera.transform;
            var location = locationReading.ToLocation();

            for (int i = 0; i < points.Length; i++)
            {
                var loc = Path.Locations[i];
                points[i] = Location.GetGameObjectPositionForLocation(arLocationRoot, ARLocationManager.Instance.MainCamera.transform, location, loc,  true);
                //points[i] = lineRenderer.gameObject.transform.worldToLocalMatrix.MultiplyPoint(points[i]);
                points[i].y = points[i].z;
                points[i].z = 0;
            }

            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = Path.Locations.Length;
            lineRenderer.SetPositions(points);
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.gameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);

            updateCount++;
        }

        void Update()
        {
            if (!initialized)
            {
                return;
            }

            lineRenderer.gameObject.transform.localPosition = MathUtils.SetY(lineRenderer.gameObject.transform.localPosition, Camera.main.transform.position.y - 1.5f);
        }

    }
}
