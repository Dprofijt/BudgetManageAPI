using System;

namespace BudgetManageAPI.Attributes
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SensitiveAttribute : Attribute
    {

    }
}
