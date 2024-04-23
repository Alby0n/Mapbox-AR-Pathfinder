using UnityEngine;

namespace ARLocation
{
    /// <summary>
    /// This static class loads the global configuration for the AR + GPS Location
    /// plugin.
    ///
    /// Any other global functionality of the plugin should be placed here as
    /// well.
    /// </summary>

    static class ARLocation
    {
        public static readonly ARLocationConfig Config;

        static ARLocation()
        {
            Config = Resources.Load<ARLocationConfig>("ARLocationConfig");

            if (Config == null)
            {
                Debug.LogWarning("Resources/ARLocationConfig.asset not found; creating new configuration from defaults.");
                Config = ScriptableObject.CreateInstance<ARLocationConfig>();
            }
        }
    }
}
