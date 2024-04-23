using UnityEngine;
using UnityEditor;

namespace ARLocation
{
    [CustomPropertyDrawer(typeof(ConditionalPropertyAttribute))]
    public class ConditionalPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (ConditionalPropertyAttribute) attribute;
            var name = conditionalAttribute.Name;

            var path = property.propertyPath;
            var prop = property.serializedObject.FindProperty(path.Replace(property.name, name));

            if (prop != null)
            {
                if (prop.boolValue)
                {
                    EditorGUI.PropertyField(position, property);
                }
            }
        }
    }
}
