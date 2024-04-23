using UnityEditor;
using UnityEngine;

namespace ARLocation.MapboxRoutes
{
    [CustomPropertyDrawer(typeof(RouteWaypoint))]
    public class RouteWaypointPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty type;
        private SerializedProperty location;
        private SerializedProperty query;

        public void FindSerializedProperties(SerializedProperty property)
        {
            type = property.FindPropertyRelative("Type");
            location = property.FindPropertyRelative("Location");
            query = property.FindPropertyRelative("Query");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            var lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var height = lineHeight;

            if (!property.isExpanded)
            {
                return height;
            }

            height += lineHeight;

            switch (type.enumValueIndex)
            {
                case (int)RouteWaypointType.Location:
                    height += EditorGUI.GetPropertyHeight(location);
                    break;

                case (int)RouteWaypointType.Query:
                    height += lineHeight;
                    break;

                case (int)RouteWaypointType.UserLocation:
                    break;
            }

            //return base.GetPropertyHeight(property, label);
            return height;

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            EditorGUI.BeginProperty(position, label, property);

            var increment = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.height = EditorGUIUtility.singleLineHeight;
            var indentRect = EditorGUI.IndentedRect(position);

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(indentRect, property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                position.y += increment;
                EditorGUI.PropertyField(position, type);

                switch (type.enumValueIndex)
                {
                    case (int)RouteWaypointType.Location:
                        position.y += increment;
                        EditorGUI.PropertyField(position, location, true);
                        break;

                    case (int)RouteWaypointType.Query:
                        position.y += increment;
                        EditorGUI.PropertyField(position, query);
                        break;

                    case (int)RouteWaypointType.UserLocation:
                        break;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
        }
    }

}
