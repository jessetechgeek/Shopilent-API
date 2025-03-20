using Shopilent.Domain.Catalog;
using Shopilent.Domain.Common;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Sales;

public class Order : AggregateRoot
{
    private Order()
    {
        // Required by EF Core
    }

    private Order(
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money subtotal,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        if (subtotal == null)
            throw new ArgumentNullException(nameof(subtotal));

        if (tax == null)
            throw new ArgumentNullException(nameof(tax));

        if (shippingCost == null)
            throw new ArgumentNullException(nameof(shippingCost));

        UserId = user?.Id;
        ShippingAddressId = shippingAddress?.Id;
        BillingAddressId = billingAddress?.Id;
        Subtotal = subtotal;
        Tax = tax;
        ShippingCost = shippingCost;
        Total = subtotal.Add(tax).Add(shippingCost);
        ShippingMethod = shippingMethod;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending; // Changed to use the Payments namespace enum
        Metadata = new Dictionary<string, object>();

        _items = new List<OrderItem>();
    }

    public static Order Create(
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money subtotal,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        var order = new Order(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }

    public static Order CreateFromCart(
        Cart cart,
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        // In real implementation, you would calculate subtotal from cart items
        var subtotal = Money.Zero("USD"); // Placeholder
        var order = Create(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);

        // In real implementation, you would transfer cart items to order items here

        return order;
    }

    public static Order CreatePaidOrder(
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money subtotal,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        var order = Create(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);
        order.MarkAsPaid();
        return order;
    }

    public Guid? UserId { get; private set; }
    public Guid? BillingAddressId { get; private set; }
    public Guid? ShippingAddressId { get; private set; }
    public Guid? PaymentMethodId { get; private set; } // Added to match DB schema
    public Money Subtotal { get; private set; }
    public Money Tax { get; private set; }
    public Money ShippingCost { get; private set; }
    public Money Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; } // Changed to use the Payments namespace enum
    public string ShippingMethod { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public OrderItem AddItem(Product product, int quantity, Money unitPrice, ProductVariant variant = null)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (unitPrice == null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify a non-pending order");

        var item = OrderItem.Create(this, product, quantity, unitPrice, variant);
        _items.Add(item);

        RecalculateOrderTotals();

        return item;
    }

    public void UpdateOrderStatus(OrderStatus status)
    {
        if (Status == status)
            return;

        var oldStatus = Status;
        Status = status;

        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, status));
    }

    public void UpdatePaymentStatus(PaymentStatus status)
    {
        if (PaymentStatus == status)
            return;

        var oldStatus = PaymentStatus;
        PaymentStatus = status;

        AddDomainEvent(new OrderPaymentStatusChangedEvent(Id, oldStatus, status));
    }

    public void MarkAsPaid()
    {
        if (PaymentStatus == PaymentStatus.Succeeded) // Changed from Paid to Succeeded
            return;

        // Store old status values before changing them
        var oldPaymentStatus = PaymentStatus;
        var oldStatus = Status;

        // Update statuses
        PaymentStatus = PaymentStatus.Succeeded; // Changed from Paid to Succeeded
        if (Status == OrderStatus.Pending)
            Status = OrderStatus.Processing;

        // Add domain events
        if (oldPaymentStatus != PaymentStatus)
            AddDomainEvent(new OrderPaymentStatusChangedEvent(Id, oldPaymentStatus, PaymentStatus));

        if (oldStatus != Status)
            AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));

        // Add the OrderPaidEvent
        AddDomainEvent(new OrderPaidEvent(Id));
    }

    public void MarkAsShipped(string trackingNumber = null)
    {
        if (Status == OrderStatus.Shipped)
            return;

        if (PaymentStatus != PaymentStatus.Succeeded) // Changed from Paid to Succeeded
            throw new InvalidOperationException("Cannot ship an unpaid order");

        Status = OrderStatus.Shipped;

        if (trackingNumber != null)
            Metadata["trackingNumber"] = trackingNumber;

        AddDomainEvent(new OrderShippedEvent(Id));
    }

    public void MarkAsDelivered()
    {
        if (Status == OrderStatus.Delivered)
            return;

        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be shipped before it can be delivered");

        Status = OrderStatus.Delivered;

        AddDomainEvent(new OrderDeliveredEvent(Id));
    }

    public void Cancel(string reason = null)
    {
        if (Status == OrderStatus.Cancelled)
            return;

        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order");

        Status = OrderStatus.Cancelled;

        if (reason != null)
            Metadata["cancellationReason"] = reason;

        AddDomainEvent(new OrderCancelledEvent(Id));
    }

    public void UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata[key] = value;
    }

    private void RecalculateOrderTotals()
    {
        // Recalculate based on items
        var newSubtotal = Money.Zero(Subtotal.Currency);

        foreach (var item in _items)
        {
            newSubtotal = newSubtotal.Add(item.TotalPrice);
        }

        Subtotal = newSubtotal;
        Total = Subtotal.Add(Tax).Add(ShippingCost);
    }

    public void SetPaymentMethod(Guid paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }
}