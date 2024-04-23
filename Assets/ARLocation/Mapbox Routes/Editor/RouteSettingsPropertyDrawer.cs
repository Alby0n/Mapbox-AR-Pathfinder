using UnityEngine;
using UnityEditor;

namespace ARLocation.MapboxRoutes
{
    [CustomPropertyDrawer(typeof(MapboxRoute.RouteSettings))]
    public class RouteSettingsPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty type;
        private SerializedProperty from;
        private SerializedProperty to;
        private SerializedProperty customRoute;

        public void FindSerializedProperties(SerializedProperty property)
        {
            type = property.FindPropertyRelative("RouteType");
            from = property.FindPropertyRelative("From");
            to = property.FindPropertyRelative("To");
            customRoute = property.FindPropertyRelative("CustomRoute");

            Debug.Assert(type != null);
            Debug.Assert(from != null);
            Debug.Assert(to != null);
            Debug.Assert(customRoute != null);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var height = lineHeight;

            // if (!property.isExpanded)
            // {
                // return height;
            // }

            // height += lineHeight;

            switch (type.enumValueIndex)
            {
                case (int)MapboxRoute.RouteType.CustomRoute:
                    height += EditorGUI.GetPropertyHeight(customRoute);
                    break;

                case (int)MapboxRoute.RouteType.Mapbox:
                    height += EditorGUI.GetPropertyHeight(from);
                    height += EditorGUI.GetPropertyHeight(to);
                    break;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            EditorGUI.BeginProperty(position, label, property);
            var increment = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.height = EditorGUIUtility.singleLineHeight;
            var indentRect = EditorGUI.IndentedRect(position);

            // property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(indentRect, property.isExpanded, label);
            // if (property.isExpanded)
            // {
                // EditorGUI.indentLevel++;
                // position.y += increment;
                EditorGUI.PropertyField(position, type);

            // EditorGUI.EndFoldoutHeaderGroup();
                switch (type.enumValueIndex)
                {
                    case (int)MapboxRoute.RouteType.Mapbox:


                        position.y += increment;
                        EditorGUI.PropertyField(position, from);

                        position.y += EditorGUI.GetPropertyHeight(from);
                        EditorGUI.PropertyField(position, to);
                        break;

                    case (int)MapboxRoute.RouteType.CustomRoute:

                        position.y += increment;
                        EditorGUI.PropertyField(position, customRoute);
                        break;
                }

                EditorGUI.indentLevel--;
            // }

            EditorGUI.EndProperty();
        }
    }
}
