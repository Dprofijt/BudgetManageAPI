using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;

namespace BudgetManageAPI.Models
{
    public class Income :ICashFlow
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public MoneyIncomeCatagory MoneyIncomeCatagory { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}