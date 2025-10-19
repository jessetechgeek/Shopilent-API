using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Queries.GetCart.V1;

public sealed record GetCartQueryV1 : IQuery<CartDto?>
{
    public Guid? CartId { get; init; }
}