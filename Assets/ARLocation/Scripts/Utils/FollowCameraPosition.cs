using UnityEngine;

namespace ARLocation.Utils
{

    public class FollowCameraPosition : MonoBehaviour
    {
        private Transform mainCameraTransform;

        public float Height = 1.4f;

        public bool UseARLocationConfig = true;

        public Transform UseGameObjectHeight;

        private float configY;
        private bool useGOHeight;

        // Use this for initialization
        void Start()
        {
            if (Camera.main != null) mainCameraTransform = Camera.main.transform;

            configY = -ARLocation.Config.InitialGroundHeightGuess;

            useGOHeight = UseGameObjectHeight != null;
        }

        // Update is called once per frame
        void Update()
        {
            var cameraPos = mainCameraTransform.position;

            var y = useGOHeight ? UseGameObjectHeight.position.y : (UseARLocationConfig ? (cameraPos.y + configY) : (cameraPos.y - Height));

            var transform1 = transform;
            transform1.position = new Vector3(
                cameraPos.x,
                y,
                cameraPos.z
            );
        }
    }
}
