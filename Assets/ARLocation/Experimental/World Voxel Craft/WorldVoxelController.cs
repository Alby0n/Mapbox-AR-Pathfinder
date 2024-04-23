using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARLocation
{
    public class WorldVoxelController : MonoBehaviour
    {
        // Start is called before the first frame update
        public PrefabDatabase PrefabDatabase;

        [System.Serializable]
        class Voxel
        {
            public string PrefabId;
            public int i, j, k;

            [System.NonSerialized]
            public GameObject Instance;
        }

        struct VoxelHit
        {
            public Voxel Voxel;
            public Vector3 Normal;
            public Vector3 WorldNorma;
        }

        [System.Serializable]
        class WorldChunk
        {
            public List<Voxel> Voxels = new List<Voxel>();
            public Location ChunkLocation;
            public float ChunkRotation;
            public bool HasLocation;
            public int Length;

            [System.NonSerialized]
            public Vector3 Origin;

            [System.NonSerialized]
            public GameObject ChunkContainer;

            [System.NonSerialized]
            public Bounds Bounds;

            //[System.NonSerialized]
            //public GameObject ChunkPlaneInstance;

            [System.NonSerialized]
            public bool IsFresh;
        }

        [System.Serializable]
        class World
        {
            public List<WorldChunk> Chunks = new List<WorldChunk>();
        }

        [System.Serializable]
        public class ElementsSettingsData
        {
            public Button ClearWorldBtn;
            public Button PickAxeBtn;
            public Button BrickBtn;
            public Button StoneBtn;
            public Button GoldBtn;
            public Text DebugText;
            public AudioClip Create;
            public AudioClip Destroy;
            public ParticleSystem BrickParticle;
            public GameObject IndicatorPlane;
            public GameObject ChunkPlanePrefab;
        }

        public enum Tools
        {
            PickAxe,
            Block
        }

        public enum Blocks
        {
            Brick,
            Stone,
            Gold
        }

        [System.Serializable]
        public class RaycastMarginSettings
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;

            public bool IsInside(Vector2 v)
            {
                if (v.x < Left || v.x > (1 - Right)) return false;
                if (v.y < Bottom || v.y > (1 - Top)) return false;

                return true;
            }
        }

        [System.Serializable]
        public class SettingsData
        {
            public float CunkScale = 1.0f;
            public Color ButtonNormalColor;
            public Color ButtonSelectedColor;
        }

        class StateData
        {
            public ApplicationState AppState;
            public GameState GameState;
            public Blocks CurrentBlock;
            public WorldChunk CurrentChunk;
            public Location CurrentLocation;
        }

        public ElementsSettingsData Elements;
        public RaycastMarginSettings RaycastMargins;
        public SettingsData Settings;

        private World world = new World();
        private readonly StateData state = new StateData();

        private void LogText(string str)
        {
            Debug.Log(str);
            Elements.DebugText.text = str;
        }

        enum ApplicationState
        {
            Initializing,
            Running
        };

        enum GameState
        {
            Destroy,
            Build
        };

        private IEnumerator StartWorld()
        {
            LogText($"Loading previous session file...");
            yield return new WaitForSeconds(0.5f);

            if (RestoreWorldFromLocalStorage())
            {
                LogText($"Restored world with {world.Chunks.Count} chunks");
                yield return new WaitForSeconds(0.5f);

                if (world.Chunks.Count > 0)
                {
                    double distance;
                    var closestChunk = FindClosestChunk(state.CurrentLocation, out distance, 1000.0);
                    if (closestChunk != null)
                    {
                        LogText($"Found closes chunk at {closestChunk.ChunkLocation}, d = {distance}");
                        yield return new WaitForSeconds(0.5f);

                        SetCurrentChunk(closestChunk);
                        LogText($"Current Chunk Set");
                        yield return new WaitForSeconds(0.5f);

                        yield break;
                    }
                    else
                    {
                        LogText($"No chunk nearby!");
                        yield return new WaitForSeconds(0.5f);

                        var i = 0;
                        foreach (var c in world.Chunks)
                        {
                            var d = Location.HorizontalDistance(state.CurrentLocation, c.ChunkLocation);
                            LogText($"Chunk {i} at {c.ChunkLocation}, d = {d}");
                            yield return new WaitForSeconds(0.5f);
                            i++;
                        }
                    }
                }
            }
            else
            {
                LogText($"No world to restore!");
                yield return new WaitForSeconds(0.5f);
            }

            LogText("Creating new chunk...");
            yield return new WaitForSeconds(0.5f);

            var chunk = CreateTestChunk();
            chunk.IsFresh = true;
            world.Chunks.Add(chunk);
            SetCurrentChunk(chunk);
            UpdateChunkLocation(chunk);

            LogText("Added new chunk!");
            yield return new WaitForSeconds(0.5f);
        }


        private IEnumerator Start()
        {
            Utils.Misc.HideGameObject(Elements.IndicatorPlane);
            ARLocationProvider.Instance.OnLocationUpdated.AddListener(OnLocationUpdatedListener);
            ChooseBrick();

            LogText("Starting...");
            LogText("Waiting for location...");
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(WaitForLocationServices());
            state.CurrentLocation = ARLocationProvider.Instance.CurrentLocation.ToLocation();
            LogText($"Location enabled: {state.CurrentLocation}");
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(StartWorld());

            LogText($"Starting UI Listeners...");
            yield return new WaitForSeconds(0.5f);
            InitUiListeners();

            LogText($"Setting app running...");
            yield return new WaitForSeconds(0.5f);
            state.AppState = ApplicationState.Running;

            LogText($"App is running!");
        }

        void SetCurrentChunk(WorldChunk chunk)
        {
            if ((state.CurrentChunk != null) && (state.CurrentChunk.ChunkContainer != null)) Destroy(state.CurrentChunk.ChunkContainer);

            state.CurrentChunk = chunk;

            if (chunk.ChunkContainer == null)
            {
                BuildChunk(chunk);
            }
        }

        void AddVoxelToChunk(WorldChunk c, string PrefabId, int i, int j, int k)
        {
            Voxel v = new Voxel { PrefabId = PrefabId, i = i, j = j, k = k };
            v.Instance = Instantiate(PrefabDatabase.GetEntryById(PrefabId), c.ChunkContainer.transform);
            v.Instance.transform.localPosition = new Vector3(v.i, v.j, v.k);
            c.Voxels.Add(v);
        }

        WorldChunk CreateDefaultChunk()
        {
            return new WorldChunk { Origin = new Vector3(0, -1.4f, 4), Length = 100 };
        }

        WorldChunk CreateTestChunk()
        {
            WorldChunk c = new WorldChunk
            {
                Voxels = new List<Voxel>
                {
                    new Voxel
                    {
                        PrefabId = "Brick",
                        Instance = null,
                        i = 0, j = 0, k = 0
                    }
                },
                Origin = new Vector3(0, -1.4f, 4),
                Length = 100
            };

            for (int i = 1; i < 10; i++)
            {
                c.Voxels.Add(new Voxel { PrefabId = "Brick", i = i, j = 0, k = 0 });
            }

            for (int i = 1; i < 10; i++)
            {
                c.Voxels.Add(new Voxel { PrefabId = "Brick", i = i, j = 0, k = 9 });
            }

            for (int i = 1; i < 10; i++)
            {
                c.Voxels.Add(new Voxel { PrefabId = "Brick", i = 0, j = 0, k = i });
            }

            for (int i = 1; i < 10; i++)
            {
                c.Voxels.Add(new Voxel { PrefabId = "Brick", i = 9, j = 0, k = i });
            }

            return c;
        }

        IEnumerator WaitForLocationServices()
        {
            while (!ARLocationProvider.Instance.IsEnabled)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        bool InputIsDown(out Vector2 pos)
        {
            if (Application.isEditor)
            {
                pos = Input.mousePosition;
                return Input.GetMouseButtonDown(0) && RaycastMargins.IsInside(pos / new Vector2(Screen.width, Screen.height));
            }

            if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
            {
                pos = Input.touches[0].position;
                return RaycastMargins.IsInside(pos / new Vector2(Screen.width, Screen.height));
            }

            pos = new Vector3();
            return false;
        }

        private void Update()
        {
            if (state.AppState == ApplicationState.Initializing) return;

            if (state.CurrentChunk == null)
            {
                FindClosestChunkOrCreateNew(state.CurrentLocation);
                return;
            }

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            VoxelHit hit;
            if (RaycastChunk(ray, state.CurrentChunk, out hit))
            {
                Utils.Misc.ShowGameObject(Elements.IndicatorPlane);
                var t = Elements.IndicatorPlane.transform;
                t.SetParent(state.CurrentChunk.ChunkContainer.transform);
                t.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                t.transform.localEulerAngles = new Vector3(0, 0, 0);
                t.transform.localPosition = new Vector3(hit.Voxel.i + Mathf.FloorToInt(hit.Normal.x) * 0.505f, hit.Voxel.j + Mathf.FloorToInt(hit.Normal.y) * 0.505f, hit.Voxel.k + Mathf.FloorToInt(hit.Normal.z) * 0.505f);
                //LogText("" + hit.Normal);
                var Normal = hit.Normal;

                if (Mathf.Abs(Normal.x) < 0.0001f && Mathf.Abs(Normal.y) < 0.0001f)
                {
                    float sign = Normal.z < 0 ? -1.0f : 1.0f;
                    Elements.IndicatorPlane.transform.localEulerAngles = new Vector3(sign * 90.0f, 0, 0);
                }
                else if (Mathf.Abs(Normal.z) < 0.0001f && Mathf.Abs(Normal.y) < 0.0001f)
                {
                    float sign = Normal.x < 0 ? 1.0f : -1.0f;
                    Elements.IndicatorPlane.transform.localEulerAngles = new Vector3(0, 0, sign * 90.0f);
                }
            }
            else
            {
                Utils.Misc.HideGameObject(Elements.IndicatorPlane);
                return;
            }

            Vector2 touchPos;
            bool isDown = InputIsDown(out touchPos);

            switch (state.GameState)
            {
                case GameState.Destroy:
                    {
                        if (isDown && hit.Voxel.Instance != null)
                        { 
                            var i = Instantiate(Elements.BrickParticle);
                            i.transform.position = hit.Voxel.Instance.transform.position;
                            i.GetComponent<ParticleSystemRenderer>().material = hit.Voxel.Instance.GetComponent<MeshRenderer>().material;
                            i.Play();

                            state.CurrentChunk.Voxels.Remove(hit.Voxel);
                            Destroy(hit.Voxel.Instance);


                            var a = Camera.main.GetComponent<AudioSource>();
                            if (a)
                            {
                                a.clip = Elements.Destroy;
                                a.Play();
                            }
                        }
                    }
                    break;
                case GameState.Build:
                    {
                        if (isDown)
                        {
                            AddVoxelToChunk(state.CurrentChunk, GetMeshIdForBlock(state.CurrentBlock), hit.Voxel.i + Mathf.FloorToInt(hit.Normal.x), hit.Voxel.j + Mathf.FloorToInt(hit.Normal.y), hit.Voxel.k + Mathf.FloorToInt(hit.Normal.z));
                            var a = Camera.main.GetComponent<AudioSource>();
                            if (a)
                            {
                                a.clip = Elements.Create;
                                a.Play();
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        bool RaycastChunk(Ray ray, WorldChunk chunk, out VoxelHit hit)
        {
            //if (!chunk.Bounds.IntersectRay(ray)) {
            //    hit = new VoxelHit();
            //    return false;
            //}

            VoxelHit currentHit = new VoxelHit();
            float currentDistance = 0;
            bool hasHit = false;
            foreach (var v in chunk.Voxels)
            {
                if (v.Instance)
                {
                    var collider = v.Instance.GetComponent<BoxCollider>();

                    Debug.Assert(collider);

                    RaycastHit h;
                    if (collider.Raycast(ray, out h, chunk.Length))
                    {
                        if (!hasHit || h.distance < currentDistance)
                        {
                            hasHit = true;
                            currentDistance = h.distance;
                            currentHit = new VoxelHit { Voxel = v, WorldNorma = h.normal, Normal = chunk.ChunkContainer.transform.InverseTransformDirection(h.normal) };
                        }
                    }
                }
            }

            hit = currentHit;

            if (hasHit)
            {
                hit = currentHit;
                return true;
            }
            else
            {
                var chunkOrigin = chunk.ChunkContainer.transform.position;
                var plane = new Plane(new Vector3(0, 1, 0), -chunkOrigin.y + 0.5f * Settings.CunkScale);
                float d;
                if (plane.Raycast(ray, out d))
                {
                    var p = chunk.ChunkContainer.transform.InverseTransformPoint(ray.GetPoint(d));

                    var i = Mathf.FloorToInt(p.x + 0.5f);
                    var j = -1;
                    var k = Mathf.FloorToInt(p.z + 0.5f);

                    hit = new VoxelHit { Voxel = new Voxel { PrefabId = "", i = i, j = j, k = k }, Normal = new Vector3(0, 1, 0) };

                    return true;
                }
            }

            return false;
        }

        bool RestoreWorldFromLocalStorage()
        {
            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(GetJsonFilename(), System.Text.Encoding.UTF8);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::RestoreWorld]: Failed to open json file for reading.");
                //RandomPopulateWorld();
                return false;
            }

            world = JsonUtility.FromJson<World>(json);
            Debug.Log($"Restored world from json file '{GetJsonFilename()}'");

            return true;
        }

        string GetJsonFilename()
        {
            var s = "WorldVoxelCraft";

            return Application.persistentDataPath + "/" + s + ".json";
        }

        WorldChunk FindClosestChunk(Location l, out double distance, double maxDistance)
        {
            WorldChunk current = null;
            double currentDistance = 0;

            foreach (var c in world.Chunks)
            {
                var d = Location.HorizontalDistance(c.ChunkLocation, l);

                if (current == null || d < currentDistance)
                {
                    current = c;
                    currentDistance = d;
                }
            }

            distance = currentDistance;

            if (currentDistance > maxDistance) return null;

            return current;
        }

        void BuildChunk(WorldChunk chunk)
        {
            chunk.ChunkContainer = new GameObject();
            chunk.ChunkContainer.transform.localScale = new Vector3(Settings.CunkScale, Settings.CunkScale, Settings.CunkScale);
            if (chunk.HasLocation)
            {
                PlaceAtLocation.AddPlaceAtComponent(chunk.ChunkContainer, chunk.ChunkLocation, new PlaceAtLocation.PlaceAtOptions { MaxNumberOfLocationUpdates = 2 });
                chunk.ChunkContainer.transform.localEulerAngles = new Vector3(0, chunk.ChunkRotation, 0);
            }
            else
            {
                chunk.ChunkContainer.transform.position = chunk.Origin;
                chunk.Bounds = new Bounds(chunk.Origin, new Vector3(chunk.Length, chunk.Length, chunk.Length));
                chunk.ChunkContainer.AddComponent<GroundHeight>();
            }

            foreach (var v in chunk.Voxels)
            {
                v.Instance = Instantiate(PrefabDatabase.GetEntryById(v.PrefabId), chunk.ChunkContainer.transform);
                v.Instance.transform.localPosition = new Vector3(v.i, v.j, v.k);
            }

            //if (Elements.ChunkPlanePrefab != null)
            //{
            //    chunk.ChunkPlaneInstance = Instantiate(Elements.ChunkPlanePrefab, chunk.ChunkContainer.transform);
            //    chunk.ChunkPlaneInstance.transform.localPosition = new Vector3(0, -0.5f, 0);
            //    //chunk.ChunkPlaneInstance.transform.localScale = new Vector3(5, 1, 5);
            //    chunk.ChunkPlaneInstance.transform.localScale = new Vector3(chunk.Length/10, 1, chunk.Length/10);
            //    //chunk.ChunkPlaneInstance.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(50, 50);
            //    chunk.ChunkPlaneInstance.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(chunk.Length, chunk.Length);
            //    chunk.ChunkPlaneInstance.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(0.5f, 0.5f);
            //}
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveWorldToLocalStorage();
        }

        private void OnDestroy()
        {
            SaveWorldToLocalStorage();
        }

        void SaveWorldToLocalStorage()
        {
            var json = JsonUtility.ToJson(world);

            try
            {
                System.IO.File.WriteAllText(GetJsonFilename(), json);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::SaveWorld]: Failed to open json file for writing.");
                return;
            }

            Debug.Log("Saved " + GetJsonFilename());
        }

        private void OnLocationUpdatedListener(Location l)
        {

            if (state.AppState != ApplicationState.Running) return;

            state.CurrentLocation = l;

            FindClosestChunkOrCreateNew(l);

            if (state.CurrentChunk != null)
            {
                UpdateChunkLocation(state.CurrentChunk);
            }

        }

        private void FindClosestChunkOrCreateNew(Location l)
        {
            double distance;
            var newClosestChunk = FindClosestChunk(l, out distance, 1000.0f);
            if (newClosestChunk != state.CurrentChunk && newClosestChunk != null)
            {
                SetCurrentChunk(newClosestChunk);
            }
            
            if (state.CurrentChunk == null)
            {
                SetCurrentChunk(CreateDefaultChunk());
            }

        }

        void UpdateChunkLocation(WorldChunk c)
        {
            if (!c.IsFresh) return;

            c.ChunkLocation = ARLocationManager.Instance.GetLocationForWorldPosition(c.ChunkContainer.transform.position);
            c.ChunkLocation.Altitude = 0;
            c.ChunkLocation.AltitudeMode = AltitudeMode.GroundRelative;
            var arLocationRoot = ARLocationManager.Instance.gameObject.transform;
            float angle = Vector3.SignedAngle(c.ChunkContainer.transform.forward, arLocationRoot.forward, new Vector3(0, 1, 0));
            c.ChunkRotation = angle;
            c.HasLocation = true;

            LogText($"Updated chunk location to {c.ChunkLocation}");
        }

        private void InitUiListeners()
        {
            Elements.ClearWorldBtn.onClick.AddListener(() =>
            {
                ClearWorld();
            });

            Elements.PickAxeBtn.onClick.AddListener(() =>
            {
                ChoosePickaxe();
            });

            Elements.BrickBtn.onClick.AddListener(() =>
            {
                ChooseBrick();
            });

            Elements.StoneBtn.onClick.AddListener(() =>
            {
                ChooseStone();
            });

            Elements.GoldBtn.onClick.AddListener(() =>
            {
                ChooseGold();
            });
        }

        void ChooseBrick()
        {
            state.GameState = GameState.Build;
            state.CurrentBlock = Blocks.Brick;

            Elements.BrickBtn.GetComponent<Image>().color = Settings.ButtonSelectedColor;
            Elements.GoldBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.StoneBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.PickAxeBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
        }

        void ChooseGold()
        {
            state.GameState = GameState.Build;
            state.CurrentBlock = Blocks.Gold;

            Elements.BrickBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.GoldBtn.GetComponent<Image>().color = Settings.ButtonSelectedColor;
            Elements.StoneBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.PickAxeBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
        }

        void ChooseStone()
        {
            state.GameState = GameState.Build;
            state.CurrentBlock = Blocks.Stone;

            Elements.BrickBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.GoldBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.StoneBtn.GetComponent<Image>().color = Settings.ButtonSelectedColor;
            Elements.PickAxeBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
        }

        void ChoosePickaxe()
        {
            state.GameState = GameState.Destroy;
            

            Elements.BrickBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.GoldBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.StoneBtn.GetComponent<Image>().color = Settings.ButtonNormalColor;
            Elements.PickAxeBtn.GetComponent<Image>().color = Settings.ButtonSelectedColor;
        }

        void ClearChunk(WorldChunk chunk)
        {
            if (chunk.ChunkContainer != null)
            {
                Elements.IndicatorPlane.transform.SetParent(null);
                Destroy(chunk.ChunkContainer);
            }

            chunk.Voxels = new List<Voxel>();
        }

        void ClearWorld()
        {
            world.Chunks.ForEach(ClearChunk);
            world.Chunks = new List<WorldChunk>();
            state.CurrentChunk = null;
            LogText("World cleared!");
        }

        private string GetMeshIdForBlock(Blocks b)
        {
            switch (b)
            {
                case Blocks.Brick:
                    return "Brick";
                case Blocks.Stone:
                    return "Stone";
                case Blocks.Gold:
                    return "Gold";
                default:
                    return "";
            }
        }
    }
}
