using CommonLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Abstract
{
    [AutoGenerateProperties(
    "Description:string:true",
    "TestInt:int:false"
        )]
    public abstract class AutoGeneratableCashFlowBase
    {
    }
}
