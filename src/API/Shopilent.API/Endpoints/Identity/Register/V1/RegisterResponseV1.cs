namespace Shopilent.API.Endpoints.Identity.Register.V1;

public class RegisterResponseV1
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public bool EmailVerified { get; init; }
    public string Message { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}