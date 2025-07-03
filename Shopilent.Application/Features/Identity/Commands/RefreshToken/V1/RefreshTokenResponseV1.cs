using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.RefreshToken.V1;

public sealed class RefreshTokenResponseV1
{
    public User User;
    public string AccessToken;
    public string RefreshToken;
}