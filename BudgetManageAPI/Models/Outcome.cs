using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;
using CommonLibrary.Attributes;

namespace BudgetManageAPI.Models
{
    [AutoGenerateProperties(
        "UserId:string:false",
        "Amount:decimal:true"
        )]
    public partial class Outcome : AutoGeneratableCashFlowBase, ICashFlow
    {
        public int Id { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCatagory { get; set; }
        public DateTime Date { get; set; }
        //public string UserId { get; set; }
    }
}
