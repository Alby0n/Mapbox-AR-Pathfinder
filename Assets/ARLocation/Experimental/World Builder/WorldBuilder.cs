using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARLocation
{
    public class WorldBuilder : MonoBehaviour
    {
        public PrefabDatabase PrefabDatabase;
        public string Id = "World";
        public bool UseLocalStorage;

        [System.Serializable]
        public class Entry
        {
            public Location Location;
            public float Rotation;
            public string MeshId;

            [System.NonSerialized]
            public GameObject Instance;
        }

        [System.Serializable]
        public class World
        {
            public List<Entry> Entries = new List<Entry>();
        }

        class State
        {
            public World World = new World();
        }

        private readonly State state = new State();

        public World GetWorld()
        {
            return state.World;
        }

        private bool initialized;

        public bool Initialized => initialized;

        IEnumerator Start()
        {
            yield return StartCoroutine(WaitForLocationServices());

            if (UseLocalStorage)
            {
                RestoreWorldFromLocalStorage();
            }

            initialized = true;
        }

        IEnumerator WaitForLocationServices()
        {
            while (!ARLocationProvider.Instance.IsEnabled)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void AddEntry(string meshId, Vector3 worldPosition, float rotation = 0)
        {
            var location = ARLocationManager.Instance.GetLocationForWorldPosition(worldPosition);
            var entry = new Entry { Location = location, MeshId = meshId, Rotation = rotation };
            state.World.Entries.Add(entry);
            PlaceEntryAtLocation(entry);
        }

        void OnDestroy()
        {
            SaveWorld();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) SaveWorld();
        }

        void PlaceWorld()
        {
            state.World.Entries.ForEach(PlaceEntryAtLocation);
        }

        void PlaceEntryAtLocation(Entry entry)
        {
            var prefab = PrefabDatabase.GetEntryById(entry.MeshId);

            if (prefab == null)
            {
                Debug.LogWarning($"Invalid prefab '{entry.MeshId}'");
                return;
            }

            var location = entry.Location;
            var options = new PlaceAtLocation.PlaceAtOptions { };

            entry.Instance = PlaceAtLocation.CreatePlacedInstance(prefab, location, options, false);
            entry.Instance.transform.localEulerAngles = new Vector3(0, entry.Rotation, 0);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(state.World);
        }

        public void FromJson(string json)
        {
            ClearWorld();
            state.World = JsonUtility.FromJson<World>(json);
            PlaceWorld();
        }

        string GetJsonFilename()
        {
            var s = Id.Trim();

            if (s == "") s = "World";

            return Application.persistentDataPath + "/" + s + ".json";
        }

        public void SaveWorld()
        {
            if (!UseLocalStorage) return;

            string json = JsonUtility.ToJson(state.World);
            Debug.Log(json);

            try
            {
                System.IO.File.WriteAllText(GetJsonFilename(), json);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::SaveWorld]: Failed to open json file for writing.");
                return;
            }

            Debug.Log($"Written to json file '{GetJsonFilename()}'");
        }

        public void RestoreWorldFromLocalStorage()
        {
            if (!UseLocalStorage) return;

            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(GetJsonFilename(), System.Text.Encoding.UTF8);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::RestoreWorld]: Failed to open json file for reading.");
                return;
            }

            state.World = JsonUtility.FromJson<World>(json);
            PlaceWorld();
            Debug.Log($"Restored world from json file '{GetJsonFilename()}'");
        }

        public void ClearWorld()
        {
            state.World.Entries.ForEach(e => {
                if (e.Instance != null)
                {
                    Destroy(e.Instance);
                }
            });
            state.World = new World();
        }

        internal void MoveEntry(Entry entry, Vector3 point)
        {
            var location = ARLocationManager.Instance.GetLocationForWorldPosition(point);
            entry.Location = location;

            if (entry.Instance != null)
            {
                var placeAt = entry.Instance.GetComponent<PlaceAtLocation>();

                if (placeAt != null)
                {
                    placeAt.Location = location;
                }
            }
        }

        public void RemoveEntry(Entry entry)
        {
            Destroy(entry.Instance);
            state.World.Entries.Remove(entry);
        }
    }
}
