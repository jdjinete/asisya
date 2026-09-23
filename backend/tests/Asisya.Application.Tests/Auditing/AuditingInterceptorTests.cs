using Asisya.Application.Common.Interfaces;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using Asisya.Infrastructure.Persistence.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Asisya.Application.Tests.Auditing;

public class AuditingInterceptorTests : IDisposable
{
    private readonly ICurrentUserService _currentUserService;
    private readonly AuditableEntitySaveChangesInterceptor _interceptor;
    private readonly TestAsisyaDbContext _context;

    public AuditingInterceptorTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("usr-123");
        _currentUserService.UserEmail.Returns("auditor@asisya.com");
        _currentUserService.IsAuthenticated.Returns(true);

        _interceptor = new AuditableEntitySaveChangesInterceptor(_currentUserService);
        _context = TestDbContextFactory.Create(interceptor: _interceptor);
    }

    [Fact]
    public async Task SaveChanges_ShouldCreateInsertAuditLog_WhenCategoryIsAdded()
    {
        // Arrange
        var category = new Category
        {
            CategoryName = "ENTERPRISE SERVERS",
            Description = "High capacity rack server systems"
        };

        _context.Categories.Add(category);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        var auditLogs = await _context.AuditLogs.ToListAsync();
        auditLogs.Should().ContainSingle();

        var log = auditLogs.First();
        log.TableName.Should().Be("Categories");
        log.Action.Should().Be("Insert");
        log.UserId.Should().Be("auditor@asisya.com");
        log.TimestampUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        log.NewValues.Should().Contain("ENTERPRISE SERVERS");
        log.OldValues.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_ShouldCreateUpdateAuditLog_WhenProductIsModified()
    {
        // Arrange
        var category = new Category { CategoryName = "NETWORKING", Description = "Routers & Switches" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            ProductName = "Core Switch 48-Port",
            CategoryId = category.CategoryId,
            UnitPrice = 1500.00m,
            UnitsInStock = 10
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Clear existing insert audit logs for clean update testing
        var initialLogsCount = await _context.AuditLogs.CountAsync();

        // Act: modify unit price and units in stock
        product.UnitPrice = 1750.50m;
        product.UnitsInStock = 8;
        await _context.SaveChangesAsync();

        // Assert
        var logs = await _context.AuditLogs.ToListAsync();
        logs.Count.Should().BeGreaterThan(initialLogsCount);

        var updateLog = logs.Last();
        updateLog.TableName.Should().Be("Products");
        updateLog.Action.Should().Be("Update");
        updateLog.UserId.Should().Be("auditor@asisya.com");
        updateLog.OldValues.Should().Contain("1500");
        updateLog.NewValues.Should().Contain("1750.5");
    }

    [Fact]
    public async Task SaveChanges_ShouldCreateDeleteAuditLog_WhenEntityIsRemoved()
    {
        // Arrange
        var category = new Category { CategoryName = "TEMP CATEGORY", Description = "Temporary testing category" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var logs = await _context.AuditLogs.ToListAsync();
        var deleteLog = logs.Last();

        deleteLog.TableName.Should().Be("Categories");
        deleteLog.Action.Should().Be("Delete");
        deleteLog.UserId.Should().Be("auditor@asisya.com");
        deleteLog.OldValues.Should().Contain("TEMP CATEGORY");
    }

    [Fact]
    public async Task SaveChanges_ShouldFallbackToSystemUser_WhenUserNotAuthenticated()
    {
        // Arrange: unauthenticated service context
        _currentUserService.UserId.Returns((string?)null);
        _currentUserService.UserEmail.Returns((string?)null);
        _currentUserService.IsAuthenticated.Returns(false);

        var category = new Category { CategoryName = "SYSTEM GENERATED", Description = "Created by batch job" };
        _context.Categories.Add(category);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        var log = await _context.AuditLogs.LastAsync();
        log.UserId.Should().Be("System");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
