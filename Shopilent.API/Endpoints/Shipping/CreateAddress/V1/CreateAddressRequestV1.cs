using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.Endpoints.Shipping.CreateAddress.V1;

public class CreateAddressRequestV1
{
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public AddressType AddressType { get; init; } = AddressType.Shipping;
    public bool IsDefault { get; init; } = false;
}