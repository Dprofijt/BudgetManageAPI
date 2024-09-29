using CommonLibrary.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Abstract
{
    [AutoGenerateProperties(
        "Amount:decimal:true",
    "Description:string:true"
        )]
    public abstract class AutoGeneratableCashFlowBase
    {
    }
}
