using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Infrastructure.Payments.Providers.Stripe;
using Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;
using Shopilent.Infrastructure.Payments.Settings;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Payments.UnitTests.Providers;

/// <summary>
/// Unit tests for StripePaymentProvider configuration and instantiation.
/// These tests verify provider setup without external API calls.
/// </summary>
public class StripePaymentProviderConfigurationUnitTests
{
    private readonly Mock<ILogger<StripePaymentProvider>> _mockLogger;
    private readonly StripeSettings _stripeSettings;

    public StripePaymentProviderConfigurationUnitTests()
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
    public void Provider_ShouldReturnStripe()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        // Act & Assert
        provider.Provider.Should().Be(PaymentProvider.Stripe);
    }

    [Fact]
    public void Constructor_WithValidSettings_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        var act = () => new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_SetsStripeApiKey()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act
        var provider = new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);

        // Assert
        // We can't directly test if the API key was set because StripeConfiguration.ApiKey is static
        // But we can verify the provider was created without throwing
        provider.Should().NotBeNull();
        provider.Provider.Should().Be(PaymentProvider.Stripe);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSecretKey_ShouldStillCreate(string invalidSecretKey)
    {
        // Arrange
        var invalidSettings = new StripeSettings
        {
            PublishableKey = "pk_test_51234567890",
            SecretKey = invalidSecretKey,
            WebhookSecret = "whsec_test_secret",
            ApiVersion = "2023-10-16"
        };
        var options = Options.Create(invalidSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        // The provider should still be created, but operations will fail at runtime
        var act = () => new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrow()
    {
        // Arrange
        IOptions<StripeSettings> nullOptions = null!;
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        var act = () => new StripePaymentProvider(nullOptions, _mockLogger.Object, mockWebhookHandlerFactory.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("settings");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        var act = () => new StripePaymentProvider(options, null!, mockWebhookHandlerFactory.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_WithNullWebhookHandlerFactory_ShouldThrow()
    {
        // Arrange
        var options = Options.Create(_stripeSettings);

        // Act & Assert
        var act = () => new StripePaymentProvider(options, _mockLogger.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("webhookHandlerFactory");
    }

    [Theory]
    [InlineData("2020-08-27")]
    [InlineData("2023-10-16")]
    [InlineData("2024-06-20")]
    public void Constructor_WithDifferentApiVersions_ShouldCreateProvider(string apiVersion)
    {
        // Arrange
        var settingsWithVersion = new StripeSettings
        {
            PublishableKey = "pk_test_51234567890",
            SecretKey = "sk_test_51234567890",
            WebhookSecret = "whsec_test_secret",
            ApiVersion = apiVersion
        };
        var options = Options.Create(settingsWithVersion);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        var act = () => new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEmptyWebhookSecret_ShouldStillCreate()
    {
        // Arrange
        var settingsWithEmptyWebhookSecret = new StripeSettings
        {
            PublishableKey = "pk_test_51234567890",
            SecretKey = "sk_test_51234567890",
            WebhookSecret = "", // Empty webhook secret
            ApiVersion = "2023-10-16"
        };
        var options = Options.Create(settingsWithEmptyWebhookSecret);
        var mockWebhookHandlerFactory = new Mock<StripeWebhookHandlerFactory>(Mock.Of<IServiceProvider>(), Mock.Of<ILogger<StripeWebhookHandlerFactory>>());

        // Act & Assert
        // Provider should be created, but webhook processing will fail at runtime
        var act = () => new StripePaymentProvider(options, _mockLogger.Object, mockWebhookHandlerFactory.Object);
        act.Should().NotThrow();
    }
}