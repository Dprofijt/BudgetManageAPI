using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;
using CommonLibrary.Attributes;

namespace BudgetManageAPI.Models
{
    public partial class Outcome : AutoGeneratableCashFlowBase, ICashFlow
    {
        public int Id { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCatagory { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
    }
}
