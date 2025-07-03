using Shopilent.Domain.Identity;

namespace Shopilent.Application.Features.Identity.Commands.Register.V1;

public sealed class RegisterResponseV1
{
    public User User;
    public string AccessToken;
    public string RefreshToken;
}