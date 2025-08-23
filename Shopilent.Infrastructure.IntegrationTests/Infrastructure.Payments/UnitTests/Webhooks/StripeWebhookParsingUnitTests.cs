using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Infrastructure.Payments.Providers.Stripe;
using Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;
using Shopilent.Infrastructure.Payments.Settings;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Payments.UnitTests.Webhooks;

/// <summary>
/// Unit tests for Stripe webhook payload parsing and validation.
/// These tests verify JSON parsing logic and payload validation without external dependencies.
/// </summary>
public class StripeWebhookParsingUnitTests
{
    private readonly Mock<ILogger<StripePaymentProvider>> _mockLogger;
    private readonly StripeSettings _stripeSettings;

    public StripeWebhookParsingUnitTests()
    {
        _mockLogger = new Mock<ILogger<StripePaymentProvider>>();
        _stripeSettings = new StripeSettings
        {
            PublishableKey = "pk_test_51234567890",
            SecretKey = "sk_test_51234567890",
            WebhookSecret = "whsec_test_secret",
            ApiVersion = "2023-10-16"
        };
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithInvalidJsonPayload_ShouldReturnFailure()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        const string invalidPayload = "{ invalid json";

        // Act
        var result = await provider.ProcessWebhookAsync(invalidPayload);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Payment.ProcessingFailed");
        result.Error.Message.Should().Contain("Invalid webhook payload");
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithEmptyPayload_ShouldReturnFailure()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        const string emptyPayload = "";

        // Act
        var result = await provider.ProcessWebhookAsync(emptyPayload);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Payment.ProcessingFailed");
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithValidJsonButMissingRequiredFields_ShouldReturnFailure()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        const string incompletePayload = """{"some": "data"}""";

        // Act
        var result = await provider.ProcessWebhookAsync(incompletePayload);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Payment.ProcessingFailed");
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNullPayload_ShouldReturnFailure()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        string? nullPayload = null;

        // Act
        var result = await provider.ProcessWebhookAsync(nullPayload!);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Payment.ProcessingFailed");
    }

    [Fact]
    public void CreateValidStripeEventPayload_ShouldReturnWellFormedJson()
    {
        // Arrange
        const string eventType = "payment_intent.succeeded";
        const string objectId = "pi_test_123";

        // Act
        var payload = CreateStripeEventPayload(eventType, objectId);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        
        // Should be valid JSON
        var act = () => JsonConvert.DeserializeObject(payload);
        act.Should().NotThrow();

        // Should contain expected fields
        payload.Should().Contain(eventType);
        payload.Should().Contain(objectId);
        payload.Should().Contain("evt_test_webhook");
    }

    [Theory]
    [InlineData("payment_intent.succeeded", "pi_test_123")]
    [InlineData("charge.succeeded", "ch_test_456")]
    [InlineData("customer.created", "cus_test_789")]
    [InlineData("setup_intent.succeeded", "seti_test_012")]
    [InlineData("payment_method.attached", "pm_test_345")]
    [InlineData("invoice.payment_succeeded", "in_test_678")]
    public void CreateStripeEventPayload_WithVariousEventTypes_ShouldCreateValidJson(string eventType, string objectId)
    {
        // Act
        var payload = CreateStripeEventPayload(eventType, objectId);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        payload.Should().Contain(eventType);
        payload.Should().Contain(objectId);

        // Should be parseable as JSON
        var act = () => JsonConvert.DeserializeObject(payload);
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateStripeEventPayload_WithLongEventType_ShouldHandleCorrectly()
    {
        // Arrange
        const string longEventType = "payment_intent.requires_payment_method";
        const string objectId = "pi_test_long_event";

        // Act
        var payload = CreateStripeEventPayload(longEventType, objectId);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        payload.Should().Contain(longEventType);
        payload.Should().Contain(objectId);

        var act = () => JsonConvert.DeserializeObject(payload);
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateStripeEventPayload_WithSpecialCharactersInObjectId_ShouldEscapeCorrectly()
    {
        // Arrange
        const string eventType = "payment_intent.succeeded";
        const string objectIdWithSpecialChars = "pi_test_\"quotes\"_and_\\backslashes";

        // Act
        var payload = CreateStripeEventPayload(eventType, objectIdWithSpecialChars);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        
        // Should be valid JSON despite special characters
        var act = () => JsonConvert.DeserializeObject(payload);
        act.Should().NotThrow();
        
        // Should properly escape special characters
        payload.Should().Contain("\\\"quotes\\\"");
        payload.Should().Contain("\\\\backslashes");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateStripeEventPayload_WithInvalidEventType_ShouldStillCreateJson(string? invalidEventType)
    {
        // Arrange
        const string objectId = "pi_test_123";

        // Act
        var payload = CreateStripeEventPayload(invalidEventType ?? "", objectId);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        
        // Should still be valid JSON
        var act = () => JsonConvert.DeserializeObject(payload);
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateStripeEventPayload_ShouldIncludeRequiredStripeFields()
    {
        // Arrange
        const string eventType = "payment_intent.succeeded";
        const string objectId = "pi_test_required_fields";

        // Act
        var payload = CreateStripeEventPayload(eventType, objectId);

        // Assert
        payload.Should().NotBeNullOrEmpty();
        
        // Should contain all required Stripe webhook fields
        payload.Should().Contain("\"id\":");
        payload.Should().Contain("\"type\":");
        payload.Should().Contain("\"created\":");
        payload.Should().Contain("\"data\":");
        payload.Should().Contain("\"livemode\":");
        payload.Should().Contain("\"pending_webhooks\":");
        payload.Should().Contain("\"request\":");
        
        // Verify livemode is false for test
        payload.Should().Contain("\"livemode\":false");
    }

    private static string CreateStripeEventPayload(string eventType, string objectId)
    {
        var eventData = new
        {
            id = "evt_test_webhook",
            type = eventType,
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            data = new
            {
                @object = new
                {
                    id = objectId,
                    @object = eventType.Split('.').FirstOrDefault() ?? "unknown",
                    status = "succeeded",
                    amount = 2000,
                    currency = "usd",
                    created = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            },
            livemode = false,
            pending_webhooks = 1,
            request = new
            {
                id = "req_test_123",
                idempotency_key = (string?)null
            },
            api_version = "2023-10-16"
        };

        return JsonConvert.SerializeObject(eventData, Formatting.None);
    }
}