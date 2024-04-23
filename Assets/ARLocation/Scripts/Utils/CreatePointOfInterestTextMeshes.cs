using System.Collections;
using System.Globalization;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
// ReSharper disable InconsistentNaming
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace ARLocation.Utils
{

    public class POIData
    {
        public Location location;
        public string name;
    }

    [System.Serializable]
    public class OverpassRequestData
    {
        [Tooltip("The SouthWest end of the bounding box.")]
        public Location SouthWest;

        [Tooltip("The NorthEast end of the bounding box.")]
        public Location NorthEast;
    }

    [System.Serializable]
    public class OpenStreetMapOptions
    {
        [Tooltip("A XML with the results of a Overpass API query. You can use http://overpass-turbo.eu/ to generate one.")]
        public TextAsset OsmXmlFile;

        [Tooltip("If true, instead of the XML file above, we fetch the data directly from a Overpass API request to https://www.overpass-api.de/api/interpreter.")]
        public bool FetchFromOverpassApi;

        [Tooltip("The data configuration used for the Overpass API request. Basically a bounding box rectangle defined by two points.")]
        public OverpassRequestData overPassRequestData;
    }

    public class CreatePointOfInterestTextMeshes : MonoBehaviour
    {
        [Tooltip("The height of the text mesh, relative to the device.")]
        public float height = 1f;

        [Tooltip("The TextMesh prefab.")]
        public TextMesh textPrefab;

        [Tooltip("The smoothing factor for movement due to GPS location adjustments; if set to zero it is disabled."), Range(0, 500)]
        public float movementSmoothingFactor = 100.0f;

        [Tooltip("Locations where the text will be displayed. The text will be the Label property. (Optional)")]
        public Location[] locations;

        [Tooltip("Use this to either fetch OpenStreetMap data from a Overpass API request, or via a locally stored XML file. (Optional)")]
        public OpenStreetMapOptions openStreetMapOptions;


        POIData[] poiData;
        // List<ARLocationManagerEntry> entries = new List<ARLocationManagerEntry>();
        string xmlFileText;

        // Use this for initialization
        void Start()
        {
            // CreateTextObjects();

            AddLocationsPOIs();

            if (openStreetMapOptions.FetchFromOverpassApi && openStreetMapOptions.overPassRequestData != null)
            {
                StartCoroutine(nameof(LoadXMLFileFromOverpassRequest));
            }
            else if (openStreetMapOptions.OsmXmlFile != null)
            {
                LoadXMLFileFromTextAsset();
            }

        }

        private void LoadXMLFileFromTextAsset()
        {
            CreateTextObjects(openStreetMapOptions.OsmXmlFile.text);
        }

        string GetOverpassRequestURL(OverpassRequestData data)
        {
            var lat1 = data.SouthWest.Latitude;
            var lng1 = data.SouthWest.Longitude;
            var lat2 = data.NorthEast.Latitude;
            var lng2 = data.NorthEast.Longitude;

            return "https://www.overpass-api.de/api/interpreter?data=[out:xml];node[amenity](" + lat1 + "," + lng1 + "," + lat2 + "," + lng2 + ");out%20meta;";
        }

        IEnumerator LoadXMLFileFromOverpassRequest()
        {
            var www = UnityWebRequest.Get(GetOverpassRequestURL(openStreetMapOptions.overPassRequestData));

            yield return www.SendWebRequest();

            if (Utils.Misc.WebRequestResultIsError(www))
            {
                Debug.Log(www.error);
                Debug.Log(GetOverpassRequestURL(openStreetMapOptions.overPassRequestData));
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                CreateTextObjects(www.downloadHandler.text);
            }
        }


        public string GetNodeTagValue(XmlNode node, string tagName)
        {
            var children = node.ChildNodes;
            foreach (XmlNode nodeTag in children)
            {
                if (nodeTag.Attributes != null && nodeTag.Attributes["k"].Value == tagName)
                {
                    return nodeTag.Attributes["v"].Value;
                }
            }

            return null;
        }

        public string GetNodeName(XmlNode node)
        {
            return (GetNodeTagValue(node, "poiName") ?? GetNodeTagValue(node, "amenity")) ?? "No Name";
        }

        // Update is called once per frame
        void CreateTextObjects(string text)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(text);

            var nodes = xmlDoc.GetElementsByTagName("node");

            poiData = new POIData[nodes.Count];

            var i = 0;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    double lat = double.Parse(node.Attributes["lat"].Value, CultureInfo.InvariantCulture);
                    double lng = double.Parse(node.Attributes["lon"].Value, CultureInfo.InvariantCulture);

                    var nodeName = GetNodeName(node);


                    poiData[i] = new POIData
                    {
                        location = new Location()
                        {
                            Latitude = lat,
                            Longitude = lng,
                            AltitudeMode = AltitudeMode.GroundRelative,
                            Altitude = height
                        },
                        name = nodeName
                    };
                }

                i++;
            }


            for (var k = 0; k < poiData.Length; k++)
            {
                AddPOI(poiData[k].location, poiData[k].name);
            }

        }

        void AddLocationsPOIs()
        {
            foreach (var location in locations)
            {
                AddPOI(location, location.Label);
            }
        }


        // ReSharper disable once UnusedParameter.Local
        void AddPOI(Location location, string poiName)
        {
            var textInstance = PlaceAtLocation.CreatePlacedInstance(textPrefab.gameObject, location,
                new PlaceAtLocation.PlaceAtOptions()
                {
                    MovementSmoothing = 0.1f,
                    HideObjectUntilItIsPlaced = true,
                    MaxNumberOfLocationUpdates = 10,
                    UseMovingAverage = false
                }, true);

            textInstance.GetComponent<TextMesh>().text = poiName;
        }
    }
}
