using MediatR;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Application.Features.Sales.Commands.CreateCart.V1;

public class CreateCartCommandV1 : IRequest<Result<CreateCartResponseV1>>
{
    public Dictionary<string, object>? Metadata { get; init; }
}