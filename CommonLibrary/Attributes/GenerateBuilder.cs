using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateBuilder : Attribute
    {
        public string[] ClassNames;

        public GenerateBuilder(params  string[] classNames)
        {
            ClassNames = classNames;
        }
    }
}
