using FluentValidation;
using Shopilent.Domain.Identity.Enums;

namespace Shopilent.Application.Features.Identity.Commands.ChangeUserRole.V1;

internal sealed class ChangeUserRoleCommandValidatorV1 : AbstractValidator<ChangeUserRoleCommandV1>
{
    public ChangeUserRoleCommandValidatorV1()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}