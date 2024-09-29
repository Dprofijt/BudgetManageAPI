using CommonLibrary.Attributes;
using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;
using System.ComponentModel.DataAnnotations;

namespace BudgetManageAPI.Models
{

    [AutoGenerateProperties]
    public partial class Income : AutoGeneratableCashFlowBase, ICashFlow
    {
        [Sensitive]
        [Key]
        public int Id { get; set; }
        //public string? Description { get; set; }

        public MoneyIncomeCatagory MoneyIncomeCatagory { get; set; }
        public DateTime Date { get; set; }
        [Sensitive]
        public string UserId { get; set; }



        // Computed property based on income category
        public string IncomeCategoryDisplay => MoneyIncomeCatagory.ToString();

        // Mask sensitive data (e.g., UserId)
        public string MaskedUserId => string.IsNullOrEmpty(UserId) ? "" : "****" + UserId[^4..];

        // Custom validation logic to ensure Date is not in the future
        public bool IsValidDate() => Date <= DateTime.Now;
    }
}