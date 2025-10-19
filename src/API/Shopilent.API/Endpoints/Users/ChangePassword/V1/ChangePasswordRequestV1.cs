namespace Shopilent.API.Endpoints.Users.ChangePassword.V1;

public class ChangePasswordRequestV1
{
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}