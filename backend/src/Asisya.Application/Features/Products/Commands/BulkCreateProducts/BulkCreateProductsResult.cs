namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// Execution summary result returned after accepting or completing a bulk product ingestion job.
/// For asynchronous message queue dispatch, returns the tracking correlation <see cref="BatchId"/>,
/// HTTP 202 Accepted status descriptor, and enqueue timestamp.
/// </summary>
public record BulkCreateProductsResult
{
    /// <summary>
    /// Unique correlation identifier for tracking this batch ingestion process.
    /// </summary>
    public Guid BatchId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Total number of product items evaluated or queued for background processing.
    /// </summary>
    public int TotalProcessed { get; init; }

    /// <summary>
    /// Count of records successfully committed to PostgreSQL (updated upon job completion).
    /// </summary>
    public int SuccessfulImports { get; init; }

    /// <summary>
    /// Count of items skipped or failed validation.
    /// </summary>
    public int FailedImports { get; init; }

    /// <summary>
    /// Total elapsed time in milliseconds for the operation (0 for immediate async dispatch).
    /// </summary>
    public long ElapsedMilliseconds { get; init; }

    /// <summary>
    /// Lifecycle execution status (e.g. "Accepted", "Processing", "Completed").
    /// </summary>
    public string Status { get; init; } = "Accepted";

    /// <summary>
    /// Informational status message describing the outcome.
    /// </summary>
    public string Message { get; init; } = "Bulk product ingestion job enqueued for asynchronous processing.";

    /// <summary>
    /// Timestamp when the batch was received and enqueued in UTC.
    /// </summary>
    public DateTime EnqueuedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of operational or business validation warning messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
