using Shopilent.Application.Abstractions.Messaging;
using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.Application.Features.Shipping.Commands.UpdateAddress.V1;

public sealed record UpdateAddressCommandV1 : ICommand<UpdateAddressResponseV1>
{
    public Guid Id { get; init; }
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }
    public string Phone { get; init; }
    public AddressType AddressType { get; init; }
}