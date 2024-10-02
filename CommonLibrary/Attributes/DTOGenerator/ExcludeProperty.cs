using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Attributes.DTOGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcludeProperty : Attribute
    {
        public string[] ClassNames { get; set; }
        public ExcludeProperty(params string[] classNames)
        {
            ClassNames = classNames;
        }
    }
}
