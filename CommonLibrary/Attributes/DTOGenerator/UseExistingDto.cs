using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Attributes.DTOGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UseExistingDto : Attribute
    {
        public string[] ClassNames { get; set; }
        public UseExistingDto(params string[] classNames)
        {
            ClassNames = classNames;
        }
    }
}
