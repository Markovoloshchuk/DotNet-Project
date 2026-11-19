using Catalog.Dal.Interfaces;
using Catalog.Dal.Persistence;
using Catalog.Dal.Specifications;
using Catalog.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public class GenericRepository<TEntity>
    : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(CatalogDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        IReadOnlyList<object> keyValues,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(keyValues);

        if (keyValues.Count == 0)
        {
            throw new ArgumentException(
                "At least one key value must be provided.",
                nameof(keyValues));
        }

        return await _dbSet.FindAsync(
            keyValues.ToArray(),
            ct);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator<TEntity>.GetQuery(
            _dbSet.AsNoTracking(),
            specification);

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbSet.AddAsync(entity, ct);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Remove(entity);
    }
}