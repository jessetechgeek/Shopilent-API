namespace Shopilent.API.Endpoints.Identity.ResetPassword.V1;

public class ResetPasswordRequestV1
{
    public string Token { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}