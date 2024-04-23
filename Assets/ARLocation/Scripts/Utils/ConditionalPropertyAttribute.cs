using UnityEngine;

namespace ARLocation {
    public class ConditionalPropertyAttribute : PropertyAttribute
    {
        public string Name;

        public ConditionalPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
