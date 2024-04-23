namespace ARLocation.MapboxRoutes.SampleProject
{
	//This script is a `DirectionsFactory` class responsible for creating and updating directions (routes) between waypoints on a map in a Unity environment using Mapbox services. 
	using UnityEngine;
	using Mapbox.Directions;
	using System.Collections.Generic;
	using Mapbox.Unity.Map;
    using Mapbox.Unity.MeshGeneration.Modifiers;
    using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
    using Mapbox.Unity;
	using System.Collections;
    using System.Linq;
//Overall, this script facilitates the dynamic generation and updating of route meshes between waypoints on a map in response to waypoint movements or updates.
	public class DirectionsFactory : MonoBehaviour
	{ //The `DirectionsFactory` class extends `MonoBehaviour`, indicating that it's a Unity component that can be attached to GameObjects.
		[SerializeField]
		//   - `_map`: A reference to the `AbstractMap` component, representing the map on which the directions will be displayed.
  // - `MeshModifiers`: An array of mesh modifiers to be applied to the generated route mesh.
   //- `_material`: The material to be used for rendering the route mesh.
   //- `_waypoints`: An array of transforms representing the waypoints between which directions will be calculated.
   //- `UpdateFrequency`: The frequency at which to update the directions query.
   //- `Layer`: The layer to assign to the created GameObject containing the route mesh.

		AbstractMap _map;

		[SerializeField]
		MeshModifier[] MeshModifiers;
		[SerializeField]
		Material _material;

		[SerializeField]
		Transform[] _waypoints;
		private List<Vector3> _cachedWaypoints;

		[SerializeField]
		[Range(1,10)]
		private float UpdateFrequency = 2;

        public int Layer;

		private Directions _directions;
		private int _counter;

		GameObject _directionsGO;
		private bool _recalculateNext;

		protected virtual void Awake()
		{ //   - In the `Awake` method, the script subscribes to map initialization and update events to trigger direction queries.
			if (_map == null)
			{
				_map = FindObjectOfType<AbstractMap>();
			}
			_directions = MapboxAccess.Instance.Directions;
			_map.OnInitialized += Query;
			_map.OnUpdated += Query;
		}

		public void Start()
		{ //In the `Start` method, waypoints' initial positions are cached, mesh modifiers are initialized, and a coroutine for the directions query timer is started.
			_cachedWaypoints = new List<Vector3>(_waypoints.Length);
			foreach (var item in _waypoints)
			{
				_cachedWaypoints.Add(item.position);
			}
			_recalculateNext = false;

			foreach (var modifier in MeshModifiers)
			{
				modifier.Initialize();
			}

			StartCoroutine(QueryTimer());
		}

		protected virtual void OnDestroy()
		{
			_map.OnInitialized -= Query;
			_map.OnUpdated -= Query;
		}

		void Query()
		{ //   - The `Query` method constructs a `DirectionResource` based on the waypoints' geo positions and queries directions from Mapbox's Directions API.
			var count = _waypoints.Length;
			var wp = new Vector2d[count];
			for (int i = 0; i < count; i++)
			{
				wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			}
			var _directionResource = new DirectionResource(wp, RoutingProfile.Driving);
			_directionResource.Steps = true;
			_directions.Query(_directionResource, HandleDirectionsResponse);
		}

		public IEnumerator QueryTimer()
		{ //The `QueryTimer` coroutine periodically checks if waypoints have moved and triggers a directions query if needed.
			while (true)
			{
				yield return new WaitForSeconds(UpdateFrequency);
				for (int i = 0; i < _waypoints.Length; i++)
				{
					if (_waypoints[i].position != _cachedWaypoints[i])
					{
						_recalculateNext = true;
						_cachedWaypoints[i] = _waypoints[i].position;
					}
				}

				if (_recalculateNext)
				{
					Query();
					_recalculateNext = false;
				}
			}
		}

		public void HandleDirectionsResponse(DirectionsResponse response)
		{ //The `HandleDirectionsResponse` method processes the response from the directions query, converts the route geometry to world positions, applies mesh modifiers, and creates a GameObject to represent the route.
			if (response == null || null == response.Routes || response.Routes.Count < 1)
			{
				return;
			}

			var meshData = new MeshData();
			var dat = new List<Vector3>();
			foreach (var point in response.Routes[0].Geometry)
			{
				dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
			}

			var feat = new VectorFeatureUnity();
			feat.Points.Add(dat);

			foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
			{
				mod.Run(feat, meshData, _map.WorldRelativeScale);
			}

			CreateGameObject(meshData);
		}

		public GameObject CreateGameObject(MeshData data)
		{ //- The `CreateGameObject` method generates a GameObject representing the route mesh based on the provided `MeshData`.
			if (_directionsGO != null)
			{
				_directionsGO.Destroy();
			}
			_directionsGO = new GameObject("direction waypoint " + " entity");
            _directionsGO.layer = Layer;
			var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
			mesh.subMeshCount = data.Triangles.Count;

			mesh.SetVertices(data.Vertices);
			_counter = data.Triangles.Count;
			for (int i = 0; i < _counter; i++)
			{
				var triangle = data.Triangles[i];
				mesh.SetTriangles(triangle, i);
			}

			_counter = data.UV.Count;
			for (int i = 0; i < _counter; i++)
			{
				var uv = data.UV[i];
				mesh.SetUVs(i, uv);
			}

			mesh.RecalculateNormals();
			_directionsGO.AddComponent<MeshRenderer>().material = _material;
			return _directionsGO;
		}
	}

}
