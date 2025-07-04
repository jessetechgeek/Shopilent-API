using FluentValidation;

namespace Shopilent.Application.Features.Sales.Commands.CreateCart.V1;

public class CreateCartCommandValidatorV1 : AbstractValidator<CreateCartCommandV1>
{
    public CreateCartCommandValidatorV1()
    {
        // Metadata validation can be added here if needed
        // For now, we'll let the domain handle metadata validation
    }
}