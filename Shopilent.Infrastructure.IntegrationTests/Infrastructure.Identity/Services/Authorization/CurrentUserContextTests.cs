using Shopilent.Application.Abstractions.Identity;
using Shopilent.Infrastructure.IntegrationTests.Common;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Identity.Services.Authorization;

[Collection("IntegrationTests")]
public class CurrentUserContextTests : IntegrationTestBase
{
    private ICurrentUserContext _currentUserContext = null!;

    public CurrentUserContextTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _currentUserContext = GetService<ICurrentUserContext>();
        return Task.CompletedTask;
    }

    [Fact]
    public void UserId_MockSetup_ShouldReturnTestUserId()
    {
        // Arrange
        // Mock ICurrentUserContext provides a test user ID for audit interceptor

        // Act
        var userId = _currentUserContext.UserId;

        // Assert
        userId.Should().NotBeNull();
        userId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Email_MockSetup_ShouldReturnTestEmail()
    {
        // Arrange
        // Mock ICurrentUserContext provides a test email

        // Act
        var email = _currentUserContext.Email;

        // Assert
        email.Should().Be("test@integrationtest.com");
    }

    [Fact]
    public void IpAddress_MockSetup_ShouldReturnTestIpAddress()
    {
        // Arrange
        // Mock ICurrentUserContext provides a test IP address

        // Act
        var ipAddress = _currentUserContext.IpAddress;

        // Assert
        ipAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public void UserAgent_MockSetup_ShouldReturnTestUserAgent()
    {
        // Arrange
        // Mock ICurrentUserContext provides a test user agent

        // Act
        var userAgent = _currentUserContext.UserAgent;

        // Assert
        userAgent.Should().Be("Integration Test Browser/1.0");
    }

    [Fact]
    public void IsAuthenticated_MockSetup_ShouldReturnTrue()
    {
        // Arrange
        // Mock ICurrentUserContext simulates authenticated user

        // Act
        var isAuthenticated = _currentUserContext.IsAuthenticated;

        // Assert
        isAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_TestRole_ShouldReturnTrue()
    {
        // Arrange
        // Mock ICurrentUserContext returns true only for "TestRole"

        // Act & Assert
        _currentUserContext.IsInRole("TestRole").Should().BeTrue();
        _currentUserContext.IsInRole("Admin").Should().BeFalse();
        _currentUserContext.IsInRole("Customer").Should().BeFalse();
        _currentUserContext.IsInRole("AnyOtherRole").Should().BeFalse();
    }
}
