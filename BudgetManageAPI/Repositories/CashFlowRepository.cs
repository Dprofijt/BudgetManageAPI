using BudgetManageAPI.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BudgetManageAPI.Repositories
{
    public interface IRepository<T> where T : ICashFlow
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public class CashFlowRepository<T> : IRepository<T> where T : class, ICashFlow
    {
        private readonly ConcurrentDictionary<int, T> _items = new ConcurrentDictionary<int, T>();
        private int _currentId = 1;

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_items.Values.AsEnumerable());
        }

        public Task<T?> GetByIdAsync(int id)
        {
            _items.TryGetValue(id, out var item);
            return Task.FromResult(item);
        }

        public Task AddAsync(T entity)
        {
            entity.Id = _currentId++;
            _items[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            _items[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            _items.TryRemove(id, out _);
            return Task.CompletedTask;
        }


        //private readonly DbContext _context;
        //private readonly DbSet<T> _dbSet;

        //public Repository(DbContext context)
        //{
        //    _context = context;
        //    _dbSet = _context.Set<T>();
        //}

        //public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        //public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        //public async Task AddAsync(T entity)
        //{
        //    await _dbSet.AddAsync(entity);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task UpdateAsync(T entity)
        //{
        //    _dbSet.Update(entity);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task DeleteAsync(int id)
        //{
        //    var entity = await GetByIdAsync(id);
        //    if (entity != null)
        //    {
        //        _dbSet.Remove(entity);
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }

}
