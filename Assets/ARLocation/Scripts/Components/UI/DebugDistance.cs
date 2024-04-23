using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARLocation.UI
{
    public class DebugDistance : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Camera mainCamera;
        private TextMesh textMesh;
        private GameObject textMeshGo;
        private ARLocationManager arLocationManager;
        private bool hasArLocationManager;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = ARLocationManager.Instance.MainCamera;
            lineRenderer = GetComponent<LineRenderer>();
            arLocationManager = ARLocationManager.Instance;
            hasArLocationManager = arLocationManager != null;

            if (!lineRenderer)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();

                var shader = Shader.Find("Unlit/Color");
                if (shader)
                {
                    lineRenderer.material = new Material(shader)
                    {
                        color = new Color(0.3960f, 0.6901f, 0.9725f)
                    };
                }
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.allowOcclusionWhenDynamic = false;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            textMeshGo = new GameObject(gameObject.name + "_text");
            textMeshGo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            textMesh = textMeshGo.AddComponent<TextMesh>();
            textMesh.fontSize = 100;
        }
        void Update()
        {

            var floorLevel = hasArLocationManager ? arLocationManager.CurrentGroundY : -ARLocation.Config.InitialGroundHeightGuess;
            var startPos = MathUtils.SetY(mainCamera.transform.position, floorLevel);
            var endPos = MathUtils.SetY(transform.position, floorLevel);

            var lineDir = (endPos - startPos).normalized;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            var textPos = startPos + lineDir * 6.0f;

            textMeshGo.transform.position = textPos;
            textMeshGo.transform.LookAt(endPos, new Vector3(0, 1, 0));
            textMeshGo.transform.Rotate(90, 90, 0);
            textMesh.text = Vector3.Distance(startPos, endPos).ToString("0.00", CultureInfo.InvariantCulture) + "m";
        }
    }

}
