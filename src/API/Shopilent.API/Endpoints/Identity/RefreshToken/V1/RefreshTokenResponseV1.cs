namespace Shopilent.API.Endpoints.Identity.RefreshToken.V1;

public class RefreshTokenResponseV1
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}