using Shopilent.Domain.Sales.DTOs;

namespace Shopilent.Application.Features.Sales.Commands.CreateOrderFromCart.V1;

public class CreateOrderFromCartResponseV1
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public Guid? BillingAddressId { get; init; }
    public Guid? ShippingAddressId { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; }
    public string PaymentStatus { get; init; }
    public string? ShippingMethod { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
    public DateTime CreatedAt { get; init; }
}