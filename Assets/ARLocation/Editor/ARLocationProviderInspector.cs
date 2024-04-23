using UnityEngine;
using UnityEditor;

namespace ARLocation
{
    [CustomEditor(typeof(ARLocationProvider))]
    public class ARLocationProviderInspector : Editor
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
