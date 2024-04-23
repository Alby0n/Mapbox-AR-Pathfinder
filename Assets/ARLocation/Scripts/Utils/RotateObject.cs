using UnityEngine;

namespace ARLocation.Utils
{
    public class RotateObject : MonoBehaviour
    {
        public float Speed = 10.0f;
        public Vector3 Axis = Vector3.up;

        private float angle;

        // Update is called once per frame
        void Update()
        {
            angle += Speed * Time.deltaTime;
            transform.localRotation = Quaternion.AngleAxis(angle, Axis);
        }
    }
}
