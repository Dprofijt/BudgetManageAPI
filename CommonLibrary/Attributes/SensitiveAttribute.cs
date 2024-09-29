using System;

namespace CommonLibrary.Attributes
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SensitiveAttribute : Attribute
    {

    }
}
