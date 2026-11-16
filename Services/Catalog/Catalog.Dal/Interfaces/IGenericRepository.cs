namespace Catalog.Dal.Interfaces;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(
        IReadOnlyList<object> keyValues,
        CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken ct = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken ct = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}