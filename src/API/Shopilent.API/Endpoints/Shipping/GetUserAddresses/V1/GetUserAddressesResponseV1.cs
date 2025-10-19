using Shopilent.Domain.Shipping.DTOs;

namespace Shopilent.API.Endpoints.Shipping.GetUserAddresses.V1;

public class GetUserAddressesResponseV1
{
    public IReadOnlyList<AddressDto> Addresses { get; init; } = new List<AddressDto>();
}