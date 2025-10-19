using FluentValidation;

namespace Shopilent.Application.Features.Identity.Queries.GetUser.V1;

public class GetUserQueryValidatorV1 : AbstractValidator<GetUserQueryV1>
{
    public GetUserQueryValidatorV1()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("User ID cannot be empty.");
    }
}