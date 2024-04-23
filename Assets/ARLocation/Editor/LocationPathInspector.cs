using System;
using UnityEngine;
using UnityEditor;
// ReSharper disable DelegateSubtraction

namespace ARLocation
{

    [CustomEditor(typeof(LocationPath))]
    public class LocationPathInspector : Editor
    {

        SerializedProperty alpha;
        SerializedProperty locations;
        SerializedProperty sceneViewScale;
        SerializedProperty splineType;

        // float viewScale = 1.0f;

        private void OnEnable()
        {
            FindProperties();

            AddOnSceneGUIDelegate(OnSceneGuiDelegate);

            Tools.hidden = true;
        }


#if UNITY_2019_1_OR_NEWER
        private void AddOnSceneGUIDelegate(Action<SceneView> del)
        {
            SceneView.duringSceneGui += del; // sceneView => OnSceneGUI();
        }
#else
        private void AddOnSceneGUIDelegate(SceneView.OnSceneFunc del)
        {
            SceneView.onSceneGUIDelegate += del;
        }
#endif

#if UNITY_2019_1_OR_NEWER
        private void RemoveOnSceneGUIDelegate(Action<SceneView> del)
        {
            SceneView.duringSceneGui -= del; // sceneView => OnSceneGUI();
        }
#else
        private void RemoveOnSceneGUIDelegate(SceneView.OnSceneFunc del)
        {
            SceneView.onSceneGUIDelegate -= del;
        }
#endif


        private void OnSceneGuiDelegate(SceneView sceneview)
        {
            OnSceneGUI();
        }

        private void FindProperties()
        {
            alpha = serializedObject.FindProperty("Alpha");
            locations = serializedObject.FindProperty("Locations");
            sceneViewScale = serializedObject.FindProperty("SceneViewScale");
            splineType = serializedObject.FindProperty("SplineType");
        }

        void OnDisable()
        {
            RemoveOnSceneGUIDelegate(OnSceneGuiDelegate);

            Tools.hidden = false;
        }

        void DrawOnSceneGui()
        {
            FindProperties();

            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(20, 20, 200, 200));

            var rect = EditorGUILayout.BeginVertical();
            GUI.color = new Color(1, 1, 1, 0.4f);
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("ARLocation Path");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var style = new GUIStyle
            {
                margin = new RectOffset(0, 0, 4, 200)
            };

            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = new Color(0.2f, 0.5f, 0.92f);

            GUILayout.Label("View Scale: ", GUILayout.Width(80.0f));


            var newViewScale = GUILayout.HorizontalSlider(sceneViewScale.floatValue, 0.01f, 1.0f);

            if (Math.Abs(newViewScale - sceneViewScale.floatValue) > 0.000001f)
            {
                sceneViewScale.floatValue = newViewScale;
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Label(sceneViewScale.floatValue.ToString("0.00"), GUILayout.Width(32.0f));


            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            GUILayout.EndArea();
            Handles.EndGUI();
        }

        void OnSceneGUI()
        {
            LocationPath locationPath = (LocationPath)target;

            if (locationPath.Locations == null)
            {
                return;
            }

            DrawOnSceneGui();
            DrawPath();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (((SplineType)splineType.enumValueIndex) == SplineType.CatmullromSpline)
            {
                EditorGUILayout.Slider(alpha, 0, 1, "Curve Alpha");
            }

            EditorGUILayout.PropertyField(splineType);
            EditorGUILayout.PropertyField(locations, true);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawPath()
        {
            LocationPath locationPath = (LocationPath)target;
            var pathLocations = locationPath.Locations;

            if (pathLocations == null || pathLocations.Length < 2)
            {
                return;
            }

            var viewScale = sceneViewScale.floatValue;

            var points = new Vector3[pathLocations.Length];

            for (var i = 0; i < pathLocations.Length; i++)
            {
                var loc = pathLocations[i];
                points[i] = Vector3.Scale(loc.ToVector3(), new Vector3(viewScale, 1, viewScale));
            }


            //var points = curve.SamplePoints(100, p => getVec(p, curve.points[0]));
            var effScale = (1.0f + Mathf.Cos(viewScale * Mathf.PI / 2 - Mathf.PI));
            var s = new Vector3(effScale, 1.0f, effScale);


            var newCPs = new Vector3[locationPath.Locations.Length];
            for (var i = 0; i < locationPath.Locations.Length; i++)
            {
                // ps.Add(locationPath.locations[i].ToVector3());

                var loc = locationPath.Locations[i];
                var p = Location.GetGameObjectPositionForLocation(
                    null,
                    new Vector3(),
                   // new Transform(),
                   pathLocations[0],
                   pathLocations[i],
                   true
                   );
                Handles.color = Color.blue;

                Handles.SphereHandleCap(i, Vector3.Scale(p, s), Quaternion.identity, 0.4f, EventType.Repaint);

                Vector3 newScaledPos = Handles.PositionHandle(Vector3.Scale(p, s), Quaternion.identity);
                Vector3 newPos = new Vector3(newScaledPos.x/effScale, newScaledPos.y, newScaledPos.z / effScale);

                Handles.Label(Vector3.Scale(p, s), loc.Label == "" ? ("   Point " + i) : loc.Label);
                newCPs[i] = Vector3.Scale(p, s);
            }

            Spline newPath;
            if (((SplineType)splineType.enumValueIndex) == SplineType.CatmullromSpline)
            {
                newPath = new CatmullRomSpline(newCPs, 100, alpha.floatValue);
            }
            else
            {
                newPath = new LinearSpline(newCPs);
            }

            var newSample = newPath.SamplePoints(1000);

            for (var i = 0; i < (newSample.Length - 2); i++)
            {
                Handles.color = Color.green;
                Handles.DrawLine(newSample[i + 1], newSample[i]);
            }
        }
    }
}
