using FastEndpoints;
using FluentValidation;

namespace Shopilent.API.Endpoints.Shipping.GetAddressById.V1;

public class GetAddressByIdRequestValidatorV1 : Validator<GetAddressByIdRequestV1>
{
    public GetAddressByIdRequestValidatorV1()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Address ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Address ID cannot be empty.");
    }
}