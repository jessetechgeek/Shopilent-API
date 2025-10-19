using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;

namespace Shopilent.Domain.Identity;

public class RefreshToken : Entity
{
    private RefreshToken()
    {
        // Required by EF Core
    }

    private RefreshToken(User user, string token, DateTime expiresAt, string ipAddress = null, string userAgent = null)
    {
        UserId = user.Id;
        Token = token;
        ExpiresAt = expiresAt;
        IssuedAt = DateTime.UtcNow;
        IsRevoked = false;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    internal static RefreshToken Create(User user, string token, DateTime expiresAt, string ipAddress = null,
        string userAgent = null)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiry date must be in the future", nameof(expiresAt));

        return new RefreshToken(user, token, expiresAt, ipAddress, userAgent);
    }

    internal static Result<RefreshToken> Create(Result<User> userResult, string token, DateTime expiresAt,
        string ipAddress = null, string userAgent = null)
    {
        if (userResult.IsFailure)
            return Result.Failure<RefreshToken>(userResult.Error);

        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<RefreshToken>(RefreshTokenErrors.EmptyToken);

        if (expiresAt <= DateTime.UtcNow)
            return Result.Failure<RefreshToken>(RefreshTokenErrors.InvalidExpiry);

        return Result.Success(new RefreshToken(userResult.Value, token, expiresAt, ipAddress, userAgent));
    }

    internal static Result<RefreshToken> CreateWithStandardExpiry(User user, string token, string ipAddress = null,
        string userAgent = null)
    {
        if (user == null)
            return Result.Failure<RefreshToken>(UserErrors.NotFound(Guid.Empty));

        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<RefreshToken>(RefreshTokenErrors.EmptyToken);

        return Result.Success(new RefreshToken(user, token, DateTime.UtcNow.AddDays(7), ipAddress, userAgent));
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string RevokedReason { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public Result Revoke(string reason)
    {
        if (IsRevoked)
            return Result.Success(); // Already revoked

        IsRevoked = true;
        RevokedReason = reason;
        return Result.Success();
    }
}