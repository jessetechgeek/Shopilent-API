using Shopilent.Domain.Identity;

namespace Shopilent.Application.Abstractions.Identity;

public class AuthTokenResponse
{
    public User User { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}