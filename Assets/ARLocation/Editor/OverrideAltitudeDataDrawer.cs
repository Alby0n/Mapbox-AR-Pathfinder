using UnityEngine;
using UnityEditor;

namespace ARLocation
{
    [CustomPropertyDrawer(typeof(OverrideAltitudeData))]
    public class OverrideAltitudeDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var initialRect = EditorGUI.IndentedRect(position); //position;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);



            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            // EditorGUI.IndentedRect(position);

            float height = 20.0f;

            var boolRect = new Rect(position.x, position.y, 30, height);
            var altitudeRect = new Rect(position.x, position.y + 20, 180, height);
            var altitudeLabelRect = new Rect(initialRect.x, position.y + height, 50, height);

            var altitudeModeRect = new Rect(position.x, position.y + (2 * height), 180, height);
            var altitudeModeLabelRect = new Rect(initialRect.x, position.y + (2 * height), 50, height);

            EditorGUI.PropertyField(boolRect, property.FindPropertyRelative("OverrideAltitude"), GUIContent.none);

            if (property.FindPropertyRelative("OverrideAltitude").boolValue)
            {
                var x = new GUIContent();
                var y = new GUIContent();
                x.text = "Altitude";
                EditorGUI.PrefixLabel(altitudeLabelRect, x);
                // EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                EditorGUI.PropertyField(altitudeRect, property.FindPropertyRelative("Altitude"), GUIContent.none);

                y.text = "Altitude Mode";
                EditorGUI.PrefixLabel(altitudeModeLabelRect, y);
                EditorGUI.PropertyField(altitudeModeRect, property.FindPropertyRelative("AltitudeMode"), GUIContent.none);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property,
                                               GUIContent label)
        {
            if (property.FindPropertyRelative("OverrideAltitude").boolValue)
            {
                return base.GetPropertyHeight(property, label) * 2 + 20;
            }
            else
            {
                return base.GetPropertyHeight(property, label); // * 2 + 20;
            }
            // Height is two times the standard height plus 20 pixels
        }
    }
}
