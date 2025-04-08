using Shopilent.Domain.Audit;
using Shopilent.Domain.Audit.Enums;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;

namespace Shopilent.Domain.Tests.Audit;

public class AuditLogTests
{
    private Email CreateTestEmail()
    {
        return Email.Create("test@example.com").Value;
    }

    private FullName CreateTestFullName()
    {
        return FullName.Create("John", "Doe").Value;
    }

    private User CreateTestUser()
    {
        var userResult = User.Create(
            CreateTestEmail(),
            "hashed_password",
            CreateTestFullName());

        return userResult.Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateAuditLog()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.NewGuid();
        var action = AuditAction.Update;
        var user = CreateTestUser();
        var oldValues = new Dictionary<string, object> { { "Name", "Old Name" } };
        var newValues = new Dictionary<string, object> { { "Name", "New Name" } };
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";
        var appVersion = "1.0.0";

        // Act
        var result = AuditLog.Create(
            entityType,
            entityId,
            action,
            user,
            oldValues,
            newValues,
            ipAddress,
            userAgent,
            appVersion);

        // Assert
        Assert.True(result.IsSuccess);
        var auditLog = result.Value;
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(action, auditLog.Action);
        Assert.Equal(user.Id, auditLog.UserId);
        Assert.Equal(oldValues, auditLog.OldValues);
        Assert.Equal(newValues, auditLog.NewValues);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(appVersion, auditLog.AppVersion);
    }

    [Fact]
    public void Create_WithEmptyEntityType_ShouldReturnFailureResult()
    {
        // Arrange
        var entityType = string.Empty;
        var entityId = Guid.NewGuid();
        var action = AuditAction.Create;

        // Act
        var result = AuditLog.Create(
            entityType,
            entityId,
            action);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("AuditLog.EntityTypeRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyEntityId_ShouldReturnFailureResult()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.Empty;
        var action = AuditAction.Create;

        // Act
        var result = AuditLog.Create(
            entityType,
            entityId,
            action);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("AuditLog.InvalidEntityId", result.Error.Code);
    }

    [Fact]
    public void CreateForCreate_ShouldCreateAuditLogWithCreateAction()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.NewGuid();
        var values = new Dictionary<string, object> { { "Name", "New Product" }, { "Price", 100m } };
        var user = CreateTestUser();
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";
        var appVersion = "1.0.0";

        // Act
        var result = AuditLog.CreateForCreate(
            entityType,
            entityId,
            values,
            user,
            ipAddress,
            userAgent,
            appVersion);

        // Assert
        Assert.True(result.IsSuccess);
        var auditLog = result.Value;
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(AuditAction.Create, auditLog.Action);
        Assert.Equal(user.Id, auditLog.UserId);
        Assert.Null(auditLog.OldValues);
        Assert.Equal(values, auditLog.NewValues);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(appVersion, auditLog.AppVersion);
    }

    [Fact]
    public void CreateForUpdate_ShouldCreateAuditLogWithUpdateAction()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.NewGuid();
        var oldValues = new Dictionary<string, object> { { "Name", "Old Name" }, { "Price", 90m } };
        var newValues = new Dictionary<string, object> { { "Name", "New Name" }, { "Price", 100m } };
        var user = CreateTestUser();
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";
        var appVersion = "1.0.0";

        // Act
        var result = AuditLog.CreateForUpdate(
            entityType,
            entityId,
            oldValues,
            newValues,
            user,
            ipAddress,
            userAgent,
            appVersion);

        // Assert
        Assert.True(result.IsSuccess);
        var auditLog = result.Value;
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(AuditAction.Update, auditLog.Action);
        Assert.Equal(user.Id, auditLog.UserId);
        Assert.Equal(oldValues, auditLog.OldValues);
        Assert.Equal(newValues, auditLog.NewValues);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(appVersion, auditLog.AppVersion);
    }

    [Fact]
    public void CreateForDelete_ShouldCreateAuditLogWithDeleteAction()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.NewGuid();
        var values = new Dictionary<string, object> { { "Name", "Deleted Product" }, { "Price", 100m } };
        var user = CreateTestUser();
        var ipAddress = "127.0.0.1";
        var userAgent = "Test Agent";
        var appVersion = "1.0.0";

        // Act
        var result = AuditLog.CreateForDelete(
            entityType,
            entityId,
            values,
            user,
            ipAddress,
            userAgent,
            appVersion);

        // Assert
        Assert.True(result.IsSuccess);
        var auditLog = result.Value;
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(AuditAction.Delete, auditLog.Action);
        Assert.Equal(user.Id, auditLog.UserId);
        Assert.Equal(values, auditLog.OldValues);
        Assert.Null(auditLog.NewValues);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal(appVersion, auditLog.AppVersion);
    }

    [Fact]
    public void Create_WithoutUser_ShouldHaveNullUserId()
    {
        // Arrange
        var entityType = "Product";
        var entityId = Guid.NewGuid();
        var action = AuditAction.View;

        // Act
        var result = AuditLog.Create(
            entityType,
            entityId,
            action);

        // Assert
        Assert.True(result.IsSuccess);
        var auditLog = result.Value;
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(action, auditLog.Action);
        Assert.Null(auditLog.UserId);
        Assert.Null(auditLog.OldValues);
        Assert.Null(auditLog.NewValues);
        Assert.Null(auditLog.IpAddress);
        Assert.Null(auditLog.UserAgent);
        Assert.Null(auditLog.AppVersion);
    }
}