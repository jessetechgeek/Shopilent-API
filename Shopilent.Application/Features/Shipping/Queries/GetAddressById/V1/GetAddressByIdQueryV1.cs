using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.Application.Features.Shipping.Queries.GetAddressById.V1;

public sealed record GetAddressByIdQueryV1 : IQuery<AddressDto>
{
    public Guid AddressId { get; init; }
}