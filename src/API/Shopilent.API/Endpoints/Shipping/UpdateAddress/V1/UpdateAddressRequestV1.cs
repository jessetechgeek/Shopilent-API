using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.Endpoints.Shipping.UpdateAddress.V1;

public class UpdateAddressRequestV1
{
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }
    public string Phone { get; init; }
    public AddressType AddressType { get; init; }
}