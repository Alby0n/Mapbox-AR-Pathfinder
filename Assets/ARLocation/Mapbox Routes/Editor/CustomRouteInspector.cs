using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace ARLocation.MapboxRoutes
{
    [CustomEditor(typeof(CustomRoute))]
    public class CustomRouteInspector : Editor
    {
        private List<Vector3> pointsCache = null;

        private void OnEnable()
        {
            AddOnSceneGUIDelegate(OnSceneGuiDelegate);
            Tools.hidden = true;
        }

        void OnDisable()
        {
            RemoveOnSceneGUIDelegate(OnSceneGuiDelegate);
            Tools.hidden = false;
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

        void DrawHandles(CustomRoute customRoute)
        {
            var viewScale = customRoute.SceneViewScale;
            var effScale = (1.0f + Mathf.Cos(viewScale * Mathf.PI / 2 - Mathf.PI));
            var s = new Vector3(effScale, 1.0f, effScale);

            for (var i = 0; i < pointsCache.Count; i++)
            {
                var p = pointsCache[i];
                var isStep = customRoute.Points[i].IsStep;
                Handles.color = isStep ? Color.red : Color.blue;
                Handles.SphereHandleCap(0, Vector3.Scale(p, s), Quaternion.identity, 4.0f, EventType.Repaint);

                if (i > 0)
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(Vector3.Scale(s, pointsCache[i - 1]), Vector3.Scale(s, pointsCache[i]));
                }
            }
        }

        void DrawOnSceneGui(CustomRoute customRoute)
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(20, 20, 200, 200));

            var rect = EditorGUILayout.BeginVertical();
            GUI.color = new Color(1, 1, 1, 0.4f);
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Custom Route");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var style = new GUIStyle
            {
                margin = new RectOffset(0, 0, 4, 200)
            };

            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = new Color(0.2f, 0.5f, 0.92f);

            GUILayout.Label("View Scale: ", GUILayout.Width(80.0f));

            var sceneViewScale = customRoute.SceneViewScale;

            var newViewScale = GUILayout.HorizontalSlider(sceneViewScale, 0.01f, 1.0f);

            if (Math.Abs(newViewScale - sceneViewScale) > 0.000001f)
            {
                sceneViewScale = newViewScale;
                serializedObject.ApplyModifiedProperties();
            }

            customRoute.SceneViewScale = sceneViewScale;

            GUILayout.Label(sceneViewScale.ToString("0.00"), GUILayout.Width(32.0f));

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        void OnSceneGUI()
        {
            var customRoute = (CustomRoute)target;

            if (customRoute == null || customRoute.Points.Count < 2)
            {
                return;
            }

            if (customRoute.IsDirty || pointsCache == null)
            {
                pointsCache = new List<Vector3>();

                for (var i = 0; i < customRoute.Points.Count; i++)
                {
                    var position = Location.GetGameObjectPositionForLocation(
                            null,
                            new Vector3(),
                            customRoute.Points[0].Location,
                            customRoute.Points[i].Location,
                            true
                            );

                    position.y = 0;
                    pointsCache.Add(position);
                }

                customRoute.IsDirty = false;
            }

            DrawOnSceneGui(customRoute);
            DrawHandles(customRoute);
        }

        [MenuItem("Assets/AR+GPS/Custom Route From KML", false)]
        private static void KmlMenuClick()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            var reader = new System.IO.StreamReader(path);
            var contents = reader.ReadToEnd();

            Debug.Log(contents);

            reader.Close();

            var xml = new System.Xml.XmlDocument();
            xml.LoadXml(contents);

            var kmlNode = xml["kml"];
            if (kmlNode == null)
            {
                Debug.LogError("Erorr parsing xml file!");
            }

            var documentNode = kmlNode["Document"];
            if (documentNode == null)
            {
                Debug.LogError("Erorr parsing xml file!");
            }

            var placemarkNodeList = documentNode.GetElementsByTagName("Placemark");
            for (var i = 0; i < placemarkNodeList.Count; i++)
            {
                var placemarkNode = placemarkNodeList[i];
                var name = placemarkNode["name"]?.Name;
                var lineStringNode = placemarkNode["LineString"];
                if (lineStringNode != null)
                {
                    var coordinatesNode = lineStringNode["coordinates"];
                    if (coordinatesNode != null)
                    {
                        // var txt = coordinatesNode.Value.TrimStart();
                        var txt = coordinatesNode.InnerText.TrimStart().TrimEnd();
                        var split = txt.Split(new char[] { ',', ' ' });
                        foreach (var s in split)
                        {
                            Debug.Log($":{s}:");
                        }

                        var customRoute = ScriptableObject.CreateInstance<MapboxRoutes.CustomRoute>();
                        customRoute.Points = new List<CustomRoute.Point>();
                        for (var k = 0; k < split.Length; k += 3)
                        {
                            var lonString = split[k];
                            var latString = split[k + 1];

                            double lat, lon;
                            if (!double.TryParse(lonString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }

                            if (!double.TryParse(latString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }

                            var location = new Location(lat, lon);
                            var point = new MapboxRoutes.CustomRoute.Point();
                            point.Location = location;
                            customRoute.Points.Add(point);
                        }

                        customRoute.Points[0].IsStep = true;
                        customRoute.Points[customRoute.Points.Count - 1].IsStep = true;

                        var dirPath = System.IO.Path.GetDirectoryName(path);
                        var baseName = System.IO.Path.GetFileNameWithoutExtension(path);
                        var filename = System.IO.Path.Combine(dirPath, baseName + (placemarkNodeList.Count == 0 ? "": $"({i})") + ".asset");

                        AssetDatabase.CreateAsset(customRoute, filename);
                    }
                }
            }

        }

        [MenuItem("Assets/AR+GPS/Custom Route From KML", true)]
        private static bool KmlMenuClickValidator()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var ext = System.IO.Path.GetExtension(path);

            return ext.ToLower(System.Globalization.CultureInfo.InvariantCulture) == ".kml";
        }
    }
}
