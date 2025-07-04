using Shopilent.Application.Abstractions.Messaging;

namespace Shopilent.Application.Features.Sales.Commands.CreateOrderFromCart.V1;

public sealed record CreateOrderFromCartCommandV1 : ICommand<CreateOrderFromCartResponseV1>
{
    public Guid? CartId { get; init; }
    public Guid ShippingAddressId { get; init; }
    public Guid? BillingAddressId { get; init; }
    public string? ShippingMethod { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}