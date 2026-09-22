namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// Execution summary result returned after completing a bulk product ingestion job.
/// </summary>
public record BulkCreateProductsResult
{
    /// <summary>
    /// Total number of product items evaluated.
    /// </summary>
    public int TotalProcessed { get; init; }

    /// <summary>
    /// Count of records successfully committed to PostgreSQL.
    /// </summary>
    public int SuccessfulImports { get; init; }

    /// <summary>
    /// Count of items skipped or failed validation.
    /// </summary>
    public int FailedImports { get; init; }

    /// <summary>
    /// Total elapsed time in milliseconds for the batch insertion process.
    /// </summary>
    public long ElapsedMilliseconds { get; init; }

    /// <summary>
    /// Collection of operational or business validation warning messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
