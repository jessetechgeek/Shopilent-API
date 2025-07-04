using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.Application.Features.Shipping.Commands.SetAddressDefault.V1;

public sealed record SetAddressDefaultCommandV1 : ICommand<AddressDto>
{
    public Guid AddressId { get; init; }
}