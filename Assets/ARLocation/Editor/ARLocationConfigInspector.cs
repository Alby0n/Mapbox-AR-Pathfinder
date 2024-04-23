using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// ReSharper disable InconsistentNaming

namespace ARLocation
{

    /// <summary>
    /// Inspector for the ARLocationConfig. This inspector is the main configuration
    /// interface for the AR+GPS Location plugin.
    /// </summary>
    [CustomEditor(typeof(ARLocationConfig))]
    public class ARLocationConfigInspector : Editor
    {

        SerializedProperty p_EarthRadiusInKM;
        SerializedProperty p_EarthEquatorialRadius;
        SerializedProperty p_EarthFirstEccentricitySquared;
        SerializedProperty p_UseVuforia;
        SerializedProperty p_UseCustomGeoCalculator;
        SerializedProperty p_InitialGroundHeightGuess;
        SerializedProperty p_VuforiaGroundHitTestDistance;
        private SerializedProperty p_MinGroundHeight;
        private SerializedProperty p_MaxGroundHeight;
        private SerializedProperty p_GroundHeightSmoothingFactor;

        DefineSymbolsManager defineSymbolsManager;

        const string ARGPS_USE_VUFORIA = "ARGPS_USE_VUFORIA";
        const string ARGPS_USE_NATIVE_LOCATION = "ARGPS_USE_NATIVE_LOCATION";
        const string ARGPS_USE_CUSTOM_GEO_CALC = "ARGPS_USE_CUSTOM_GEO_CALC";

        Dictionary<string, string> defineSymbolProps = new Dictionary<string, string> {
        {ARGPS_USE_VUFORIA, "UseVuforia"},
        {ARGPS_USE_NATIVE_LOCATION, "UseNativeLocationModule"}
    };

        private void OnEnable()
        {
            p_EarthRadiusInKM = serializedObject.FindProperty("EarthMeanRadiusInKM");
            p_EarthEquatorialRadius = serializedObject.FindProperty("EarthEquatorialRadiusInKM");
            p_EarthFirstEccentricitySquared = serializedObject.FindProperty("EarthFirstEccentricitySquared");
            p_UseVuforia = serializedObject.FindProperty("UseVuforia");
            p_UseCustomGeoCalculator = serializedObject.FindProperty("UseCustomGeoCalculator");
            p_InitialGroundHeightGuess = serializedObject.FindProperty("InitialGroundHeightGuess");
            p_VuforiaGroundHitTestDistance = serializedObject.FindProperty("VuforiaGroundHitTestDistance");
            p_MinGroundHeight = serializedObject.FindProperty("MinGroundHeight");
            p_MaxGroundHeight = serializedObject.FindProperty("MaxGroundHeight");
            p_GroundHeightSmoothingFactor = serializedObject.FindProperty("GroundHeightSmoothingFactor");

            defineSymbolsManager = new DefineSymbolsManager(new[]
            {
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android
            });
        }

        private void UpdateDefineSymbolsFromPlayerSettings()
        {
            defineSymbolsManager.UpdateFromBuildSettings();

            foreach (var item in defineSymbolProps)
            {
                if (item.Value == "UseVuforia")
                {
#if !UNITY_2019_3_OR_NEWER
#if UNITY_2019_2
                    var value = defineSymbolsManager.Has(item.Key) && PlayerSettings.vuforiaEnabled;
#else
                    var value = defineSymbolsManager.Has(item.Key) && PlayerSettings.GetPlatformVuforiaEnabled(BuildTargetGroup.Android) && PlayerSettings.GetPlatformVuforiaEnabled(BuildTargetGroup.iOS);
#endif
                    UpdateDefineSymbolProp(item.Value, value);
#endif
                }
                else
                {
                    UpdateDefineSymbolProp(item.Value, defineSymbolsManager.Has(item.Key));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateDefineSymbolProp(string propName, bool value)
        {
            var prop = serializedObject.FindProperty(propName);

            if (prop == null)
            {
                return;
            }

            prop.boolValue = value;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UpdateDefineSymbolsFromPlayerSettings();

            defineSymbolsManager.UpdateFromBuildSettings();


            EditorGUILayout.HelpBox("AR+GPS Location " + ARLocationConfig.Version, MessageType.None, true);
            EditorGUILayout.PropertyField(p_EarthRadiusInKM);
            EditorGUILayout.PropertyField(p_EarthEquatorialRadius);
            EditorGUILayout.PropertyField(p_EarthFirstEccentricitySquared);
            EditorGUILayout.PropertyField(p_InitialGroundHeightGuess);
            EditorGUILayout.PropertyField(p_MinGroundHeight);
            EditorGUILayout.PropertyField(p_MaxGroundHeight);
            EditorGUILayout.PropertyField(p_GroundHeightSmoothingFactor);
            EditorGUILayout.PropertyField(p_VuforiaGroundHitTestDistance);
            EditorGUILayout.PropertyField(p_UseVuforia);
            EditorGUILayout.PropertyField(p_UseCustomGeoCalculator);


            if (p_UseVuforia.boolValue)
            {
#if UNITY_2019_3_OR_NEWER
		EditorGUILayout.HelpBox("Make sure that Vuforia is instaled in the Package Manager Window.  On Android, also make sure that the 'ARCore XR Plugin' is not installed.", MessageType.Info);
#endif
                // EditorGUILayout.HelpBox("So that Vuforia works correctly, please enable the 'Track Device Pose' option in the Vuforia configuration, and set the tracking" +
                //     " mode to 'POSITIONAL'.", MessageType.Warning);
                EditorGUILayout.HelpBox(
                    "Note that the regular sample scenes do not work with Vuforia. You can download a project with Vuforia samples at https://github.com/dmbfm/unity-ar-gps-location-issues/releases/tag/v3.1.1", MessageType.Warning);
            }


            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("https://docs.unity-ar-gps-location.com");
            }

            var config = (ARLocationConfig)target;

            UpdateDefineSymbolPropConfig(config.UseVuforia, p_UseVuforia.boolValue, ARGPS_USE_VUFORIA);

            UpdateVuforiaPlayerSettings(config.UseVuforia, p_UseVuforia.boolValue);

            UpdateDefineSymbolPropConfig(config.UseCustomGeoCalculator, p_UseCustomGeoCalculator.boolValue, ARGPS_USE_CUSTOM_GEO_CALC);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateVuforiaPlayerSettings(bool oldValue, bool newValue)
        {
            if (newValue == oldValue)
            {
                return;
            }

#if !UNITY_2019_3_OR_NEWER
#if UNITY_2019_2
            if (newValue)
            {
                PlayerSettings.vuforiaEnabled = true;
            }
            else
            {
                PlayerSettings.vuforiaEnabled = false;
            }
#else
            if (newValue)
            {
                PlayerSettings.SetPlatformVuforiaEnabled(BuildTargetGroup.Android, true);
                PlayerSettings.SetPlatformVuforiaEnabled(BuildTargetGroup.iOS, true);
            }
            else
            {
                PlayerSettings.SetPlatformVuforiaEnabled(BuildTargetGroup.Android, false);
                PlayerSettings.SetPlatformVuforiaEnabled(BuildTargetGroup.iOS, false);
            }
#endif
#endif

        }

        private void UpdateDefineSymbolPropConfig(bool oldValue, bool newValue, string symbol)
        {
            if (newValue == oldValue) return;

            if (newValue)
            {
                defineSymbolsManager.Add(symbol);
            }
            else
            {
                defineSymbolsManager.Remove(symbol);
            }

            defineSymbolsManager.ApplyToBuildSettings();
        }
    }
}
