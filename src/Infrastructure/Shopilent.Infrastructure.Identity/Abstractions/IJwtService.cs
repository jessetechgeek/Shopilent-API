using Shopilent.Domain.Identity;

namespace Shopilent.Infrastructure.Identity.Abstractions;

internal interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    (bool isValid, string email, Guid userId) DecodeAccessToken(string token);
    DateTime GetAccessTokenExpiration(string token);
}
