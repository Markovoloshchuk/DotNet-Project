using System.Linq.Expressions;

namespace Catalog.Domain.Specifications;

public abstract class BaseSpecification<TEntity>
    : ISpecification<TEntity>
{
    private readonly List<Expression<Func<TEntity, object>>> _includes = [];

    public Expression<Func<TEntity, bool>>? Criteria { get; }

    public IReadOnlyList<Expression<Func<TEntity, object>>> Includes =>
        _includes;

    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

    public Expression<Func<TEntity, object>>? OrderByDescending
        { get; private set; }

    public int? Skip { get; private set; }

    public int? Take { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    protected BaseSpecification()
    {
    }

    protected BaseSpecification(
        Expression<Func<TEntity, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        Criteria = criteria;
    }

    protected void AddInclude(
        Expression<Func<TEntity, object>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(includeExpression);

        _includes.Add(includeExpression);
    }

    protected void ApplyOrderBy(
        Expression<Func<TEntity, object>> orderByExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByExpression);

        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    protected void ApplyOrderByDescending(
        Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByDescendingExpression);

        OrderByDescending = orderByDescendingExpression;
        OrderBy = null;
    }

    protected void ApplyPaging(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}