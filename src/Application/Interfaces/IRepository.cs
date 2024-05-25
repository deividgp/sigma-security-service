namespace Application.Interfaces;

public interface IRepository<T, TId>
    where T : class, IEntity<TId>
{
    public Task<IQueryable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = default,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = default
    );
    public Task<T?> GetFirstAsync(Expression<Func<T, bool>>? predicate = default);
    public Task<T?> GetSingleAsync(Expression<Func<T, bool>>? predicate = default);
    public Task<T?> GetByIdAsync(TId id);
    public Task<T> CreateAsync(T entity);
    public Task UpdateAsync(T entity);
    public Task RemoveAsync(TId id);
    public Task UpdateOneAsync(Expression<Func<T, bool>> predicate, object update);
    public Task UpdateManyAsync(Expression<Func<T, bool>> predicate, object update);
}
