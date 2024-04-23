using UnityEngine;

namespace ARLocation.Utils
{

    public class FaceCamera : MonoBehaviour
    {
        private Transform mainCameraTransform;

        // Use this for initialization
        void Start()
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
            else
            {
                mainCameraTransform = ARLocationManager.Instance.MainCamera.transform;
            }

        }

        // Update is called once per frame
        void Update()
        {
            var position = mainCameraTransform.position;
            Vector3 v = position - transform.position;
            v.x = v.z = 0.0f;
            transform.LookAt(position - v);
            transform.Rotate(0, 180, 0);
        }
    }
}
