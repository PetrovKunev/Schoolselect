using System.Linq.Expressions;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // Основни CRUD операции
        Task<TEntity> GetByIdAsync(object id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        Task<int> CountAsync();
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
