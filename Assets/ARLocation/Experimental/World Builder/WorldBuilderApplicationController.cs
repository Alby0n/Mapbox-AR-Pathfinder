using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace ARLocation
{
    [RequireComponent(typeof(WorldBuilder))]
    public class WorldBuilderApplicationController : MonoBehaviour
    {
        [System.Serializable]
        public class UiElementsSettings
        {
            public Button CubeBtn;
            public Button CylinderBtn;
            public Button LogoBtn;
            public Button MoveBtn;
            public Button RotateBtn;
            public Button DeselectBtn;
            public Button ClearWorldBtn;
            public Button HeightBtn;
            public Button DeleteObjectBtn;
            public Text DebugText;
        }

        [System.Serializable]
        public class RaycastMarginSettings
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;
        }

        [System.Serializable]
        public class GeneralSettings
        {
            public float HeightAdjustmentSensitivity = 0.25f;
            public float RotationAdjustmentSensitivity = 1.0f;
            public string SaveWorldUrl;
            public string RestoreWorldUrl;
            public bool SaveToServer;
        }

        public UiElementsSettings UiElements;
        public RaycastMarginSettings RaycastMargins;
        public GeneralSettings Settings;

        private WorldBuilder worldBuilder;
        private PlaceAtLocation selectedObject;
        private WorldBuilder.Entry selectedObjectEntry;

        enum AppState
        {
            PlacementMode,
            MoveMode,
            RotateMode,
            HeightMode,
            IdleMode
        };

        class State
        {
            public string CurrentMeshId;
            public AppState AppState;
        }

        private readonly State state = new State();

        private void Awake()
        {
            worldBuilder = GetComponent<WorldBuilder>();

            if (Settings.SaveToServer)
            {
                worldBuilder.UseLocalStorage = false;
            }

            Debug.Assert(worldBuilder != null);
            Debug.Assert(worldBuilder.PrefabDatabase.Entries.Count > 0);

            state.CurrentMeshId = "Cube";
            UiElements.CubeBtn.image.color = UiElements.CubeBtn.colors.pressedColor;

            SetObjectSelectedUIVisible(false);

            InitListeners();
        }

        IEnumerator Start()
        {
            if (Settings.SaveToServer)
            {

                while (!worldBuilder.Initialized)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                yield return RestoreWorldFromServer();
            }
        }

        IEnumerator RestoreWorldFromServer()
        {
            var request = UnityWebRequest.Get($"{Settings.RestoreWorldUrl}{worldBuilder.Id}");

            yield return request.SendWebRequest();

            if (Utils.Misc.WebRequestResultIsError(request))
            {
                Debug.Log("Failed to restore world from server!");
                yield break;
            }

            Debug.Log(request.downloadHandler.text);
            Debug.Log(request.responseCode);
            worldBuilder.FromJson(request.downloadHandler.text);
        }

        IEnumerator SaveWorldToServer()
        {
            var request = new UnityWebRequest($"{Settings.SaveWorldUrl}{worldBuilder.Id}", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(worldBuilder.ToJson());
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (Utils.Misc.WebRequestResultIsError(request))
            {
                Debug.LogWarning("Failed to save to server!");
            }

        }

        void SetObjectSelectedUIVisible(bool visible)
        {
            UiElements.MoveBtn.gameObject.SetActive(visible);
            UiElements.RotateBtn.gameObject.SetActive(visible);
            UiElements.DeselectBtn.gameObject.SetActive(visible);
            UiElements.HeightBtn.gameObject.SetActive(visible);
            UiElements.DeleteObjectBtn.gameObject.SetActive(visible);

            UiElements.CubeBtn.gameObject.SetActive(!visible);
            UiElements.CylinderBtn.gameObject.SetActive(!visible);
            UiElements.LogoBtn.gameObject.SetActive(!visible);
            UiElements.ClearWorldBtn.gameObject.SetActive(!visible);
        }

        void SetMoveMode()
        {
            UiElements.MoveBtn.image.color = UiElements.MoveBtn.colors.pressedColor;
            UiElements.RotateBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.DeselectBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.HeightBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            state.AppState = AppState.MoveMode;
        }

        void SetRotateMode()
        {
            UiElements.MoveBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.RotateBtn.image.color = UiElements.MoveBtn.colors.pressedColor;
            UiElements.DeselectBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.HeightBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            state.AppState = AppState.RotateMode;
        }

        private void SetHeightMode()
        {
            UiElements.MoveBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.RotateBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.DeselectBtn.image.color = UiElements.MoveBtn.colors.normalColor;
            UiElements.HeightBtn.image.color = UiElements.MoveBtn.colors.pressedColor;
            state.AppState = AppState.HeightMode;
        }

        void InitListeners()
        {
            UiElements.ClearWorldBtn.onClick.AddListener(() =>
            {
                worldBuilder.ClearWorld();
                SetObjectSelectedUIVisible(false);
                state.AppState = AppState.PlacementMode;
            });
            UiElements.CubeBtn.onClick.AddListener(() =>
            {
                UiElements.CubeBtn.image.color = UiElements.CubeBtn.colors.pressedColor;
                UiElements.CylinderBtn.image.color = UiElements.CylinderBtn.colors.normalColor;
                UiElements.LogoBtn.image.color = UiElements.LogoBtn.colors.normalColor;
                state.CurrentMeshId = "Cube";
            });

            UiElements.CylinderBtn.onClick.AddListener(() =>
            {
                UiElements.CubeBtn.image.color = UiElements.CubeBtn.colors.normalColor;
                UiElements.CylinderBtn.image.color = UiElements.CylinderBtn.colors.pressedColor;
                UiElements.LogoBtn.image.color = UiElements.LogoBtn.colors.normalColor;
                state.CurrentMeshId = "Cylinder";
            });

            UiElements.LogoBtn.onClick.AddListener(() =>
            {
                UiElements.CubeBtn.image.color = UiElements.CubeBtn.colors.normalColor;
                UiElements.CylinderBtn.image.color = UiElements.CylinderBtn.colors.normalColor;
                UiElements.LogoBtn.image.color = UiElements.LogoBtn.colors.pressedColor;
                state.CurrentMeshId = "Logo";
            });

            UiElements.DeselectBtn.onClick.AddListener(() =>
            {
                state.CurrentMeshId = "Cube";
                UiElements.CubeBtn.image.color = UiElements.CubeBtn.colors.pressedColor;
                UiElements.CylinderBtn.image.color = UiElements.CylinderBtn.colors.normalColor;
                UiElements.LogoBtn.image.color = UiElements.LogoBtn.colors.normalColor;
                state.AppState = AppState.PlacementMode;
                SetObjectSelectedUIVisible(false);
                selectedObjectEntry = null;
            });

            UiElements.RotateBtn.onClick.AddListener(() =>
            {
                SetRotateMode();
            });

            UiElements.MoveBtn.onClick.AddListener(() =>
            {
                SetMoveMode();
            });

            UiElements.HeightBtn.onClick.AddListener(() =>
            {
                SetHeightMode();
            });

            UiElements.DeleteObjectBtn.onClick.AddListener(() =>
            {
                worldBuilder.RemoveEntry(selectedObjectEntry);
            });
        }

        void DebugOutput(string text)
        {
            if (UiElements.DebugText)
            {
                UiElements.DebugText.text = text;
            }
        }

        void Update()
        {
            DebugOutput($" c = {ARLocationManager.Instance.MainCamera.transform.position}");
            if (Input.touchCount == 1)
            {
                var touch = Input.touches[0];

                if (state.AppState == AppState.RotateMode)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        float dx = Settings.RotationAdjustmentSensitivity * (180.0f / Mathf.PI) * touch.deltaPosition.x / Screen.width;

                        DebugOutput($"dx = {dx}");

                        if (selectedObjectEntry != null && selectedObjectEntry.Instance != null)
                        {
                            var euler = selectedObjectEntry.Instance.transform.localEulerAngles;
                            selectedObjectEntry.Rotation = euler.y + dx;
                            selectedObjectEntry.Instance.transform.localEulerAngles = new Vector3(euler.x, selectedObjectEntry.Rotation, euler.z);
                        }
                    }
                    return;
                }
                else if (state.AppState == AppState.HeightMode)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        float dy = Settings.HeightAdjustmentSensitivity * (180.0f / Mathf.PI) * touch.deltaPosition.y / Screen.height;

                        DebugOutput($"dy = {dy}");

                        if (selectedObjectEntry != null && selectedObjectEntry.Instance != null)
                        {
                            var Location = selectedObjectEntry.Instance.GetComponent<PlaceAtLocation>().Location.Clone();
                            Location.Altitude += dy;
                            selectedObjectEntry.Location.Altitude += dy;
                            selectedObjectEntry.Instance.GetComponent<PlaceAtLocation>().Location = Location;
                        }
                    }
                    return;
                }
                else if (touch.phase == TouchPhase.Began)
                {
                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(0)) return;

                    OnTouchOrClick(touch.position);
                }
            }

            if (Application.isEditor && Input.GetMouseButtonDown(0))
            {
                OnTouchOrClick(Input.mousePosition);
            }
        }

        private void OnTouchOrClick(Vector2 p)
        {
            float x = p.x / Screen.width;
            float y = p.y / Screen.height;

            if (x < RaycastMargins.Left || x > (1 - RaycastMargins.Right)) return;
            if (y < RaycastMargins.Bottom || y > (1 - RaycastMargins.Top)) return;

            var camera = Application.isEditor ? Camera.main : ARLocationManager.Instance.MainCamera;
            var ray = camera.ScreenPointToRay(p);


            if (state.AppState == AppState.PlacementMode)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject go = null;
                    WorldBuilder.Entry entry = null;
                    var o = hit.collider.transform;
                    while (o.parent)
                    {
                        Debug.Log(o.name);
                        entry = worldBuilder.GetWorld().Entries.Find(e => e.Instance == o.gameObject);

                        if (entry != null)
                        {
                            go = entry.Instance;
                            break;
                        }

                        o = o.parent;
                    }

                    if (go != null && entry != null)
                    {
                        selectedObjectEntry = entry;
                        SetObjectSelectedUIVisible(true);
                        SetMoveMode();
                        return;
                    }
                }
            }

            float enter;
            if (RaycastGround(ray, out enter))
            {
                var point = ray.GetPoint(enter);
                switch (state.AppState)
                {
                    case AppState.PlacementMode:
                        OnPlacementRaycast(point);
                        break;

                    case AppState.MoveMode:
                        OnMoveModeRaycast(point);
                        break;
                }
            }
        }

        private bool RaycastGround(Ray ray, out float t)
        {
            var arRaycastManager = FindObjectOfType<ARRaycastManager>();

            if (Application.isEditor || arRaycastManager == null)
            {
                var camera = Application.isEditor ? Camera.main : ARLocationManager.Instance.MainCamera;
                var plane = new Plane(new Vector3(0, 1, 0), camera.transform.position - new Vector3(0, 1.4f, 0));
                return plane.Raycast(ray, out t);
            }
            else
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                arRaycastManager.Raycast(ray, hits, trackableTypes: UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinInfinity);

                if (hits.Count > 0)
                {
                    t = hits[0].distance;
                    return true;
                }
                else
                {
                    var camera = Application.isEditor ? Camera.main : ARLocationManager.Instance.MainCamera;
                    var plane = new Plane(new Vector3(0, 1, 0), camera.transform.position - new Vector3(0, 1.4f, 0));
                    return plane.Raycast(ray, out t);
                }
            }
        }

        private void OnMoveModeRaycast(Vector3 point)
        {
            worldBuilder.MoveEntry(selectedObjectEntry, point);
        }

        private void OnPlacementRaycast(Vector3 v)
        {
            Debug.Log("ok!");
            worldBuilder.AddEntry(state.CurrentMeshId, v);

            if (Settings.SaveToServer)
            {
                Debug.Log("ok2!");
                StartCoroutine(SaveWorldToServer());
            }
        }
    }
}
