﻿using CommonLibrary.Attributes;

namespace BudgetManageAPI.Models
{
    [AutoGenerateProperties(
        "Name:string:true",
        "Description:string:true:Test",
        "TargetAmount:decimal:false",
        "CurrentAmount:decimal:true:6"
        )]
    public partial class SavingGoal
    {
        public int Id { get; set; }
        //public string? Name { get; set; }
        //public string Description { get; set; }

        //public decimal? TargetAmount { get; set; }
        //public decimal CurrentAmount { get; set; }
        public DateTime? DateGoal { get; set; }
    
    }
}
