using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ARLocation.UI
{
    public class DebugInfoOverlay : MonoBehaviour
    {

        [FormerlySerializedAs("show")] public bool Show;
        [FormerlySerializedAs("showObjectInfo")] public bool ShowObjectInfo;

        private GameObject canvas;
        private GameObject canvas2;
        private GameObject btn1;
        private GameObject btn2;
        private Text btn1Text;

        // Use this for initialization
        void Start()
        {
            canvas = GameObject.Find(gameObject.name + "/Canvas");
            canvas2 = GameObject.Find(gameObject.name + "/ObjectInfoCanvas");
            btn1 = GameObject.Find(gameObject.name + "/ButtonCanvas/ToggleInfoButton");

            if (btn1)
            {
                btn1Text = btn1.GetComponentInChildren<Text>();
            }

            UpdateInfo();
        }

        private void UpdateInfo()
        {
            if (!canvas || !canvas2) return;

            canvas.SetActive(Show);
            canvas2.SetActive(ShowObjectInfo);

            var message = Show ?  "Hide Info Overlay" : "Show Info Overlay";

            if (btn1Text)
            {
                btn1Text.text = message;
            }
        }

        public void Toggle()
        {
            Show = !Show;
            UpdateInfo();
        }

        public void ToggleObjectInfo()
        {
            ShowObjectInfo = !ShowObjectInfo;
            UpdateInfo();
        }
    }
}
