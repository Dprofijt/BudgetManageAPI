using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;
using CommonLibrary.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BudgetManageAPI.Models
{
    [GenerateBuilder]
    public partial class Outcome : AutoGeneratableCashFlowBase, ICashFlow
    {
        [Key]
        public int Id { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCatagory { get; set; }
        public DateTime Date { get; set; }
        [Sensitive]
        public string? UserId { get; set; }
    }
}
