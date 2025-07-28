using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.Enums;
using Shopilent.Domain.Sales.Events;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Catalog;
using Shopilent.Domain.Catalog.ValueObjects;
using Shopilent.Domain.Shipping.ValueObjects;
using Shopilent.Domain.Payments.Enums;

namespace Shopilent.Domain.Tests.Sales;

public class OrderTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        Assert.True(emailResult.IsSuccess);

        var fullNameResult = FullName.Create("Test", "User");
        Assert.True(fullNameResult.IsSuccess);

        var userResult = User.Create(
            emailResult.Value,
            "hashed_password",
            fullNameResult.Value);

        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    private Address CreateTestAddress(User user)
    {
        var postalAddressResult = PostalAddress.Create(
            "123 Main St",
            "Anytown",
            "State",
            "Country",
            "12345");
        Assert.True(postalAddressResult.IsSuccess);

        var addressResult = Address.CreateShipping(
            user,
            postalAddressResult.Value);

        Assert.True(addressResult.IsSuccess);
        return addressResult.Value;
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrder()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        var shippingMethod = "Standard";

        // Act
        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost,
            shippingMethod);

        // Assert
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        Assert.Equal(user.Id, order.UserId);
        Assert.Equal(shippingAddress.Id, order.ShippingAddressId);
        Assert.Equal(billingAddress.Id, order.BillingAddressId);
        Assert.Equal(subtotal, order.Subtotal);
        Assert.Equal(tax, order.Tax);
        Assert.Equal(shippingCost, order.ShippingCost);
        Assert.Equal(shippingMethod, order.ShippingMethod);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);

        // Total should be sum of subtotal, tax, and shipping
        var expectedTotalResult = Money.Create(115, "USD");
        Assert.True(expectedTotalResult.IsSuccess);
        var expectedTotal = expectedTotalResult.Value;
        Assert.Equal(expectedTotal.Amount, order.Total.Amount);

        Assert.Empty(order.Items);
        Assert.Contains(order.DomainEvents, e => e is OrderCreatedEvent);
    }

    [Fact]
    public void Create_WithNullSubtotal_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);
        Money subtotal = null;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        // Act
        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        // Assert
        Assert.True(orderResult.IsFailure);
        Assert.Equal("Payment.NegativeAmount", orderResult.Error.Code);
    }

    [Fact]
    public void Create_WithNullTax_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        Money tax = null;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        // Act
        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        // Assert
        Assert.True(orderResult.IsFailure);
        Assert.Equal("Payment.NegativeAmount", orderResult.Error.Code);
    }

    [Fact]
    public void Create_WithNullShippingCost_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        Money shippingCost = null;

        // Act
        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        // Assert
        Assert.True(orderResult.IsFailure);
        Assert.Equal("Payment.NegativeAmount", orderResult.Error.Code);
    }

    [Fact]
    public void CreatePaidOrder_ShouldCreateOrderWithPaidStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        // Act
        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        // Assert
        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        Assert.Equal(PaymentStatus.Succeeded, order.PaymentStatus);
        Assert.Equal(OrderStatus.Processing, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderCreatedEvent);
        Assert.Contains(order.DomainEvents, e => e is OrderPaidEvent);
    }

    // Removing CreateFromCart test as this method doesn't exist anymore

    [Fact]
    public void AddItem_ShouldAddItemToOrder()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(0, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);

        var productPriceResult = Money.Create(50, "USD");
        Assert.True(productPriceResult.IsSuccess);

        var productResult = Product.Create(
            "Test Product",
            slugResult.Value,
            productPriceResult.Value);

        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var quantity = 2;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice);

        // Assert
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;
        Assert.Single(order.Items);
        Assert.Equal(product.Id, orderItem.ProductId);
        Assert.Equal(quantity, orderItem.Quantity);
        Assert.Equal(unitPrice, orderItem.UnitPrice);

        // Total price for the item should be unit price * quantity
        var expectedItemTotalResult = Money.Create(100, "USD");
        Assert.True(expectedItemTotalResult.IsSuccess);
        var expectedItemTotal = expectedItemTotalResult.Value;
        Assert.Equal(expectedItemTotal.Amount, orderItem.TotalPrice.Amount);

        // Order subtotal should be updated
        Assert.Equal(expectedItemTotal.Amount, order.Subtotal.Amount);

        // Order total should be updated (subtotal + tax + shipping)
        var expectedOrderTotalResult = Money.Create(115, "USD");
        Assert.True(expectedOrderTotalResult.IsSuccess);
        var expectedOrderTotal = expectedOrderTotalResult.Value;
        Assert.Equal(expectedOrderTotal.Amount, order.Total.Amount);
    }

    [Fact]
    public void AddItem_WithNullProduct_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(0, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(0, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(0, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Product product = null;
        var quantity = 1;

        var unitPriceResult = Money.Create(50, "USD");
        Assert.True(unitPriceResult.IsSuccess);
        var unitPrice = unitPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice);

        // Assert
        Assert.True(orderItemResult.IsFailure);
        Assert.Equal("Product.NotFound", orderItemResult.Error.Code);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var moneyResult = Money.Create(0, "USD");
        Assert.True(moneyResult.IsSuccess);
        var money = moneyResult.Value;

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            money,
            money,
            money);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);

        var priceResult = Money.Create(50, "USD");
        Assert.True(priceResult.IsSuccess);

        var productResult = Product.Create(
            "Test Product",
            slugResult.Value,
            priceResult.Value);

        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var quantity = 0;

        // Act
        var orderItemResult = order.AddItem(product, quantity, priceResult.Value);

        // Assert
        Assert.True(orderItemResult.IsFailure);
        Assert.Equal("Order.InvalidQuantity", orderItemResult.Error.Code);
    }

    [Fact]
    public void AddItem_WithNonPendingOrder_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        // Change order status from Pending
        var paidResult = order.MarkAsPaid();
        Assert.True(paidResult.IsSuccess);

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);

        Assert.Equal(OrderStatus.Shipped, order.Status);

        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);

        var priceResult = Money.Create(50, "USD");
        Assert.True(priceResult.IsSuccess);

        var productResult = Product.Create(
            "Test Product",
            slugResult.Value,
            priceResult.Value);

        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var quantity = 1;

        // Act
        var orderItemResult = order.AddItem(product, quantity, priceResult.Value);

        // Assert
        Assert.True(orderItemResult.IsFailure);
        Assert.Equal("Order.InvalidStatus", orderItemResult.Error.Code);
    }

    [Fact]
    public void AddItem_WithProductVariant_ShouldAddItemWithVariant()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(0, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var slugResult = Slug.Create("test-product");
        Assert.True(slugResult.IsSuccess);

        var basePriceResult = Money.Create(50, "USD");
        Assert.True(basePriceResult.IsSuccess);

        var productResult = Product.Create(
            "Test Product",
            slugResult.Value,
            basePriceResult.Value);

        Assert.True(productResult.IsSuccess);
        var product = productResult.Value;

        var variantPriceResult = Money.Create(60, "USD");
        Assert.True(variantPriceResult.IsSuccess);

        var variantResult = ProductVariant.Create(
            product.Id,
            "VAR-123",
            variantPriceResult.Value,
            10);

        Assert.True(variantResult.IsSuccess);
        var variant = variantResult.Value;

        var quantity = 1;
        var unitPrice = variantPriceResult.Value;

        // Act
        var orderItemResult = order.AddItem(product, quantity, unitPrice, variant);

        // Assert
        Assert.True(orderItemResult.IsSuccess);
        var orderItem = orderItemResult.Value;
        Assert.Single(order.Items);
        Assert.Equal(product.Id, orderItem.ProductId);
        Assert.Equal(variant.Id, orderItem.VariantId);
        Assert.Equal(quantity, orderItem.Quantity);
        Assert.Equal(unitPrice, orderItem.UnitPrice);

        // Product data should include variant info
        Assert.True(orderItem.ProductData.ContainsKey("variant_sku"));
        Assert.Equal(variant.Sku, orderItem.ProductData["variant_sku"]);
    }

    [Fact]
    public void UpdateOrderStatus_ShouldChangeStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(OrderStatus.Pending, order.Status);

        // Act
        var updateResult = order.UpdateOrderStatus(OrderStatus.Processing);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(OrderStatus.Processing, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderStatusChangedEvent);
    }

    [Fact]
    public void UpdatePaymentStatus_ShouldChangePaymentStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);

        // Act
        var updateResult = order.UpdatePaymentStatus(PaymentStatus.Succeeded);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, order.PaymentStatus);
        Assert.Contains(order.DomainEvents, e => e is OrderPaymentStatusChangedEvent);
    }

    [Fact]
    public void MarkAsPaid_ShouldUpdateStatusesCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
        Assert.Equal(OrderStatus.Pending, order.Status);

        // Act
        var paidResult = order.MarkAsPaid();

        // Assert
        Assert.True(paidResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, order.PaymentStatus);
        Assert.Equal(OrderStatus.Processing, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderPaidEvent);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldNotChangeStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(PaymentStatus.Succeeded, order.PaymentStatus);

        // Clear domain events for fresh test
        order.ClearDomainEvents();

        // Act
        var paidResult = order.MarkAsPaid();

        // Assert
        Assert.True(paidResult.IsSuccess);
        Assert.Equal(PaymentStatus.Succeeded, order.PaymentStatus);
        Assert.Empty(order.DomainEvents); // No events should be raised
    }

    [Fact]
    public void MarkAsShipped_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(OrderStatus.Processing, order.Status);
        var trackingNumber = "TRACK123456";

        // Act
        var shippedResult = order.MarkAsShipped(trackingNumber);

        // Assert
        Assert.True(shippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(trackingNumber, order.Metadata["trackingNumber"]);
        Assert.Contains(order.DomainEvents, e => e is OrderShippedEvent);
    }

    [Fact]
    public void MarkAsShipped_WithUnpaidOrder_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);

        // Act
        var shippedResult = order.MarkAsShipped();

        // Assert
        Assert.True(shippedResult.IsFailure);
        Assert.Equal("Order.PaymentRequired", shippedResult.Error.Code);
    }

    [Fact]
    public void MarkAsShipped_WhenAlreadyShipped_ShouldNotChangeStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped("TRACK123");
        Assert.True(shippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);

        // Clear domain events for fresh test
        order.ClearDomainEvents();

        // Act
        var secondShippedResult = order.MarkAsShipped("TRACK456");

        // Assert
        Assert.True(secondShippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal("TRACK123", order.Metadata["trackingNumber"]); // Shouldn't change
        Assert.Empty(order.DomainEvents); // No events should be raised
    }

    [Fact]
    public void MarkAsDelivered_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);

        // Act
        var deliveredResult = order.MarkAsDelivered();

        // Assert
        Assert.True(deliveredResult.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderDeliveredEvent);
    }

    [Fact]
    public void MarkAsDelivered_WithoutShipping_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        Assert.Equal(OrderStatus.Processing, order.Status);

        // Act
        var deliveredResult = order.MarkAsDelivered();

        // Assert
        Assert.True(deliveredResult.IsFailure);
        Assert.Equal("Order.InvalidStatus", deliveredResult.Error.Code);
    }

    [Fact]
    public void MarkAsDelivered_WhenAlreadyDelivered_ShouldNotChangeStatus()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);

        var deliveredResult = order.MarkAsDelivered();
        Assert.True(deliveredResult.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);

        // Clear domain events for fresh test
        order.ClearDomainEvents();

        // Act
        var secondDeliveredResult = order.MarkAsDelivered();

        // Assert
        Assert.True(secondDeliveredResult.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Empty(order.DomainEvents); // No events should be raised
    }

    [Fact]
    public void Cancel_ShouldCancelOrder()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var reason = "Customer request";

        // Act - default behavior (customer)
        var cancelResult = order.Cancel(reason);

        // Assert
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(reason, order.Metadata["cancellationReason"]);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_WithoutReason_ShouldCancelWithoutReason()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        // Act - default behavior (customer)
        var cancelResult = order.Cancel();

        // Assert
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.False(order.Metadata.ContainsKey("cancellationReason"));
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);

        var deliveredResult = order.MarkAsDelivered();
        Assert.True(deliveredResult.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);

        // Act - default behavior (customer)
        var cancelResult = order.Cancel();

        // Assert
        Assert.True(cancelResult.IsFailure);
        Assert.Equal("Order.InvalidStatus", cancelResult.Error.Code);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldNotRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var cancelResult = order.Cancel("Initial reason");
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);

        // Clear domain events for fresh test
        order.ClearDomainEvents();

        // Act
        var secondCancelResult = order.Cancel("New reason");

        // Assert
        Assert.True(secondCancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Initial reason", order.Metadata["cancellationReason"]); // Shouldn't change
        Assert.Empty(order.DomainEvents); // No events should be raised
    }

    [Fact]
    public void UpdateMetadata_ShouldAddOrUpdateMetadata()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var key = "note";
        var value = "Please leave at the door";

        // Act
        var updateResult = order.UpdateMetadata(key, value);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.True(order.Metadata.ContainsKey(key));
        Assert.Equal(value, order.Metadata[key]);
    }

    [Fact]
    public void UpdateMetadata_WithEmptyKey_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var key = string.Empty;
        var value = "test value";

        // Act
        var updateResult = order.UpdateMetadata(key, value);

        // Assert
        Assert.True(updateResult.IsFailure);
        Assert.Equal("Order.InvalidMetadataKey", updateResult.Error.Code);
    }

    [Fact]
    public void AddMultipleItems_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(0, "USD");
        Assert.True(subtotalResult.IsSuccess);
        var subtotal = subtotalResult.Value;

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);
        var tax = taxResult.Value;

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);
        var shippingCost = shippingCostResult.Value;

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotal,
            tax,
            shippingCost);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var slugResult1 = Slug.Create("product-1");
        Assert.True(slugResult1.IsSuccess);

        var priceResult1 = Money.FromDollars(50);
        Assert.True(priceResult1.IsSuccess);

        var productResult1 = Product.Create("Product 1", slugResult1.Value, priceResult1.Value);
        Assert.True(productResult1.IsSuccess);
        var product1 = productResult1.Value;

        var slugResult2 = Slug.Create("product-2");
        Assert.True(slugResult2.IsSuccess);

        var priceResult2 = Money.FromDollars(75);
        Assert.True(priceResult2.IsSuccess);

        var productResult2 = Product.Create("Product 2", slugResult2.Value, priceResult2.Value);
        Assert.True(productResult2.IsSuccess);
        var product2 = productResult2.Value;

        var slugResult3 = Slug.Create("product-3");
        Assert.True(slugResult3.IsSuccess);

        var priceResult3 = Money.FromDollars(25);
        Assert.True(priceResult3.IsSuccess);

        var productResult3 = Product.Create("Product 3", slugResult3.Value, priceResult3.Value);
        Assert.True(productResult3.IsSuccess);
        var product3 = productResult3.Value;

        // Act
        order.AddItem(product1, 2, priceResult1.Value); // $100
        order.AddItem(product2, 1, priceResult2.Value); // $75
        order.AddItem(product3, 3, priceResult3.Value); // $75

        // Assert
        Assert.Equal(3, order.Items.Count);

        // Expected subtotal: $100 + $75 + $75 = $250
        Assert.Equal(250m, order.Subtotal.Amount);

        // Expected total: $250 + $10 + $5 = $265
        Assert.Equal(265m, order.Total.Amount);
    }

    [Fact]
    public void Cancel_CustomerCancellPendingOrder_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.Create(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        Assert.Equal(OrderStatus.Pending, order.Status);

        // Act
        var cancelResult = order.Cancel("Customer request", isAdminOrManager: false);

        // Assert
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_CustomerCancelProcessingOrder_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;
        Assert.Equal(OrderStatus.Processing, order.Status);

        // Act
        var cancelResult = order.Cancel("Customer request", isAdminOrManager: false);

        // Assert
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_CustomerCancelShippedOrder_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);

        // Act
        var cancelResult = order.Cancel("Customer request", isAdminOrManager: false);

        // Assert
        Assert.True(cancelResult.IsFailure);
        Assert.Equal("Order.InvalidStatus", cancelResult.Error.Code);
        Assert.Contains("cancel - only pending or processing orders can be cancelled by customers", cancelResult.Error.Message);
    }

    [Fact]
    public void Cancel_AdminCancelShippedOrder_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);

        // Act
        var cancelResult = order.Cancel("Admin cancellation", isAdminOrManager: true);

        // Assert
        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_AdminCancelDeliveredOrder_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var shippingAddress = CreateTestAddress(user);
        var billingAddress = CreateTestAddress(user);

        var subtotalResult = Money.Create(100, "USD");
        Assert.True(subtotalResult.IsSuccess);

        var taxResult = Money.Create(10, "USD");
        Assert.True(taxResult.IsSuccess);

        var shippingCostResult = Money.Create(5, "USD");
        Assert.True(shippingCostResult.IsSuccess);

        var orderResult = Order.CreatePaidOrder(
            user,
            shippingAddress,
            billingAddress,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);

        Assert.True(orderResult.IsSuccess);
        var order = orderResult.Value;

        var shippedResult = order.MarkAsShipped();
        Assert.True(shippedResult.IsSuccess);

        var deliveredResult = order.MarkAsDelivered();
        Assert.True(deliveredResult.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);

        // Act
        var cancelResult = order.Cancel("Admin cancellation", isAdminOrManager: true);

        // Assert
        Assert.True(cancelResult.IsFailure);
        Assert.Equal("Order.InvalidStatus", cancelResult.Error.Code);
        Assert.Contains("cancel - delivered orders cannot be cancelled", cancelResult.Error.Message);
    }
}