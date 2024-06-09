namespace BudgetManageAPI.Interfaces
{
    public interface ICashFlow
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
