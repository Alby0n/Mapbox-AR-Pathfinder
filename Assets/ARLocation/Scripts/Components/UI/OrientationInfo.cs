using UnityEngine;
using UnityEngine.UI;

namespace ARLocation.UI
{
    public class OrientationInfo : MonoBehaviour
    {
        private GameObject redArrow;
        private GameObject trueNorthLabel;
        private GameObject magneticNorthLabel;
        private GameObject headingAccuracyLabel;
        private GameObject compassImage;
        private ARLocationProvider locationProvider;
        private GameObject mainCamera;
        private bool isMainCameraNull;
        private Text text;
        private Text text1;
        private Text text2;
        private RectTransform rectTransform;
        private RectTransform rectTransform1;

        // Use this for initialization
        void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            isMainCameraNull = mainCamera == null;

            locationProvider = ARLocationProvider.Instance;

            redArrow = GameObject.Find(gameObject.name + "/Panel/CompassImage/RedArrow");
            trueNorthLabel = GameObject.Find(gameObject.name + "/Panel/TrueNorthLabel");
            magneticNorthLabel = GameObject.Find(gameObject.name + "/Panel/MagneticNorthLabel");
            headingAccuracyLabel = GameObject.Find(gameObject.name + "Panel/HeadingAccuracyLabel");
            compassImage = GameObject.Find(gameObject.name + "Panel/CompassImage");

            text2 = headingAccuracyLabel.GetComponent<Text>();
            text1 = magneticNorthLabel.GetComponent<Text>();
            text = trueNorthLabel.GetComponent<Text>();

            rectTransform1 = compassImage.GetComponent<RectTransform>();
            rectTransform = redArrow.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (isMainCameraNull)
            {
                return;
            }

            var currentHeading = locationProvider.CurrentHeading.heading;
            var currentMagneticHeading = locationProvider.CurrentHeading.magneticHeading;
            var currentAccuracy = locationProvider.Provider.CurrentHeading.accuracy;

            text.text = "TRUE NORTH: " + currentHeading;
            text1.text = "MAGNETIC NORTH: " + currentMagneticHeading;
            text2.text = "ACCURACY: " + currentAccuracy;

            rectTransform.rotation = Quaternion.Euler(0, 0, (float)currentMagneticHeading);
            rectTransform1.rotation = Quaternion.Euler(0, 0, (float)currentHeading);
        }
    }
}
