using Asisya.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/> streams,
/// providing asynchronous pagination capabilities decoupled from domain models.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Executes pagination asynchronously on an <see cref="IQueryable{T}"/> source.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="source">The queryable data source.</param>
    /// <param name="pageIndex">The requested 1-based page index.</param>
    /// <param name="pageSize">The requested number of records per page.</param>
    /// <param name="cancellationToken">Cancellation token to cancel asynchronous database operations.</param>
    /// <returns>A new <see cref="PaginatedList{T}"/> instance with items and pagination metadata.</returns>
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
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
