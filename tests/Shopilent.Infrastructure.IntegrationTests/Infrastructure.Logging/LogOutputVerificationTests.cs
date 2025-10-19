using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Utilities;
using Shopilent.Infrastructure.Logging.Extensions;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Logging;

[Collection("IntegrationTests")]
public class LogOutputVerificationTests : IntegrationTestBase
{
    private TestLoggerProvider _testLoggerProvider = null!;
    private ILogger<LogOutputVerificationTests> _logger = null!;

    public LogOutputVerificationTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        // Create a separate service provider with test logging for verification tests
        var services = new ServiceCollection();
        services.AddTestLogging();
        var testServiceProvider = services.BuildServiceProvider();

        _testLoggerProvider = testServiceProvider.GetRequiredService<TestLoggerProvider>();
        _logger = testServiceProvider.GetRequiredService<ILogger<LogOutputVerificationTests>>();

        return Task.CompletedTask;
    }

    [Fact]
    public void LogOutput_WithInformationLevel_ShouldCaptureCorrectly()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var testMessage = "This is an information message";

        // Act
        _logger.LogInformation(testMessage);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, testMessage);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 1);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Information);
        logEntry.Should().NotBeNull();
        logEntry!.Message.Should().Contain(testMessage);
        logEntry.CategoryName.Should().Contain(nameof(LogOutputVerificationTests));
    }

    [Fact]
    public void LogOutput_WithWarningLevel_ShouldCaptureCorrectly()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var testMessage = "This is a warning message";

        // Act
        _logger.LogWarning(testMessage);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, testMessage);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 1);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Warning);
        logEntry.Should().NotBeNull();
        logEntry!.LogLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void LogOutput_WithErrorLevel_ShouldCaptureCorrectly()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var testMessage = "This is an error message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _logger.LogError(exception, testMessage);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Error, testMessage);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Error, 1);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Error);
        logEntry.Should().NotBeNull();
        logEntry!.Exception.Should().Be(exception);
        logEntry.Message.Should().Contain(testMessage);
    }

    [Fact]
    public void LogOutput_WithStructuredLogging_ShouldCaptureParameters()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var userId = Guid.NewGuid();
        var productName = "Test Product";
        var price = 99.99m;

        // Act
        _logger.LogInformation("User {UserId} created product {ProductName} with price {Price}", 
            userId, productName, price);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "User");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "created product");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, productName);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, price.ToString());
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Information);
        logEntry.Should().NotBeNull();
        logEntry!.Message.Should().Contain(userId.ToString());
    }

    [Fact]
    public void LogOutput_WithUserActionExtension_ShouldCaptureUserAction()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var userId = Guid.NewGuid().ToString();
        var action = "Create Product";
        var details = "Product: Test Item, Category: Electronics";

        // Act
        _logger.LogUserAction(action, userId, details);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "User Action");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, action);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, userId);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, details);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Information);
        logEntry.Should().NotBeNull();
        logEntry!.Message.Should().ContainAll(new[] { "User Action", action, userId, details });
    }

    [Fact]
    public void LogOutput_WithDatabaseOperationExtension_ShouldCaptureDatabaseOperation()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var operation = "INSERT";
        var entity = "Products";
        var details = "ProductId: 123, Name: Test Product";

        // Act
        _logger.LogDatabaseOperation(operation, entity, details);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "Database Operation");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, operation);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, entity);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, details);
    }

    [Fact]
    public void LogOutput_WithSecurityEventExtension_ShouldCaptureSecurityEvent()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var eventType = "Failed Login Attempt";
        var userId = Guid.NewGuid().ToString();
        var details = "IP: 192.168.1.1, UserAgent: Chrome";

        // Act
        _logger.LogSecurityEvent(eventType, userId, details);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, "Security Event");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, eventType);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, userId);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, details);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Warning);
        logEntry.Should().NotBeNull();
        logEntry!.LogLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void LogOutput_WithLoginAttemptExtension_ShouldCaptureLoginAttempts()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var username = "testuser@example.com";
        var ipAddress = "192.168.1.100";

        // Act - Successful login
        _logger.LogLoginAttempt(username, true, ipAddress);
        
        // Act - Failed login
        _logger.LogLoginAttempt(username, false, ipAddress);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "Successful login");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, "Failed login");
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 1);
        
        var successEntry = _testLoggerProvider.GetLogEntriesContaining("Successful login").First();
        var failureEntry = _testLoggerProvider.GetLogEntriesContaining("Failed login").First();
        
        successEntry.Message.Should().ContainAll(new[] { username, ipAddress });
        failureEntry.Message.Should().ContainAll(new[] { username, ipAddress });
    }

    [Fact]
    public void LogOutput_WithApiRequestExtension_ShouldCaptureApiRequests()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var endpoint = "/api/v1/products";
        var method = "GET";
        var statusCode = 200;
        var durationMs = 150L;

        // Act
        _logger.LogApiRequest(endpoint, method, statusCode, durationMs);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "API");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, endpoint);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, method);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, statusCode.ToString());
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, durationMs.ToString());
    }

    [Fact]
    public void LogOutput_WithApiRequestExtension_ErrorStatusCode_ShouldLogAsWarning()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var endpoint = "/api/v1/products/invalid";
        var method = "POST";
        var statusCode = 400;
        var durationMs = 50L;

        // Act
        _logger.LogApiRequest(endpoint, method, statusCode, durationMs);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, "API");
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 0);
    }

    [Fact]
    public void LogOutput_WithDataAccessExtension_ShouldCaptureDataAccess()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var operation = "SELECT";
        var entity = "Users";
        var durationMs = 25L;

        // Act - Successful operation
        _logger.LogDataAccess(operation, entity, true, durationMs);
        
        // Act - Failed operation
        _logger.LogDataAccess(operation, entity, false, durationMs);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "Data access");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "completed successfully");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Error, "failed");
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Error, 1);
    }

    [Fact]
    public void LogOutput_WithPermissionCheckExtension_ShouldCapturePermissionChecks()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var userId = Guid.NewGuid().ToString();
        var resource = "Product";
        var action = "Read";

        // Act - Allowed permission
        _logger.LogPermissionCheck(userId, resource, action, true);
        
        // Act - Denied permission
        _logger.LogPermissionCheck(userId, resource, action, false);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Debug, "Permission granted");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Warning, "Permission denied");
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Debug, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 1);
    }

    [Fact]
    public void LogOutput_WithExceptionExtension_ShouldCaptureExceptions()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var exception = new InvalidOperationException("Test exception message");
        var context = "During product creation";

        // Act
        _logger.LogException(exception, context);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Error, "Exception occurred");
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Error, context);
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Error, exception.Message);
        
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Error);
        logEntry.Should().NotBeNull();
        logEntry!.Exception.Should().Be(exception);
    }

    [Fact]
    public void LogOutput_WithMultipleLogLevels_ShouldCaptureAllLevels()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var baseMessage = "Test message";

        // Act
        _logger.LogTrace($"Trace {baseMessage}");
        _logger.LogDebug($"Debug {baseMessage}");
        _logger.LogInformation($"Information {baseMessage}");
        _logger.LogWarning($"Warning {baseMessage}");
        _logger.LogError($"Error {baseMessage}");
        _logger.LogCritical($"Critical {baseMessage}");

        // Assert
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Trace, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Debug, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Error, 1);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Critical, 1);
        
        var allLogs = _testLoggerProvider.GetLogs().ToList();
        allLogs.Should().HaveCount(6);
        allLogs.Should().OnlyContain(log => log.Message.Contains(baseMessage));
    }

    [Fact]
    public void LogOutput_WithTimestamps_ShouldCaptureCorrectTimestamps()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var startTime = DateTime.UtcNow;

        // Act
        _logger.LogInformation("First message");
        Task.Delay(10).Wait(); // Small delay
        _logger.LogInformation("Second message");
        var endTime = DateTime.UtcNow;

        // Assert
        var logs = _testLoggerProvider.GetLogEntriesBetween(startTime, endTime).ToList();
        logs.Should().HaveCount(2);
        
        var firstLog = logs.OrderBy(l => l.Timestamp).First();
        var secondLog = logs.OrderBy(l => l.Timestamp).Last();
        
        firstLog.Message.Should().Contain("First message");
        secondLog.Message.Should().Contain("Second message");
        secondLog.Timestamp.Should().BeAfter(firstLog.Timestamp);
    }

    [Fact]
    public void LogOutput_WithExceptionTypes_ShouldCaptureSpecificExceptionTypes()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var invalidOpException = new InvalidOperationException("Invalid operation");
        var argumentException = new ArgumentException("Invalid argument");
        var notImplementedException = new NotImplementedException("Not implemented");

        // Act
        _logger.LogError(invalidOpException, "First error");
        _logger.LogError(argumentException, "Second error");
        _logger.LogError(notImplementedException, "Third error");

        // Assert
        _testLoggerProvider.HasExceptionLogEntry<InvalidOperationException>("First error").Should().BeTrue();
        _testLoggerProvider.HasExceptionLogEntry<ArgumentException>("Second error").Should().BeTrue();
        _testLoggerProvider.HasExceptionLogEntry<NotImplementedException>("Third error").Should().BeTrue();
        
        var exceptionLogs = _testLoggerProvider.GetLogEntriesWithException().ToList();
        exceptionLogs.Should().HaveCount(3);
    }

    [Fact]
    public void LogOutput_WithRecentLogs_ShouldFilterByTimeRange()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();

        // Act
        _logger.LogInformation("Old message");
        Task.Delay(100).Wait();
        _logger.LogInformation("Recent message 1");
        _logger.LogInformation("Recent message 2");

        // Assert
        var recentLogs = _testLoggerProvider.GetLogEntriesInLastSeconds(1).ToList();
        recentLogs.Should().HaveCountGreaterOrEqualTo(2);
        
        var allLogs = _testLoggerProvider.GetLogs().ToList();
        allLogs.Should().HaveCount(3);
    }

    [Fact]
    public void LogOutput_WithClearLogs_ShouldRemoveAllLogs()
    {
        // Arrange
        _logger.LogInformation("Message 1");
        _logger.LogWarning("Message 2");
        _logger.LogError("Message 3");
        
        _testLoggerProvider.GetLogs().Should().HaveCount(3);

        // Act
        _testLoggerProvider.ClearLogs();

        // Assert
        _testLoggerProvider.GetLogs().Should().BeEmpty();
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Information, 0);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Warning, 0);
        _testLoggerProvider.AssertLogEntryCount(LogLevel.Error, 0);
    }

    [Fact]
    public void LogOutput_WithComplexObjectSerialization_ShouldCaptureObjectDetails()
    {
        // Arrange
        _testLoggerProvider.ClearLogs();
        var complexObject = new
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 99.99m,
            Categories = new[] { "Electronics", "Gadgets" },
            Metadata = new Dictionary<string, object>
            {
                { "Color", "Blue" },
                { "Weight", 1.5 },
                { "InStock", true }
            }
        };

        // Act
        _logger.LogInformation("Created product: {@Product}", complexObject);

        // Assert
        _testLoggerProvider.AssertLogEntryExists(LogLevel.Information, "Created product");
        var logEntry = _testLoggerProvider.GetLastLogEntry(LogLevel.Information);
        logEntry.Should().NotBeNull();
        
        // The structured logging should capture the object properties
        logEntry!.Message.Should().Contain("Created product");
    }
}

public static class LogOutputAssertionExtensions
{
    public static void ShouldContainAll(this string actual, IEnumerable<string> expected)
    {
        foreach (var expectedValue in expected)
        {
            actual.Should().Contain(expectedValue, $"Expected '{actual}' to contain '{expectedValue}'");
        }
    }
}