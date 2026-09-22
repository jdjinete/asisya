using System.Diagnostics;
using Asisya.Application.Common.Interfaces;
using Asisya.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// High-performance MediatR handler for <see cref="BulkCreateProductsCommand"/>.
/// 
/// Batching Strategy:
/// 1. Transactional Boundary: Wraps the entire operation inside a single atomic database transaction.
/// 2. Chunking (1,000 items per roundtrip): Avoids reaching PostgreSQL parameter limits (65,535 max)
///    and prevents Large Object Heap (LOH) memory allocation spikes.
/// 3. Change Tracker Eviction: Invokes <see cref="IApplicationDbContext.ClearChangeTracker"/> after
///    each chunk save. Without clearing, EF Core's ChangeTracker retains tracked entity snapshots,
///    causing O(N^2) complexity on subsequent DetectChanges cycles and consuming hundreds of megabytes
///    when inserting 100,000 records.
/// 4. Category Integrity: Ensures core categories ('SERVIDORES' and 'CLOUD') exist in database
///    to satisfy referential integrity constraints.
/// </summary>
public class BulkCreateProductsCommandHandler : IRequestHandler<BulkCreateProductsCommand, BulkCreateProductsResult>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkCreateProductsCommandHandler"/> class.
    /// </summary>
    public BulkCreateProductsCommandHandler(IApplicationDbContext _context)
    {
        this._context = _context;
    }

    /// <inheritdoc />
    public async Task<BulkCreateProductsResult> Handle(BulkCreateProductsCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();

        // Ensure baseline categories exist for foreign key compliance
        var (servidoresId, cloudId) = await EnsureDefaultCategoriesExistAsync(cancellationToken);

        int totalToProcess = 0;
        int successfulImports = 0;
        int failedImports = 0;

        var batchSize = request.BatchSize > 0 ? request.BatchSize : 1000;

        using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.GenerateRandomCount.HasValue && request.GenerateRandomCount.Value > 0)
            {
                totalToProcess = request.GenerateRandomCount.Value;
                var random = new Random(42); // Deterministic seed for reproducible testing
                var categories = new[] { servidoresId, cloudId };
                var serverPrefixes = new[] { "Dell PowerEdge", "HP ProLiant", "Lenovo ThinkSystem", "Cisco UCS", "Supermicro" };
                var cloudPrefixes = new[] { "AWS EC2 Instance", "Azure VM Standard", "GCP Compute Engine", "Kubernetes Node Pod", "Cloud Dedicated Host" };

                var currentChunk = new List<Product>(batchSize);

                for (int i = 1; i <= totalToProcess; i++)
                {
                    var isCloud = (i % 2 == 0);
                    var categoryId = isCloud ? cloudId : servidoresId;
                    var prefix = isCloud 
                        ? cloudPrefixes[random.Next(cloudPrefixes.Length)] 
                        : serverPrefixes[random.Next(serverPrefixes.Length)];

                    var product = new Product
                    {
                        ProductName = $"{prefix} - Gen{random.Next(10, 16)} #{i:D6}",
                        CategoryId = categoryId,
                        UnitPrice = Math.Round((decimal)(random.NextDouble() * 5000 + 100), 2),
                        UnitsInStock = (short)random.Next(0, 500),
                        UnitsOnOrder = (short)random.Next(0, 50),
                        ReorderLevel = 10,
                        Discontinued = (random.Next(0, 100) < 5), // 5% discontinued
                        QuantityPerUnit = isCloud ? "1 vCPU / 4GB RAM hourly" : "Rack 1U chassis"
                    };

                    currentChunk.Add(product);

                    if (currentChunk.Count >= batchSize)
                    {
                        await _context.Products.AddRangeAsync(currentChunk, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                        _context.ClearChangeTracker();
                        successfulImports += currentChunk.Count;
                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    await _context.Products.AddRangeAsync(currentChunk, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    _context.ClearChangeTracker();
                    successfulImports += currentChunk.Count;
                    currentChunk.Clear();
                }
            }
            else if (request.Products != null && request.Products.Count > 0)
            {
                totalToProcess = request.Products.Count;
                var currentChunk = new List<Product>(batchSize);

                foreach (var item in request.Products)
                {
                    if (string.IsNullOrWhiteSpace(item.ProductName))
                    {
                        failedImports++;
                        errors.Add("Encountered product item with empty or null ProductName.");
                        continue;
                    }

                    var product = new Product
                    {
                        ProductName = item.ProductName.Trim(),
                        CategoryId = item.CategoryId ?? servidoresId,
                        SupplierId = item.SupplierId,
                        UnitPrice = item.UnitPrice,
                        UnitsInStock = item.UnitsInStock ?? 0,
                        Discontinued = item.Discontinued,
                        QuantityPerUnit = item.QuantityPerUnit
                    };

                    currentChunk.Add(product);

                    if (currentChunk.Count >= batchSize)
                    {
                        await _context.Products.AddRangeAsync(currentChunk, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                        _context.ClearChangeTracker();
                        successfulImports += currentChunk.Count;
                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    await _context.Products.AddRangeAsync(currentChunk, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    _context.ClearChangeTracker();
                    successfulImports += currentChunk.Count;
                    currentChunk.Clear();
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            errors.Add($"Transaction rolled back due to error: {ex.Message}");
            throw;
        }

        stopwatch.Stop();

        return new BulkCreateProductsResult
        {
            TotalProcessed = totalToProcess,
            SuccessfulImports = successfulImports,
            FailedImports = failedImports,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            Errors = errors
        };
    }

    private async Task<(int ServidoresId, int CloudId)> EnsureDefaultCategoriesExistAsync(CancellationToken cancellationToken)
    {
        var servidores = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryName == "SERVIDORES", cancellationToken);

        if (servidores == null)
        {
            servidores = new Category
            {
                CategoryName = "SERVIDORES",
                Description = "Dedicated on-premises enterprise rack and blade server units"
            };
            _context.Categories.Add(servidores);
        }

        var cloud = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryName == "CLOUD", cancellationToken);

        if (cloud == null)
        {
            cloud = new Category
            {
                CategoryName = "CLOUD",
                Description = "Scalable cloud instances, virtual machines, and managed cloud infrastructure"
            };
            _context.Categories.Add(cloud);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ClearChangeTracker();

        return (servidores.CategoryId, cloud.CategoryId);
    }
}
