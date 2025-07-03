namespace Shopilent.API.Endpoints.Identity.Register.V1;

public class RegisterRequestV1
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Phone { get; init; }
}