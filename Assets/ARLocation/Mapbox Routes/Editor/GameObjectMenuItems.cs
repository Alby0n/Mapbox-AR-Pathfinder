using UnityEngine;
using UnityEditor;

namespace ARLocation.MapboxRoutes
{
    public static class GameObjectMenuItems
    {
        [MenuItem("GameObject/AR+GPS/Mapbox Route")]
        public static GameObject CreateMapboxRoute()
        {
            var go = new GameObject("Mapbox Route");

            var route = go.AddComponent<MapboxRoutes.MapboxRoute>();

            var signpostPath = AssetDatabase.GUIDToAssetPath("154e5264001534d7999c9720b114562d");
            var signpostPrefab = AssetDatabase.LoadAssetAtPath<MapboxRoutes.AbstractRouteSignpost>(signpostPath);
            var pathRenderer = go.AddComponent<MapboxRoutes.NextStepRoutePathRenderer>();
            var indicator = go.AddComponent<MapboxRoutes.DefaultOnScreenTargetIndicator>();
            var arrowPath = AssetDatabase.GUIDToAssetPath("19a300465515241c2bb5670f7b18e69b");
            var arrow = AssetDatabase.LoadAssetAtPath<Sprite>(arrowPath);
            var lineMaterialPath = AssetDatabase.GUIDToAssetPath("697c3d00ed75e4c388f1112e6817fa65");
            var lineMaterial = AssetDatabase.LoadAssetAtPath<Material>(lineMaterialPath);

            indicator.ArrowSprite = arrow;
            indicator.NeutralArrowDirection = DefaultOnScreenTargetIndicator.ArrowDir.Right;

            pathRenderer.Settings.LineMaterial = lineMaterial;

            route.Settings.SignpostPrefabs.Add(signpostPrefab);
            route.Settings.PathRenderer = pathRenderer;
            route.Settings.OnScreenIndicator = indicator;

            return go;
        }
    }
}

