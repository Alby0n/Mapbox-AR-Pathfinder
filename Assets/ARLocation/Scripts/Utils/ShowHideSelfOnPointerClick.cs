using UnityEngine;

namespace ARLocation.Utils
{

    public class ShowHideSelfOnPointerClick : MonoBehaviour
    {
        private Canvas canvas;

        // Use this for initialization
        void Start()
        {
            canvas = GetComponent<Canvas>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                canvas.enabled = !canvas.enabled;
            }
        }
    }
}
