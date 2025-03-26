using Shopilent.Domain.Identity;
using Shopilent.Domain.Identity.ValueObjects;
using Shopilent.Domain.Payments;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Domain.Payments.Specifications;
using Shopilent.Domain.Payments.ValueObjects;
using Shopilent.Domain.Sales;
using Shopilent.Domain.Sales.ValueObjects;
using Shopilent.Domain.Shipping;
using Shopilent.Domain.Shipping.ValueObjects;

namespace Shopilent.Domain.Tests.Payments.Specifications;

public class PaymentSpecificationsTests
{
    private User CreateTestUser()
    {
        var emailResult = Email.Create("test@example.com");
        var fullNameResult = FullName.Create("Test", "User");
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

    private Order CreateTestOrder(User user, Address address)
    {
        var subtotalResult = Money.Create(100, "USD");
        var taxResult = Money.Create(10, "USD");
        var shippingCostResult = Money.Create(5, "USD");
        
        Assert.True(subtotalResult.IsSuccess);
        Assert.True(taxResult.IsSuccess);
        Assert.True(shippingCostResult.IsSuccess);
        
        var orderResult = Order.Create(
            user,
            address,
            address,
            subtotalResult.Value,
            taxResult.Value,
            shippingCostResult.Value);
            
        Assert.True(orderResult.IsSuccess);
        return orderResult.Value;
    }

    [Fact]
    public void ActivePaymentMethodSpecification_WithActiveMethod_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetailsResult.Value);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var specification = new ActivePaymentMethodSpecification();

        // Act
        var result = specification.IsSatisfiedBy(paymentMethod);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ActivePaymentMethodSpecification_WithInactiveMethod_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetailsResult.Value);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var deactivateResult = paymentMethod.Deactivate();
        Assert.True(deactivateResult.IsSuccess);

        var specification = new ActivePaymentMethodSpecification();

        // Act
        var result = specification.IsSatisfiedBy(paymentMethod);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DefaultPaymentMethodSpecification_WithDefaultMethod_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetailsResult.Value,
            isDefault: true);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var specification = new DefaultPaymentMethodSpecification();

        // Act
        var result = specification.IsSatisfiedBy(paymentMethod);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DefaultPaymentMethodSpecification_WithNonDefaultMethod_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var cardDetailsResult = PaymentCardDetails.Create("Visa", "4242", DateTime.UtcNow.AddYears(1));
        Assert.True(cardDetailsResult.IsSuccess);
        
        var paymentMethodResult = PaymentMethod.CreateCardMethod(
            user,
            PaymentProvider.Stripe,
            "tok_visa_123",
            cardDetailsResult.Value,
            isDefault: false);
        Assert.True(paymentMethodResult.IsSuccess);
        var paymentMethod = paymentMethodResult.Value;

        var specification = new DefaultPaymentMethodSpecification();

        // Act
        var result = specification.IsSatisfiedBy(paymentMethod);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PaymentByOrderSpecification_WithMatchingOrder_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var specification = new PaymentByOrderSpecification(order.Id);

        // Act
        var result = specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PaymentByOrderSpecification_WithDifferentOrder_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var differentOrderId = Guid.NewGuid();

        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var specification = new PaymentByOrderSpecification(differentOrderId);

        // Act
        var result = specification.IsSatisfiedBy(payment);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SuccessfulPaymentSpecification_WithSuccessfulPayment_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var successResult = payment.MarkAsSucceeded("txn_123");
        Assert.True(successResult.IsSuccess);

        var specification = new SuccessfulPaymentSpecification();

        // Act
        var result = specification.IsSatisfiedBy(payment);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SuccessfulPaymentSpecification_WithPendingPayment_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        // Still pending

        var specification = new SuccessfulPaymentSpecification();

        // Act
        var result = specification.IsSatisfiedBy(payment);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SuccessfulPaymentSpecification_WithFailedPayment_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var address = CreateTestAddress(user);
        var order = CreateTestOrder(user, address);
        var amountResult = Money.Create(115, "USD");
        Assert.True(amountResult.IsSuccess);
        
        var paymentResult = Payment.Create(
            order,
            user,
            amountResult.Value,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe);
        Assert.True(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        var failedResult = payment.MarkAsFailed("Payment failed");
        Assert.True(failedResult.IsSuccess);

        var specification = new SuccessfulPaymentSpecification();

        // Act
        var result = specification.IsSatisfiedBy(payment);

        // Assert
        Assert.False(result);
    }
}