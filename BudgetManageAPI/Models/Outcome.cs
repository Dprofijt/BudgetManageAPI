using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;

namespace BudgetManageAPI.Models
{
    public class Outcome : ICashFlow
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCatagory { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
    }
}
