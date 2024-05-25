namespace Infrastructure.Repositories;

public class MongoRepository<T, TId>(ApplicationDbContext context) : IRepository<T, TId>
    where T : class, IEntity<TId>
{
    private readonly IMongoCollection<T> _collection = context.GetCollection<T>();

    public async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public Task<IQueryable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = default,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = default
    )
    {
        IQueryable<T> query = _collection.AsQueryable();

        query = predicate is null ? query : query.Where(predicate);
        query = orderBy is null ? query : orderBy(query);

        return Task.FromResult(query);
    }

    public Task<T?> GetFirstAsync(Expression<Func<T, bool>>? predicate = default)
    {
        IQueryable<T> query = _collection.AsQueryable();

        query = predicate is null ? query : query.Where(predicate);

        return Task.FromResult(query.FirstOrDefault());
    }

    public Task<T?> GetSingleAsync(Expression<Func<T, bool>>? predicate = default)
    {
        IQueryable<T> query = _collection.AsQueryable();

        query = predicate is null ? query : query.Where(predicate);

        return Task.FromResult(query.SingleOrDefault());
    }

    public Task<T?> GetByIdAsync(TId id)
    {
        return Task.FromResult(
            _collection.AsQueryable().Where(x => x.Id!.Equals(id)).SingleOrDefault()
        );
    }

    public async Task RemoveAsync(TId id)
    {
        await _collection.DeleteOneAsync(x => x.Id!.Equals(id));
    }

    public async Task UpdateAsync(T entity)
    {
        await _collection.ReplaceOneAsync(x => x.Id!.Equals(entity.Id), entity);
    }

    public async Task UpdateOneAsync(Expression<Func<T, bool>> predicate, object update)
    {
        await _collection.UpdateOneAsync(predicate, (UpdateDefinition<T>)update);
    }

    public async Task UpdateManyAsync(Expression<Func<T, bool>> predicate, object update)
    {
        await _collection.UpdateManyAsync(predicate, (UpdateDefinition<T>)update);
    }
}
