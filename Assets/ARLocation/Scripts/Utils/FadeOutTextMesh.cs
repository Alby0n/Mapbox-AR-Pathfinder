using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ARLocation.Utils
{

    public class FadeOutTextMesh : MonoBehaviour
    {

        [FormerlySerializedAs("duration")] public float Duration = 2.0f;
        private TextMesh textMesh;

        // Use this for initialization
        void Start()
        {
            textMesh = GetComponent<TextMesh>();
            StartCoroutine("FadeOut");
        }

        IEnumerator FadeOut()
        {
            var t = Duration;
            var initialColor = textMesh.color;
            while (textMesh.color.a > 0.001f)
            {
                var color = textMesh.color;
                var target = new Color(color.r, color.g, color.b, 0);
                textMesh.color = Color.Lerp(initialColor, target, 1 - t / Duration);
                t -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
