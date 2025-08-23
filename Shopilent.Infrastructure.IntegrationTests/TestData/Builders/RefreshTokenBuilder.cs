using System.Reflection;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;

namespace Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

public class RefreshTokenBuilder
{
    private User _user;
    private string _token;
    private DateTime _expiresAt = DateTime.UtcNow.AddDays(7);
    private string? _ipAddress = "192.168.1.1";
    private string? _userAgent = "Mozilla/5.0 Test Browser";

    public RefreshTokenBuilder()
    {
        // Generate unique token to ensure uniqueness across tests
        _token = $"refresh_token_{Guid.NewGuid():N}";
        // Create default user - tests should provide their own user via WithUser()
        _user = new UserBuilder().Build();
    }

    public RefreshTokenBuilder WithUser(User user)
    {
        _user = user;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public RefreshTokenBuilder WithIpAddress(string? ipAddress)
    {
        _ipAddress = ipAddress;
        return this;
    }

    public RefreshTokenBuilder WithUserAgent(string? userAgent)
    {
        _userAgent = userAgent;
        return this;
    }

    public RefreshTokenBuilder WithStandardExpiry()
    {
        _expiresAt = DateTime.UtcNow.AddDays(7);
        return this;
    }

    public RefreshTokenBuilder WithShortExpiry()
    {
        _expiresAt = DateTime.UtcNow.AddMinutes(5);
        return this;
    }

    public Result<RefreshToken> BuildResult()
    {
        return _user.AddRefreshToken(_token, _expiresAt, _ipAddress, _userAgent);
    }

    public RefreshToken Build()
    {
        var result = BuildResult();
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to build RefreshToken: {result.Error.Message}");

        return result.Value;
    }

    public RefreshToken BuildExpired()
    {
        // Create with future expiry first, then manually set it to expired using reflection
        var token = WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
        
        // Use reflection to set the expiry date to the past since it's a private setter
        var expiresAtField = typeof(RefreshToken).GetField("<ExpiresAt>k__BackingField", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (expiresAtField == null)
        {
            // Try alternative field name patterns
            expiresAtField = typeof(RefreshToken).GetField("_expiresAt", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        if (expiresAtField != null)
        {
            expiresAtField.SetValue(token, DateTime.UtcNow.AddMinutes(-1));
        }
        else
        {
            throw new InvalidOperationException("Could not find ExpiresAt field for reflection. RefreshToken structure may have changed.");
        }
        
        return token;
    }

    public static RefreshToken CreateExpiredToken(User user)
    {
        return new RefreshTokenBuilder()
            .WithUser(user)
            .WithToken($"expired_token_{Guid.NewGuid():N}")
            .BuildExpired();
    }

    public static RefreshTokenBuilder Create()
    {
        return new RefreshTokenBuilder();
    }

    public static RefreshTokenBuilder ForUser(User user)
    {
        return new RefreshTokenBuilder().WithUser(user);
    }
}