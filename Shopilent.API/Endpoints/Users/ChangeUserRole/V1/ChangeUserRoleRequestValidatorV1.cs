using FastEndpoints;
using FluentValidation;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.API.Endpoints.Users.ChangeUserRole.V1;

public class ChangeUserRoleRequestValidatorV1 : Validator<ChangeUserRoleRequestV1>
{
    public ChangeUserRoleRequestValidatorV1()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.")
            .Must(role => role == UserRole.Admin || role == UserRole.Manager || role == UserRole.Customer)
            .WithMessage("Role must be Admin, Manager, or Customer.");
    }
}