using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation.Utils
{

    public class DevCameraController : MonoBehaviour
    {
        /// <summary>
        /// The mouse look/rotation sensitivity.
        /// </summary>
        public float MouseSensitivity = 1.0f;

        /// <summary>
        /// The walking speed
        /// </summary>
        [FormerlySerializedAs("speed")] public float Speed = 1.0f;

        // Current orientation parameters
        float rotationY;
        float rotationX;

        // The initial location
        Location firstLocation;

        // The accumulated lat/lng displacement
        private Vector3 accumDelta;

        // Use this for initialization
        void Awake()
        {
            // If we are not running on a device, make this the main
            // camera, else, self-destruct.
            if (!Misc.IsARDevice())
            {
                var arCamera = GameObject.Find("AR Camera");

                if (arCamera != null)
                {
                    arCamera.tag = "Untagged";
                    arCamera.SetActive(false);
                }

                GetComponent<Camera>().gameObject.SetActive(true);

                GameObject o;
                (o = gameObject).AddComponent<AudioListener>();
                o.tag = "MainCamera";

                ARLocationManager.Instance.WaitForARTrackingToStart = false;
            }
            else
            {
                Destroy(gameObject);
            }

            var rotation = transform.rotation;
            rotationX = rotation.eulerAngles.x;
            rotationY = rotation.eulerAngles.y;
        }

        // Update is called once per frame
        void Update()
        {
            var forward = Vector3.ProjectOnPlane(transform.forward, new Vector3(0, 1, 0));

            var initialPosition = transform.position;
            var spd = Speed;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                spd *= 2;
            }

            if (Input.GetKey("w"))
            {
                transform.Translate(
                    forward * spd, Space.World
                );
            }

            if (Input.GetKey("s"))
            {
                transform.Translate(
                    -forward * spd, Space.World
                );
            }

            if (Input.GetKey("d"))
            {
                transform.Translate(
                    transform.right * spd, Space.World
                );
            }

            if (Input.GetKey("a"))
            {
                transform.Translate(
                    -transform.right * spd, Space.World
                );
            }

            if (Input.GetKey("up"))
            {
                transform.Translate(
                    transform.up * spd, Space.World
                );
            }

            var finalPosition = transform.position;
            var delta = finalPosition - initialPosition;

            var locMngr = ARLocationProvider.Instance;

            if (firstLocation == null)
            {
                firstLocation = locMngr.CurrentLocation.ToLocation();
            }

            accumDelta += delta * 0.00001f;

            //locMngr.UpdateMockLocation(new Location(
            //    firstLocation.latitude + accumDelta.z,
            //    firstLocation.longitude + accumDelta.x,
            //    0
            //));

            rotationY += Input.GetAxis("Mouse X") * MouseSensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * MouseSensitivity;

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
    }
}
