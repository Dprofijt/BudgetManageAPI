using System;
using System.Linq;
using System.Reflection;

namespace CommonLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoGeneratePropertiesAttribute : Attribute
    {
        public PropertyInfo[] Properties { get; }

        public AutoGeneratePropertiesAttribute(params string[] properties)
        {
            // Convert each string to a PropertyInfo instance
            Properties = properties.Select(p =>
            {
                var parts = p.Split(':');
                return new PropertyInfo(parts[0], parts[1], bool.Parse(parts[2]));
            }).ToArray();
        }
    }
    public class PropertyInfo
    {
        public string Name { get; }
        public string Type { get; }
        public bool IsRequired { get; }

        public PropertyInfo(string name, string type, bool isRequired = false)
        {
            Name = name;
            Type = type;
            IsRequired = isRequired;
        }
    }
}
