using CommonLibrary.Attributes;
using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;

namespace BudgetManageAPI.Models
{

    [AutoGenerateProperties(
    "Name:string:true",
    "TestInt2:int:false"
        )]
    public partial class Income : AutoGeneratableCashFlowBase, ICashFlow
    {
        [Sensitive]
        public int Id { get; set; }
        //public string? Description { get; set; }

        public MoneyIncomeCatagory MoneyIncomeCatagory { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        [Sensitive]
        public string UserId { get; set; }

        // Ensure the Amount is never negative
        public bool IsValidAmount => Amount >= 0;

        // Computed property based on income category
        public string IncomeCategoryDisplay => MoneyIncomeCatagory.ToString();

        // Mask sensitive data (e.g., UserId)
        public string MaskedUserId => string.IsNullOrEmpty(UserId) ? "" : "****" + UserId[^4..];

        // Custom validation logic to ensure Date is not in the future
        public bool IsValidDate() => Date <= DateTime.Now;
    }
}