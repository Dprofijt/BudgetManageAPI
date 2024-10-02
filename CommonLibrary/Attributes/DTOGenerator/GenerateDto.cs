using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Attributes.DTOGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateDto : Attribute
    {
        public bool UseDynamic { get; set; }
        public string[] ClassNames { get; set; }

        public GenerateDto(params string[] classNames)
        {
            ClassNames = classNames;
        }
        public GenerateDto(bool useDynamic, params string[] classNames)
        {
            UseDynamic = useDynamic;
            ClassNames = classNames;
        }

    }
}
