using UnityEngine;
using UnityEditor;

#if !ARGPS_USE_VUFORIA
using UnityEngine.XR.ARFoundation;
#endif

namespace ARLocation
{
    public static class GameObjectMenuItems{

        [MenuItem("GameObject/AR+GPS/ARLocationRoot", false, 20)]
        public static void CreateARLocationRoot()
        {
            var go = new GameObject("ARLocationRoot");

            go.AddComponent<ARLocationManager>();
            go.AddComponent<ARLocationProvider>();

            var arSessionOrigin = GameObject.Find("AR Session Origin");

            if (arSessionOrigin != null)
            {
                go.transform.SetParent(arSessionOrigin.transform);
            }
        }

        [MenuItem("GameObject/AR+GPS/GPS Stage Object", false, 20)]
        public static GameObject CreateGpsStageObject()
        {
            var go = new GameObject("GPS Stage Object");

            go.AddComponent<PlaceAtLocation>();

            return go;
        }

        [MenuItem("GameObject/AR+GPS/GPS Hotspot Object", false, 20)]
        public static GameObject CreateGpsHotspotObject()
        {
            var go = new GameObject("GPS Hotspot Object");

            go.AddComponent<Hotspot>();

            return go;
        }

#if ARGPS_DEV_MODE
        [MenuItem("Assets/Print GUID", false)]
        public static void PrintGuit()
        {
            foreach (var s in Selection.assetGUIDs)
            {
                Debug.Log(s);
            }
        }
#endif

        [MenuItem("GameObject/AR+GPS/Create Basic Scene Structure", false, 20)]
        public static void CreateBasicScene()
        {
#if ARGPS_USE_VUFORIA
            EditorApplication.ExecuteMenuItem("GameObject/Vuforia Engine/AR Camera");
            Selection.activeObject = null;
            EditorApplication.ExecuteMenuItem("GameObject/Vuforia Engine/Ground Plane/Plane Finder");

            CreateARLocationRoot();
            var stage = CreateGpsStageObject();

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(stage.transform);
#else
            EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session");
            Selection.activeObject = null;
            EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session Origin");
            Selection.activeObject = null;
            EditorApplication.ExecuteMenuItem("GameObject/AR+GPS/ARLocationRoot");

            var prevMain = GameObject.FindWithTag("MainCamera");
            if (prevMain)
            {
                Object.DestroyImmediate(prevMain);
            }

            var cam = GameObject.Find("AR Camera");

            if (cam)
            {
                cam.tag = "MainCamera";
                var camera = cam.GetComponent<Camera>();
                camera.farClipPlane = 1000.0f;
            }

            var arSessionOrigin = Object.FindObjectOfType<ARSessionOrigin>().gameObject;
            arSessionOrigin.AddComponent<ARPlaneManager>();

            var stage = CreateGpsStageObject();
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(stage.transform);
#endif
        }
    }
}
