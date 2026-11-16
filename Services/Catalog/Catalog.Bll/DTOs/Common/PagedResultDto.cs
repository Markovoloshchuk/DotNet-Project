
namespace Catalog.Bll.DTOs.Common;

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; }
        = Array.Empty<T>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
