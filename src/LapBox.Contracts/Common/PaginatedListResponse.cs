namespace LapBox.Contracts.Common;

/// <summary>
/// Standard paginated list response for all list endpoints.
/// </summary>
public class PaginatedListResponse<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyCollection<T>? Items { get; init; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
