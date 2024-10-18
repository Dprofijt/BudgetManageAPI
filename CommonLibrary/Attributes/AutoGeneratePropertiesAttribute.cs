using System;
using System.Linq;
using System.Reflection;

namespace CommonLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoGeneratePropertiesAttribute : Attribute
    {
        public PropertyInfo[] Properties { get; }

        /// <summary>
        /// Attribute so that the PropertyGenerator knows what to create for the class
        /// </summary>
        /// <param name="properties"> define properties by "name:type:requiredBool:defaultValue"</param>
        public AutoGeneratePropertiesAttribute(params string[] properties)
        {
            // Convert each string to a PropertyInfo instance
            Properties = properties.Select(p =>
            {
                var parts = p.Split(':');
                if (parts.Length >=4)
                {
                    return new PropertyInfo(parts[0], parts[1], bool.Parse(parts[2]), parts[3]);
                }else
                {
                    return new PropertyInfo(parts[0], parts[1], bool.Parse(parts[2]));
                }
            }).ToArray();
        }
    }
    public class PropertyInfo
    {
        public string Name { get; }
        public string Type { get; }
        public bool IsRequired { get; }
        public string DefaultValue{ get; }

        public PropertyInfo(string name, string type, bool isRequired = false, string defaultValue = null)
        {
            Name = name;
            Type = type;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }
    }
}
