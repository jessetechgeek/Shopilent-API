using FastEndpoints;

namespace Shopilent.API.Endpoints.Sales.CreateCart.V1;

public class CreateCartRequestValidatorV1 : Validator<CreateCartRequestV1>
{
    public CreateCartRequestValidatorV1()
    {
        // Metadata is optional, no additional validation needed for cart creation
        // Individual metadata keys will be validated in the domain layer if needed
    }
}