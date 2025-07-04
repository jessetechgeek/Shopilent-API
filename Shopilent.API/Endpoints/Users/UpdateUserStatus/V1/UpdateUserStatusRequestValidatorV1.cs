using FastEndpoints;

namespace Shopilent.API.Endpoints.Users.UpdateUserStatus.V1;

public class UpdateUserStatusRequestValidatorV1 : Validator<UpdateUserStatusRequestV1>
{
    public UpdateUserStatusRequestValidatorV1()
    {
        // No additional validation needed for this simple request
        // IsActive is a boolean and will be validated by model binding
    }
}