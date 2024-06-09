namespace BudgetManageAPI.Repositories
{
    public interface IBaseRepository
    {
        T Get<T>(int name);
        void Save<T>(T entity);
    }
}
