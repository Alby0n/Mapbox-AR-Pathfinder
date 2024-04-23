using UnityEngine;
using UnityEngine.UI;

namespace ARLocation.UI
{

    public class ARTrackingInfo : MonoBehaviour
    {
        public Text InfoValue;
        public Text ProviderValue;

        private void Update()
        {
            InfoValue.text = ARLocationManager.Instance.GetARSessionInfoString();
            ProviderValue.text = ARLocationManager.Instance.GetARSessionProviderString();
        }
    }
}
