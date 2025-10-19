using FluentValidation;

namespace Shopilent.Application.Features.Identity.Commands.UpdateUserStatus.V1;

internal sealed class UpdateUserStatusCommandValidatorV1 : AbstractValidator<UpdateUserStatusCommandV1>
{
    public UpdateUserStatusCommandValidatorV1()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("User ID is required.");
    }
}