namespace Shopilent.API.Endpoints.Identity.Login.V1;

public class LoginResponseV1
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public bool EmailVerified { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}