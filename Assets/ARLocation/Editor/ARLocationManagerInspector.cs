using UnityEngine;
using UnityEditor;

namespace ARLocation
{
    [CustomEditor(typeof(ARLocationManager))]
    public class ARLocationManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open AR+GPS Location configuration"))
            {
                Selection.activeObject = Resources.Load<ARLocationConfig>("ARLocationConfig");
            }
        }
    }
}
