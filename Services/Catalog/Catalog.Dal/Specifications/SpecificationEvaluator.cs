using Catalog.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Specifications;

public static class SpecificationEvaluator<TEntity>
    where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification)
    {
        ArgumentNullException.ThrowIfNull(inputQuery);
        ArgumentNullException.ThrowIfNull(specification);

        IQueryable<TEntity> query = inputQuery;

        // Filtering
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Eager loading
        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        // Sorting
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(
                specification.OrderByDescending);
        }

        // Pagination
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip is null ||
                specification.Take is null)
            {
                throw new InvalidOperationException(
                    "Paging requires both Skip and Take values.");
            }

            query = query
                .Skip(specification.Skip.Value)
                .Take(specification.Take.Value);
        }

        return query;
    }
}