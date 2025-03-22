using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Errors;
using Shopilent.Domain.Sales.Errors;

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
        UserId = user?.Id;
        ShippingAddressId = shippingAddress?.Id;
        BillingAddressId = billingAddress?.Id;
        Subtotal = subtotal;
        Tax = tax;
        ShippingCost = shippingCost;
        Total = subtotal.Add(tax).Add(shippingCost);
        ShippingMethod = shippingMethod;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        Metadata = new Dictionary<string, object>();

        _items = new List<OrderItem>();
    }

    // public static Result<Order> Create(
    //     User user,
    //     Address shippingAddress,
    //     Address billingAddress,
    //     Money subtotal,
    //     Money tax,
    //     Money shippingCost,
    //     string shippingMethod = null)
    // {
    //     if (subtotal == null)
    //         return Result.Failure<Order>(OrderErrors.NegativeAmount);
    //
    //     if (tax == null)
    //         return Result.Failure<Order>(OrderErrors.NegativeAmount);
    //
    //     if (shippingCost == null)
    //         return Result.Failure<Order>(OrderErrors.NegativeAmount);
    //
    //     if (shippingAddress == null)
    //         return Result.Failure<Order>(OrderErrors.ShippingAddressRequired);
    //
    //     var order = new Order(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);
    //     order.AddDomainEvent(new OrderCreatedEvent(order.Id));
    //     return Result.Success(order);
    // }
    public static Result<Order> Create(
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money subtotal,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        if (subtotal == null)
            return Result.Failure<Order>(PaymentErrors.NegativeAmount);

        if (tax == null)
            return Result.Failure<Order>(PaymentErrors.NegativeAmount);

        if (shippingCost == null)
            return Result.Failure<Order>(PaymentErrors.NegativeAmount);

        if (shippingAddress == null)
            return Result.Failure<Order>(OrderErrors.ShippingAddressRequired);

        var order = new Order(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return Result.Success(order);
    }

    public static Result<Order> CreateFromCart(
        Cart cart,
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        if (cart == null)
            return Result.Failure<Order>(CartErrors.CartNotFound(Guid.Empty));

        if (cart.Items.Count == 0)
            return Result.Failure<Order>(OrderErrors.EmptyCart);

        // In a real implementation, you would calculate subtotal from cart items
        var subtotal = Money.Zero("USD"); // Placeholder
        var result = Create(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);

        if (result.IsFailure)
            return result;

        // In a real implementation, you would transfer cart items to order items here
        return result;
    }

    public static Result<Order> CreatePaidOrder(
        User user,
        Address shippingAddress,
        Address billingAddress,
        Money subtotal,
        Money tax,
        Money shippingCost,
        string shippingMethod = null)
    {
        var result = Create(user, shippingAddress, billingAddress, subtotal, tax, shippingCost, shippingMethod);
        if (result.IsFailure)
            return result;

        var order = result.Value;
        var markAsPaidResult = order.MarkAsPaid();
        if (markAsPaidResult.IsFailure)
            return Result.Failure<Order>(markAsPaidResult.Error);

        return Result.Success(order);
    }

    public Guid? UserId { get; private set; }
    public Guid? BillingAddressId { get; private set; }
    public Guid? ShippingAddressId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public Money Subtotal { get; private set; }
    public Money Tax { get; private set; }
    public Money ShippingCost { get; private set; }
    public Money Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string ShippingMethod { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Result<OrderItem> AddItem(Product product, int quantity, Money unitPrice, ProductVariant variant = null)
    {
        if (product == null)
            return Result.Failure<OrderItem>(ProductErrors.NotFound(Guid.Empty));

        if (quantity <= 0)
            return Result.Failure<OrderItem>(OrderErrors.InvalidQuantity);

        if (unitPrice == null)
            return Result.Failure<OrderItem>(OrderErrors.NegativeAmount);

        if (Status != OrderStatus.Pending)
            return Result.Failure<OrderItem>(OrderErrors.InvalidOrderStatus("add item"));

        var item = OrderItem.Create(this, product, quantity, unitPrice, variant);
        _items.Add(item);

        RecalculateOrderTotals();

        return Result.Success(item);
    }

    public Result UpdateOrderStatus(OrderStatus status)
    {
        if (Status == status)
            return Result.Success();

        var oldStatus = Status;
        Status = status;

        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, status));
        return Result.Success();
    }

    public Result UpdatePaymentStatus(PaymentStatus status)
    {
        if (PaymentStatus == status)
            return Result.Success();

        var oldStatus = PaymentStatus;
        PaymentStatus = status;

        AddDomainEvent(new OrderPaymentStatusChangedEvent(Id, oldStatus, status));
        return Result.Success();
    }

    public Result MarkAsPaid()
    {
        if (PaymentStatus == PaymentStatus.Succeeded)
            return Result.Success();

        // Store old status values before changing them
        var oldPaymentStatus = PaymentStatus;
        var oldStatus = Status;

        // Update statuses
        PaymentStatus = PaymentStatus.Succeeded;
        if (Status == OrderStatus.Pending)
            Status = OrderStatus.Processing;

        // Add domain events
        if (oldPaymentStatus != PaymentStatus)
            AddDomainEvent(new OrderPaymentStatusChangedEvent(Id, oldPaymentStatus, PaymentStatus));

        if (oldStatus != Status)
            AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));

        // Add the OrderPaidEvent
        AddDomainEvent(new OrderPaidEvent(Id));
        return Result.Success();
    }

    public Result MarkAsShipped(string trackingNumber = null)
    {
        if (Status == OrderStatus.Shipped)
            return Result.Success();

        if (PaymentStatus != PaymentStatus.Succeeded)
            return Result.Failure(OrderErrors.PaymentRequired);

        Status = OrderStatus.Shipped;

        if (trackingNumber != null)
            Metadata["trackingNumber"] = trackingNumber;

        AddDomainEvent(new OrderShippedEvent(Id));
        return Result.Success();
    }

    public Result MarkAsDelivered()
    {
        if (Status == OrderStatus.Delivered)
            return Result.Success();

        if (Status != OrderStatus.Shipped)
            return Result.Failure(OrderErrors.InvalidOrderStatus("mark as delivered"));

        Status = OrderStatus.Delivered;

        AddDomainEvent(new OrderDeliveredEvent(Id));
        return Result.Success();
    }

    public Result Cancel(string reason = null)
    {
        if (Status == OrderStatus.Cancelled)
            return Result.Success();

        if (Status == OrderStatus.Delivered)
            return Result.Failure(OrderErrors.InvalidOrderStatus("cancel"));

        Status = OrderStatus.Cancelled;

        if (reason != null)
            Metadata["cancellationReason"] = reason;

        AddDomainEvent(new OrderCancelledEvent(Id));
        return Result.Success();
    }

    public Result UpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Failure(OrderErrors.InvalidMetadataKey);

        Metadata[key] = value;
        return Result.Success();
    }

    public Result SetPaymentMethod(Guid paymentMethodId)
    {
        if (paymentMethodId == Guid.Empty)
            return Result.Failure(PaymentErrors.PaymentMethodNotFound(paymentMethodId));

        PaymentMethodId = paymentMethodId;
        return Result.Success();
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
}