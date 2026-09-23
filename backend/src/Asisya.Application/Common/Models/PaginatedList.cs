using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Common.Models;

/// <summary>
/// Generic paginated response envelope providing items and pagination metadata.
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

    /// <summary>
    /// Creates a paginated list asynchronously from an EF Core IQueryable source.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
