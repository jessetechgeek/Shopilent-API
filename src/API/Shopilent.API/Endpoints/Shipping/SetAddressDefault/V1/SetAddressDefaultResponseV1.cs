using Shopilent.Domain.Shipping.Enums;

namespace Shopilent.API.Endpoints.Shipping.SetAddressDefault.V1;

public sealed class SetAddressDefaultResponseV1
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string AddressLine1 { get; init; }
    public string AddressLine2 { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }
    public string Phone { get; init; }
    public bool IsDefault { get; init; }
    public AddressType AddressType { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}