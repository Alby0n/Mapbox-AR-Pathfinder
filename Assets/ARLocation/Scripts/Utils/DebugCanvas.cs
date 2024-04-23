using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Local

namespace ARLocation.Utils
{

    public class DebugCanvas : MonoBehaviour
    {

        GameObject latValueText;
        GameObject lngValueText;
        GameObject headingValueText;
        GameObject altitudeValueText;
        GameObject debugText;

        float firstHeading;

        // Use this for initialization
        void Start()
        {
            latValueText = GameObject.Find("LatValue");
            lngValueText = GameObject.Find("LngValue");
            headingValueText = GameObject.Find("HeadingValue");
            altitudeValueText = GameObject.Find("AltValue");
            debugText = GameObject.Find("DebugText");

            //locationProvider.onLocationUpdated((Location location, Location _, Vector3 __, float accuracy) =>
            //{
            //    setLat(location.latitude, accuracy);
            //    setLng(location.longitude);
            //    setAltitude(location.altitude);
            //});

            //locationProvider.onCompassUpdated(setHeading);
        }

        void SetLat(double val)
        {
            latValueText.GetComponent<Text>().text = val.ToString(CultureInfo.InvariantCulture);
        }

        void SetLng(double val)
        {
            lngValueText.GetComponent<Text>().text = val.ToString(CultureInfo.InvariantCulture);
        }

        void SetHeading(double val)
        {
            headingValueText.GetComponent<Text>().text = val.ToString(CultureInfo.InvariantCulture);
        }

        void SetAltitude(double val)
        {
            altitudeValueText.GetComponent<Text>().text = val.ToString(CultureInfo.InvariantCulture);
        }

        public void SetDebugText(string val)
        {
            debugText.GetComponent<Text>().text = val;
        }
    }
}
