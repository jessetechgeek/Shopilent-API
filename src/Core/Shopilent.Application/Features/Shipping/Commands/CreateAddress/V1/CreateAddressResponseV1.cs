using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.Application.Features.Shipping.Commands.CreateAddress.V1;

public sealed class CreateAddressResponseV1
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public AddressType AddressType { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
}