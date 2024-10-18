using CommonLibrary.Attributes;
using BudgetManageAPI.Enums;
using BudgetManageAPI.Interfaces;
using CommonLibrary.Abstract;
using System.ComponentModel.DataAnnotations;
using CommonLibrary.Attributes.DTOGenerator;

namespace BudgetManageAPI.Models
{
    [GenerateDto("IncomeDTO", "IncomeDTOWithNoUser")]
    public partial class Income : AutoGeneratableCashFlowBase, ICashFlow
    {
        [Sensitive]
        [Key]
        public int Id { get; set; }
        //public string? Description { get; set; }

        public MoneyIncomeCatagory MoneyIncomeCatagory { get; set; }
        public DateTime Date { get; set; }
        [ExcludeProperty("IncomeDTOWithNoUser")]
        public string? UserId { get; set; }



        // Computed property based on income category
        //public string IncomeCategoryDisplay => MoneyIncomeCatagory.ToString();

        // Mask sensitive data (e.g., UserId)
        //[ExcludeProperty("IncomeDTOWithNoUser")]
        //public string MaskedUserId => string.IsNullOrEmpty(UserId) ? "" : "****" + UserId[^4..];

        // Custom validation logic to ensure Date is not in the future
        //public bool IsValidDate() => Date <= DateTime.Now;
    }
    public class UserService
    {
        private const string MagicString_Admin = "Admin"; //expect no warning for this line
        private const string MagicString_Active = "Active"; //expect no warning for this line
        public string GetUserRole()
        {
            return "Admin"; // Expect warning for this line
        }

        public string GetUserStatus()
        {
            return MagicString_Active;
        }
    }
}
