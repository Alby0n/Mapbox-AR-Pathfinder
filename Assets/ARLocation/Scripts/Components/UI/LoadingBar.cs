using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ARLocation.UI
{
    public class LoadingBar : MonoBehaviour
    {

        [FormerlySerializedAs("fillPercentage")] [Range(0, 1)]
        public float FillPercentage = 0.4f;

        [FormerlySerializedAs("startColor")] public Color StartColor = Color.green;
        [FormerlySerializedAs("middleColor")] public Color MiddleColor = Color.yellow;
        [FormerlySerializedAs("endColor")] public Color EndColor = Color.red;
        [FormerlySerializedAs("textColor")] public Color TextColor = Color.blue;
        [FormerlySerializedAs("usePercentageText")] public bool UsePercentageText;
        [FormerlySerializedAs("text")] public string Text = "100";

        private GameObject fillBar;
        private Text barText;
        private RectTransform rectTransform;
        private RectTransform fillBarRect;
        private Image fillBarImage;

        // Use this for initialization
        void Start()
        {
            fillBar = transform.Find("Bar").gameObject;
            barText = transform.Find("Text").gameObject.GetComponent<Text>();
            barText.color = TextColor;
            barText.fontStyle = FontStyle.Bold;
            rectTransform = GetComponent<RectTransform>();
            fillBarRect = fillBar.GetComponent<RectTransform>();
            fillBarImage = fillBar.GetComponent<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            var w = rectTransform.rect.width;

            fillBarRect.offsetMin = new Vector2(0, 0);
            fillBarRect.offsetMax = new Vector2((FillPercentage - 1) * w, 0);

            if (FillPercentage < 0.5)
            {
                fillBarImage.color = Color.Lerp(StartColor, MiddleColor, FillPercentage * 2);
            }
            else
            {
                fillBarImage.color = Color.Lerp(MiddleColor, EndColor, (FillPercentage - 0.5f) * 2);
            }

            if (UsePercentageText)
            {
                barText.text = ((int)(FillPercentage * 100.0f)) + "%";
            }
            else
            {
                barText.text = Text;
            }
        }
    }
}
