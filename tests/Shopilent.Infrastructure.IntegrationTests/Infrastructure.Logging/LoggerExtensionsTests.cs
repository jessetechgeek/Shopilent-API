using Microsoft.Extensions.Logging;
using Moq;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.Logging.Extensions;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Logging;

[Collection("IntegrationTests")]
public class LoggerExtensionsTests : IntegrationTestBase
{
    private Mock<ILogger> _mockLogger = null!;
    private ILogger<LoggerExtensionsTests> _realLogger = null!;

    public LoggerExtensionsTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _mockLogger = new Mock<ILogger>();
        _realLogger = GetService<ILogger<LoggerExtensionsTests>>();
        return Task.CompletedTask;
    }

    [Fact]
    public void LogUserAction_WithValidParameters_ShouldLogInformationLevel()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var action = "Create Product";
        var details = "Product ID: 12345";

        // Act
        _mockLogger.Object.LogUserAction(action, userId, details);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User Action") && 
                                            v.ToString()!.Contains(action) && 
                                            v.ToString()!.Contains(userId) &&
                                            v.ToString()!.Contains(details)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogUserAction_WithNullDetails_ShouldLogWithEmptyString()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var action = "Update Profile";

        // Act
        _mockLogger.Object.LogUserAction(action, userId, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User Action") && 
                                            v.ToString()!.Contains(action) && 
                                            v.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogUserAction_WithRealLogger_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var action = "Real Logger Test";
        var details = "Integration test";

        // Act & Assert
        var logAction = () => _realLogger.LogUserAction(action, userId, details);
        logAction.Should().NotThrow();
    }

    [Fact]
    public void LogDatabaseOperation_WithValidParameters_ShouldLogInformationLevel()
    {
        // Arrange
        var operation = "INSERT";
        var entity = "Product";
        var details = "ProductId: 123, Name: Test Product";

        // Act
        _mockLogger.Object.LogDatabaseOperation(operation, entity, details);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database Operation") && 
                                            v.ToString()!.Contains(operation) && 
                                            v.ToString()!.Contains(entity) &&
                                            v.ToString()!.Contains(details)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDatabaseOperation_WithNullDetails_ShouldLogWithEmptyString()
    {
        // Arrange
        var operation = "DELETE";
        var entity = "Category";

        // Act
        _mockLogger.Object.LogDatabaseOperation(operation, entity, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database Operation") && 
                                            v.ToString()!.Contains(operation) && 
                                            v.ToString()!.Contains(entity)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogSecurityEvent_WithValidParameters_ShouldLogWarningLevel()
    {
        // Arrange
        var eventType = "Failed Login Attempt";
        var userId = Guid.NewGuid().ToString();
        var details = "IP: 192.168.1.1, Browser: Chrome";

        // Act
        _mockLogger.Object.LogSecurityEvent(eventType, userId, details);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Security Event") && 
                                            v.ToString()!.Contains(eventType) && 
                                            v.ToString()!.Contains(userId) &&
                                            v.ToString()!.Contains(details)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogSecurityEvent_WithNullDetails_ShouldLogWithEmptyString()
    {
        // Arrange
        var eventType = "Unauthorized Access";
        var userId = "anonymous";

        // Act
        _mockLogger.Object.LogSecurityEvent(eventType, userId, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Security Event") && 
                                            v.ToString()!.Contains(eventType) && 
                                            v.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogLoginAttempt_WithSuccessfulLogin_ShouldLogInformationLevel()
    {
        // Arrange
        var username = "testuser@example.com";
        var ipAddress = "192.168.1.100";

        // Act
        _mockLogger.Object.LogLoginAttempt(username, true, ipAddress);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successful login") && 
                                            v.ToString()!.Contains(username) && 
                                            v.ToString()!.Contains(ipAddress)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogLoginAttempt_WithFailedLogin_ShouldLogWarningLevel()
    {
        // Arrange
        var username = "hacker@example.com";
        var ipAddress = "10.0.0.1";

        // Act
        _mockLogger.Object.LogLoginAttempt(username, false, ipAddress);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed login") && 
                                            v.ToString()!.Contains(username) && 
                                            v.ToString()!.Contains(ipAddress)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogLoginAttempt_WithNullIpAddress_ShouldLogWithUnknownIP()
    {
        // Arrange
        var username = "testuser@example.com";

        // Act
        _mockLogger.Object.LogLoginAttempt(username, true, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successful login") && 
                                            v.ToString()!.Contains(username) && 
                                            v.ToString()!.Contains("unknown IP")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogApiRequest_WithSuccessfulRequest_ShouldLogInformationLevel()
    {
        // Arrange
        var endpoint = "/api/v1/products";
        var method = "GET";
        var statusCode = 200;
        var durationMs = 150L;

        // Act
        _mockLogger.Object.LogApiRequest(endpoint, method, statusCode, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API") && 
                                            v.ToString()!.Contains(method) && 
                                            v.ToString()!.Contains(endpoint) &&
                                            v.ToString()!.Contains(statusCode.ToString()) &&
                                            v.ToString()!.Contains(durationMs.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogApiRequest_WithErrorStatusCode_ShouldLogWarningLevel()
    {
        // Arrange
        var endpoint = "/api/v1/products/invalid";
        var method = "POST";
        var statusCode = 400;
        var durationMs = 50L;

        // Act
        _mockLogger.Object.LogApiRequest(endpoint, method, statusCode, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API") && 
                                            v.ToString()!.Contains(method) && 
                                            v.ToString()!.Contains(endpoint) &&
                                            v.ToString()!.Contains(statusCode.ToString()) &&
                                            v.ToString()!.Contains(durationMs.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogApiRequest_WithServerErrorStatusCode_ShouldLogWarningLevel()
    {
        // Arrange
        var endpoint = "/api/v1/products";
        var method = "PUT";
        var statusCode = 500;
        var durationMs = 2000L;

        // Act
        _mockLogger.Object.LogApiRequest(endpoint, method, statusCode, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API") && 
                                            v.ToString()!.Contains(method) && 
                                            v.ToString()!.Contains(endpoint) &&
                                            v.ToString()!.Contains(statusCode.ToString()) &&
                                            v.ToString()!.Contains(durationMs.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDataAccess_WithSuccessfulOperation_ShouldLogInformationLevel()
    {
        // Arrange
        var operation = "SELECT";
        var entity = "Products";
        var durationMs = 25L;

        // Act
        _mockLogger.Object.LogDataAccess(operation, entity, true, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Data access") && 
                                            v.ToString()!.Contains(operation) && 
                                            v.ToString()!.Contains(entity) &&
                                            v.ToString()!.Contains("completed successfully") &&
                                            v.ToString()!.Contains(durationMs.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDataAccess_WithFailedOperation_ShouldLogErrorLevel()
    {
        // Arrange
        var operation = "UPDATE";
        var entity = "Users";
        var durationMs = 100L;

        // Act
        _mockLogger.Object.LogDataAccess(operation, entity, false, durationMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Data access") && 
                                            v.ToString()!.Contains(operation) && 
                                            v.ToString()!.Contains(entity) &&
                                            v.ToString()!.Contains("failed") &&
                                            v.ToString()!.Contains(durationMs.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogPermissionCheck_WithAllowedPermission_ShouldLogDebugLevel()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var resource = "Product";
        var action = "Read";

        // Act
        _mockLogger.Object.LogPermissionCheck(userId, resource, action, true);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Permission granted") && 
                                            v.ToString()!.Contains(userId) && 
                                            v.ToString()!.Contains(resource) &&
                                            v.ToString()!.Contains(action)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogPermissionCheck_WithDeniedPermission_ShouldLogWarningLevel()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var resource = "AdminPanel";
        var action = "Access";

        // Act
        _mockLogger.Object.LogPermissionCheck(userId, resource, action, false);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Permission denied") && 
                                            v.ToString()!.Contains(userId) && 
                                            v.ToString()!.Contains(resource) &&
                                            v.ToString()!.Contains(action)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogException_WithValidException_ShouldLogErrorLevel()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");
        var context = "During product creation";

        // Act
        _mockLogger.Object.LogException(exception, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception occurred") && 
                                            v.ToString()!.Contains(context) &&
                                            v.ToString()!.Contains(exception.Message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogException_WithNullContext_ShouldLogWithoutContext()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");

        // Act
        _mockLogger.Object.LogException(exception, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception occurred") && 
                                            v.ToString()!.Contains(exception.Message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogException_WithEmptyContext_ShouldLogWithoutContext()
    {
        // Arrange
        var exception = new NotImplementedException("Feature not implemented");

        // Act
        _mockLogger.Object.LogException(exception, string.Empty);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception occurred") && 
                                            v.ToString()!.Contains(exception.Message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AllLoggingExtensions_WithRealLogger_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var exception = new Exception("Test exception");

        // Act & Assert - All extension methods should work with real logger
        var actions = new Action[]
        {
            () => _realLogger.LogUserAction("Test Action", userId, "Details"),
            () => _realLogger.LogDatabaseOperation("SELECT", "Users", "Query details"),
            () => _realLogger.LogSecurityEvent("Security Test", userId, "Event details"),
            () => _realLogger.LogLoginAttempt("test@example.com", true, "127.0.0.1"),
            () => _realLogger.LogLoginAttempt("test@example.com", false, "127.0.0.1"),
            () => _realLogger.LogApiRequest("/api/test", "GET", 200, 100),
            () => _realLogger.LogApiRequest("/api/test", "POST", 400, 200),
            () => _realLogger.LogDataAccess("INSERT", "Products", true, 50),
            () => _realLogger.LogDataAccess("UPDATE", "Users", false, 150),
            () => _realLogger.LogPermissionCheck(userId, "TestResource", "Read", true),
            () => _realLogger.LogPermissionCheck(userId, "AdminResource", "Write", false),
            () => _realLogger.LogException(exception, "Test context"),
            () => _realLogger.LogException(exception, null)
        };

        foreach (var action in actions)
        {
            action.Should().NotThrow();
        }
    }

    [Fact]
    public void LoggingExtensions_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var userIdWithSpecialChars = "user@domain.com";
        var actionWithSpecialChars = "Create Product with Name: <Test & Validation>";
        var detailsWithSpecialChars = "Details: {\"key\": \"value\", \"special\": \"chars & symbols\"}";

        // Act & Assert
        var logAction = () => _realLogger.LogUserAction(actionWithSpecialChars, userIdWithSpecialChars, detailsWithSpecialChars);
        logAction.Should().NotThrow();
    }

    [Fact]
    public void LoggingExtensions_WithVeryLongStrings_ShouldNotThrow()
    {
        // Arrange
        var longUserId = new string('A', 1000);
        var longAction = new string('B', 2000);
        var longDetails = new string('C', 5000);

        // Act & Assert
        var logAction = () => _realLogger.LogUserAction(longAction, longUserId, longDetails);
        logAction.Should().NotThrow();
    }
}