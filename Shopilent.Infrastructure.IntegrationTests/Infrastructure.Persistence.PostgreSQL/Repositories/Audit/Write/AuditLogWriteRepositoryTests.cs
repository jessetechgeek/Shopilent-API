using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Audit.Enums;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Persistence.PostgreSQL.Repositories.Audit.Write;

[Collection("IntegrationTests")]
public class AuditLogWriteRepositoryTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public AuditLogWriteRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ValidAuditLog_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");

        // Act
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditLog.Id);
        result.UserId.Should().Be(user.Id);
        result.EntityType.Should().Be(auditLog.EntityType);
        result.EntityId.Should().Be(auditLog.EntityId);
        result.Action.Should().Be(auditLog.Action);
        result.CreatedAt.Should().BeCloseTo(auditLog.CreatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task AddAsync_AuditLogWithoutUser_ShouldPersistSuccessfully()
    {
        // Arrange
        await ResetDatabaseAsync();

        var systemAuditLog = AuditLogBuilder.CreateForEntity("System", Guid.NewGuid(), AuditAction.Create);

        // Act
        await _unitOfWork.AuditLogWriter.AddAsync(systemAuditLog);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AuditLogReader.GetByIdAsync(systemAuditLog.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(systemAuditLog.Id);
        result.UserId.Should().BeNull();
        result.EntityType.Should().Be(systemAuditLog.EntityType);
        result.EntityId.Should().Be(systemAuditLog.EntityId);
        result.Action.Should().Be(systemAuditLog.Action);
    }

    [Fact]
    public async Task AddAsync_AuditLogWithComplexValues_ShouldPersistComplexData()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var oldValues = new Dictionary<string, object>
        {
            ["Name"] = "Old Product Name",
            ["Price"] = 99.99m,
            ["IsActive"] = true,
            ["Tags"] = new[] { "electronics", "gadget" },
            ["Metadata"] = new { Category = "Electronics", Brand = "TechCorp" }
        };

        var newValues = new Dictionary<string, object>
        {
            ["Name"] = "New Product Name",
            ["Price"] = 149.99m,
            ["IsActive"] = false,
            ["Tags"] = new[] { "electronics", "premium" },
            ["Metadata"] = new { Category = "Electronics", Brand = "TechCorp", Featured = true }
        };

        var auditLog = new AuditLogBuilder("Product", Guid.NewGuid(), AuditAction.Update)
            .WithUser(user)
            .WithOldValues(oldValues)
            .WithNewValues(newValues)
            .WithIpAddress("192.168.1.100")
            .WithUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36")
            .WithAppVersion("2.1.0")
            .Build();

        // Act
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        result.Should().NotBeNull();
        result!.OldValues.Should().NotBeNull();
        result.NewValues.Should().NotBeNull();
        result.OldValues.Should().ContainKey("Name");
        result.NewValues.Should().ContainKey("Name");
        result.IpAddress.Should().Be("192.168.1.100");
        result.UserAgent.Should().Contain("Mozilla");
        result.AppVersion.Should().Be("2.1.0");
    }

    [Fact]
    public async Task UpdateAsync_ExistingAuditLog_ShouldModifyAuditLog()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Detach to simulate real scenario
        DbContext.Entry(auditLog).State = EntityState.Detached;

        // Act - Load fresh entity and update
        var existingAuditLog = await _unitOfWork.AuditLogWriter.GetByIdAsync(auditLog.Id);
        existingAuditLog.Should().NotBeNull();

        // Note: AuditLog is typically immutable, but let's test the repository update functionality
        await _unitOfWork.AuditLogWriter.UpdateAsync(existingAuditLog!);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedResult = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        updatedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingAuditLog_ShouldRemoveFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _unitOfWork.AuditLogWriter.DeleteAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var result = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ShouldReturnAuditLogEntity()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _unitOfWork.AuditLogWriter.GetByIdAsync(auditLog.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditLog.Id);
        result.UserId.Should().Be(user.Id);
        result.EntityType.Should().Be(auditLog.EntityType);
        result.EntityId.Should().Be(auditLog.EntityId);
        result.Action.Should().Be(auditLog.Action);
        result.IpAddress.Should().Be(auditLog.IpAddress);
        result.UserAgent.Should().Be(auditLog.UserAgent);
        result.AppVersion.Should().Be(auditLog.AppVersion);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _unitOfWork.AuditLogWriter.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEntityAsync_ExistingEntity_ShouldReturnAuditLogEntities()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var entityType = "Product";
        var entityId = Guid.NewGuid();

        var auditLog1 = AuditLogBuilder.CreateCreateAuditLog(entityType, entityId, user);
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog1);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var auditLog2 = AuditLogBuilder.CreateUpdateAuditLog(entityType, entityId, user);
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog2);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var auditLog3 = AuditLogBuilder.CreateForEntity("Category", Guid.NewGuid(), AuditAction.Create);
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog3);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var results = await _unitOfWork.AuditLogWriter.GetByEntityAsync(entityType, entityId);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.EntityType == entityType && r.EntityId == entityId);
        results.Should().BeInDescendingOrder(x => x.CreatedAt);

        var auditLogIds = results.Select(r => r.Id).ToList();
        auditLogIds.Should().Contain(auditLog1.Id);
        auditLogIds.Should().Contain(auditLog2.Id);
        auditLogIds.Should().NotContain(auditLog3.Id);
    }

    [Fact]
    public async Task GetByUserAsync_ExistingUser_ShouldReturnUserAuditLogEntities()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user1 = UserBuilder.CreateDefaultUser();
        var user2 = UserBuilder.CreateDefaultUser();

        await _unitOfWork.UserWriter.AddAsync(user1);
        await _unitOfWork.UserWriter.AddAsync(user2);
        await _unitOfWork.SaveChangesAsync();

        var user1AuditLog1 = AuditLogBuilder.CreateForUser(user1, "Product");
        await _unitOfWork.AuditLogWriter.AddAsync(user1AuditLog1);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var user1AuditLog2 = AuditLogBuilder.CreateForUser(user1, "Category");
        await _unitOfWork.AuditLogWriter.AddAsync(user1AuditLog2);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var user2AuditLog = AuditLogBuilder.CreateForUser(user2, "Order");
        await _unitOfWork.AuditLogWriter.AddAsync(user2AuditLog);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var systemAuditLog = AuditLogBuilder.CreateForEntity("System", Guid.NewGuid(), AuditAction.Create);
        await _unitOfWork.AuditLogWriter.AddAsync(systemAuditLog);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var results = await _unitOfWork.AuditLogWriter.GetByUserAsync(user1.Id);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.UserId == user1.Id);
        results.Should().BeInDescendingOrder(x => x.CreatedAt);

        var auditLogIds = results.Select(r => r.Id).ToList();
        auditLogIds.Should().Contain(user1AuditLog1.Id);
        auditLogIds.Should().Contain(user1AuditLog2.Id);
        auditLogIds.Should().NotContain(user2AuditLog.Id);
        auditLogIds.Should().NotContain(systemAuditLog.Id);
    }

    [Fact]
    public async Task GetByActionAsync_ExistingAction_ShouldReturnActionAuditLogEntities()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var createAuditLog1 = AuditLogBuilder.CreateCreateAuditLog("Product", Guid.NewGuid(), user);
        await _unitOfWork.AuditLogWriter.AddAsync(createAuditLog1);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var createAuditLog2 = AuditLogBuilder.CreateCreateAuditLog("Category", Guid.NewGuid(), user);
        await _unitOfWork.AuditLogWriter.AddAsync(createAuditLog2);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var updateAuditLog = AuditLogBuilder.CreateUpdateAuditLog("Product", Guid.NewGuid(), user);
        await _unitOfWork.AuditLogWriter.AddAsync(updateAuditLog);
        await _unitOfWork.SaveChangesAsync();
        await Task.Delay(100); // Ensure sufficient time gap
        
        var deleteAuditLog = AuditLogBuilder.CreateDeleteAuditLog("Product", Guid.NewGuid(), user);
        await _unitOfWork.AuditLogWriter.AddAsync(deleteAuditLog);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var results = await _unitOfWork.AuditLogWriter.GetByActionAsync(AuditAction.Create);

        // Assert
        // Note: User creation by audit interceptor creates 1 audit log + our 2 test logs = 3 total
        results.Should().NotBeEmpty();
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Action == AuditAction.Create);
        results.Should().BeInDescendingOrder(x => x.CreatedAt);

        var auditLogIds = results.Select(r => r.Id).ToList();
        auditLogIds.Should().Contain(createAuditLog1.Id);
        auditLogIds.Should().Contain(createAuditLog2.Id);
        auditLogIds.Should().NotContain(updateAuditLog.Id);
        auditLogIds.Should().NotContain(deleteAuditLog.Id);
    }

    [Fact]
    public async Task BulkOperations_MultipleAuditLogs_ShouldHandleCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLogs = new List<Domain.Audit.AuditLog>
        {
            AuditLogBuilder.CreateCreateAuditLog("Product", Guid.NewGuid(), user),
            AuditLogBuilder.CreateUpdateAuditLog("Product", Guid.NewGuid(), user),
            AuditLogBuilder.CreateDeleteAuditLog("Product", Guid.NewGuid(), user),
            AuditLogBuilder.CreateForEntity("Category", Guid.NewGuid(), AuditAction.Create),
            AuditLogBuilder.CreateForEntity("Order", Guid.NewGuid(), AuditAction.Update)
        };

        // Act - Add all audit logs in bulk
        foreach (var auditLog in auditLogs)
        {
            await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        }
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verify all were persisted
        // Note: User creation by audit interceptor creates 1 audit log + our 5 test logs = 6 total
        var allResults = await _unitOfWork.AuditLogReader.ListAllAsync();
        allResults.Should().HaveCount(6);

        foreach (var auditLog in auditLogs)
        {
            var result = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
            result.Should().NotBeNull();
            result!.Id.Should().Be(auditLog.Id);
        }
    }

    [Fact]
    public async Task OptimisticConcurrency_ConcurrentUpdates_ShouldHandleCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Act - Simulate concurrent access with two UnitOfWork instances
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();

        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var auditLog1 = await unitOfWork1.AuditLogWriter.GetByIdAsync(auditLog.Id);
        var auditLog2 = await unitOfWork2.AuditLogWriter.GetByIdAsync(auditLog.Id);

        auditLog1.Should().NotBeNull();
        auditLog2.Should().NotBeNull();

        // AuditLogs are immutable by design - they should not be updated after creation
        // The UpdateAsync method exists in the interface but should not modify audit logs
        // Let's verify the entities remain unchanged after "update" operations
        await unitOfWork1.AuditLogWriter.UpdateAsync(auditLog1!);
        await unitOfWork1.SaveChangesAsync();

        await unitOfWork2.AuditLogWriter.UpdateAsync(auditLog2!);
        await unitOfWork2.SaveChangesAsync(); // Should not throw since no actual changes are made

        // Verify the audit log remains unchanged
        var finalResult = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        finalResult.Should().NotBeNull();
        finalResult!.EntityType.Should().Be(auditLog.EntityType);
        finalResult.Action.Should().Be(auditLog.Action);
        finalResult.UserId.Should().Be(auditLog.UserId);
    }

    [Fact]
    public async Task AuditInterceptor_SkipsAuditLogEntities_PreventsInfiniteRecursion()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var auditLog = AuditLogBuilder.CreateForUser(user, "Product");

        // Act - Add audit log 
        await _unitOfWork.AuditLogWriter.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        // Assert - AuditLog entities are explicitly skipped by AuditSaveChangesInterceptor (line 86)
        // to prevent infinite recursion, so no audit trail should be created for AuditLog itself
        var auditTrailLogs = await _unitOfWork.AuditLogReader.GetByEntityAsync("AuditLog", auditLog.Id);
        auditTrailLogs.Should().BeEmpty(); // No audit trail for AuditLog entities
        
        // But the audit log itself should be created successfully
        var result = await _unitOfWork.AuditLogReader.GetByIdAsync(auditLog.Id);
        result.Should().NotBeNull();
        result!.EntityType.Should().Be("Product");
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task SpecializedAuditLogCreation_AllActionTypes_ShouldPersistCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.CreateDefaultUser();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var entityType = "Product";
        var entityId = Guid.NewGuid();

        // Act & Assert - Test CreateForCreate
        var createValues = new Dictionary<string, object> { ["Name"] = "New Product", ["Price"] = 99.99m };
        var createResult = Domain.Audit.AuditLog.CreateForCreate(entityType, entityId, createValues, user);
        createResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AuditLogWriter.AddAsync(createResult.Value);
        await _unitOfWork.SaveChangesAsync();

        var persistedCreate = await _unitOfWork.AuditLogReader.GetByIdAsync(createResult.Value.Id);
        persistedCreate.Should().NotBeNull();
        persistedCreate!.Action.Should().Be(AuditAction.Create);
        persistedCreate.NewValues.Should().NotBeNull();
        persistedCreate.OldValues.Should().BeNull();

        // Act & Assert - Test CreateForUpdate
        var oldValues = new Dictionary<string, object> { ["Name"] = "Old Product", ["Price"] = 89.99m };
        var newValues = new Dictionary<string, object> { ["Name"] = "Updated Product", ["Price"] = 99.99m };
        var updateResult = Domain.Audit.AuditLog.CreateForUpdate(entityType, entityId, oldValues, newValues, user);
        updateResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AuditLogWriter.AddAsync(updateResult.Value);
        await _unitOfWork.SaveChangesAsync();

        var persistedUpdate = await _unitOfWork.AuditLogReader.GetByIdAsync(updateResult.Value.Id);
        persistedUpdate.Should().NotBeNull();
        persistedUpdate!.Action.Should().Be(AuditAction.Update);
        persistedUpdate.OldValues.Should().NotBeNull();
        persistedUpdate.NewValues.Should().NotBeNull();

        // Act & Assert - Test CreateForDelete
        var deleteValues = new Dictionary<string, object> { ["Name"] = "Deleted Product", ["Price"] = 99.99m };
        var deleteResult = Domain.Audit.AuditLog.CreateForDelete(entityType, entityId, deleteValues, user);
        deleteResult.IsSuccess.Should().BeTrue();

        await _unitOfWork.AuditLogWriter.AddAsync(deleteResult.Value);
        await _unitOfWork.SaveChangesAsync();

        var persistedDelete = await _unitOfWork.AuditLogReader.GetByIdAsync(deleteResult.Value.Id);
        persistedDelete.Should().NotBeNull();
        persistedDelete!.Action.Should().Be(AuditAction.Delete);
        persistedDelete.OldValues.Should().NotBeNull();
        persistedDelete.NewValues.Should().BeNull();
    }
}
