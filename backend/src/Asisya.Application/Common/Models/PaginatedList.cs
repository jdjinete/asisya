namespace Asisya.Application.Common.Models;

/// <summary>
/// Generic paginated response envelope providing items and pagination metadata.
/// Pure domain/application model decoupled from any data access infrastructure.
/// </summary>
/// <typeparam name="T">The type of items contained in the page.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// The page elements.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Current 1-based page index.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total number of items matching filter criteria across all pages.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Total number of calculated pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// True if there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    /// True if there is a subsequent page available.
    /// </summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    public PaginatedList(IReadOnlyList<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }
}
