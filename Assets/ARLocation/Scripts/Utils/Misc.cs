using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if !ARGPS_USE_VUFORIA
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace ARLocation.Utils
{
    public class Misc
    {
        public static bool IsARDevice()
        {
            return (
                Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer
            );
        }

        public static bool WebRequestResultIsError(UnityWebRequest request)
        {
#if UNITY_2020_3_OR_NEWER
            return (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError);
#else
            return (request.isNetworkError || request.isHttpError);
#endif
        }

#if !ARGPS_USE_VUFORIA
        public static void RequestPlaneDetectionMode(ARPlaneManager manager, PlaneDetectionMode mode)
        {
            var managerType = manager.GetType();
            var requestedDetectionModeProp = managerType.GetProperty("requestedDetectionMode");
            if (requestedDetectionModeProp != null)
            {
                requestedDetectionModeProp.SetValue(manager, mode);
            }
            else
            {
                var detectionModeProp = managerType.GetProperty("detectionMode");
                if (detectionModeProp != null)
                {
                    detectionModeProp.SetValue(manager, mode);
                }
                else
                {
                    throw new System.Exception("[ARGPS][RequestPlaneDetectionMode]: Failed to set detection mode!");
                }
            }
        }
#endif

        public static float FloatListAverage(List<float> list)
        {
            var average = 0.0f;

            foreach (var value in list)
            {
                average += value;
            }

            return average / list.Count;

        }

        public static float GetNormalizedDegrees(float value)
        {
            if (value < 0)
            {
                return (360 + (value % 360));
            }

            return value % 360;
        }

        public static T FindAndGetComponent<T>(string name)
        {
            var gameObject = GameObject.Find(name);

            if (gameObject == null)
            {
                return default(T);
            }

            return gameObject.GetComponent<T>();
        }

        public static T FindAndGetComponentAndLogError<T>(string name, string message)
        {
            var result = FindAndGetComponent<T>(name);

            if (EqualityComparer<T>.Default.Equals(result, default(T)))
            {
                Debug.LogError(message);
            }

            return result;
        }

        public static GameObject FindAndLogError(string name, string message)
        {
            var go = GameObject.Find(name);

            if (go == null)
            {
                Debug.LogError(message);
            }

            return go;
        }

        public static Spline BuildSpline(SplineType type, Vector3[] points, int n, float alpha)
        {
            if (type == SplineType.CatmullromSpline)
            {
                return new CatmullRomSpline(points, n, alpha);
            }
            else
            {
                return new LinearSpline(points);
            }
        }

        public static void SetActiveOnAllChildren(GameObject go, bool value)
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetActive(value);
            }
        }

        public static void SetGameObjectVisible(GameObject go, bool value)
        {
            var meshRenderer = go.GetComponent<MeshRenderer>();
            var skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();

            if (meshRenderer)
            {
                meshRenderer.enabled = value;
            }

            if (skinnedMeshRenderer)
            {
                skinnedMeshRenderer.enabled = value;
            }

            SetActiveOnAllChildren(go, value);
        }

        public static void HideGameObject(GameObject go)
        {
            SetGameObjectVisible(go, false);
        }

        public static void ShowGameObject(GameObject go)
        {
            SetGameObjectVisible(go, true);
        }

        public static void SetTransformPositionY(Transform t, float y)
        {
            var p = t.position;
            p.y = y;
            t.position = p;
        }
    }
}
