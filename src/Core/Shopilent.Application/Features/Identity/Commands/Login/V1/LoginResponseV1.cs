using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.Login.V1;

public sealed class LoginResponseV1
{
    public User User;
    public string AccessToken;
    public string RefreshToken;
}