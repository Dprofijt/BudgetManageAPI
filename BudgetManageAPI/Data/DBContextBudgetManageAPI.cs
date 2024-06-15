using Microsoft.EntityFrameworkCore;

namespace BudgetManageAPI.Data
{
    public class DBContextBudgetManageAPI : DbContext 
    {

        public DBContextBudgetManageAPI(DbContextOptions<DBContextBudgetManageAPI> options) : base(options)
        {
        }

        public DbSet<Models.Income> Incomes { get; set; }
        public DbSet<Models.Outcome> Outcomes { get; set; }
        public DbSet<Models.SavingGoal> SavingGoals { get; set; }

        
    }
}
