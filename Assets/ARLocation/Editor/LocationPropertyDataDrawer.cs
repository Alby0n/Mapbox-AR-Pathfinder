using UnityEditor;
using UnityEngine;

namespace ARLocation
{
    [CustomPropertyDrawer(typeof(LocationPropertyData))]
    public class LocationPropertyDataDrawer : PropertyDrawer
    {
        private SerializedProperty type;
        private SerializedProperty location;
        private SerializedProperty locationData;
        private SerializedProperty overrideAltitudeData;

        public void FindSerializedProperties(SerializedProperty property)
        {
            type = property.FindPropertyRelative("LocationInputType");
            location = property.FindPropertyRelative("Location");
            locationData = property.FindPropertyRelative("LocationData");
            overrideAltitudeData = property.FindPropertyRelative("OverrideAltitudeData");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            var height = EditorGUIUtility.singleLineHeight;

            if (type.enumValueIndex == (int) LocationPropertyData.LocationPropertyType.Location)
            {
                height += EditorGUI.GetPropertyHeight(location);
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUI.GetPropertyHeight(overrideAltitudeData, includeChildren: true);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindSerializedProperties(property);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, type, includeChildren:true);

            position.y += EditorGUIUtility.singleLineHeight;

            if (type.enumValueIndex == (int) LocationPropertyData.LocationPropertyType.Location)
            {
                EditorGUI.PropertyField(position, location, includeChildren:true);
            }
            else
            {
                EditorGUI.PropertyField(position, locationData, includeChildren:true);
                position.y += EditorGUI.GetPropertyHeight(locationData, includeChildren: true);
                EditorGUI.PropertyField(position, overrideAltitudeData, includeChildren: true);
            }

            EditorGUI.EndProperty();
        }
    }
}
