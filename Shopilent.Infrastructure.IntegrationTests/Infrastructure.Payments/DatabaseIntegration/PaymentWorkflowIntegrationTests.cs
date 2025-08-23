using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Persistence;
using Shopilent.Domain.Payments.Enums;
using Shopilent.Infrastructure.IntegrationTests.Common;
using Shopilent.Infrastructure.IntegrationTests.TestData.Builders;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Payments.DatabaseIntegration;

[Collection("IntegrationTests")]
public class PaymentWorkflowIntegrationTests : IntegrationTestBase
{
    private IUnitOfWork _unitOfWork = null!;

    public PaymentWorkflowIntegrationTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _unitOfWork = GetService<IUnitOfWork>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CompletePaymentWorkflow_WithCreditCard_ShouldPersistAllEntities()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Create user and order
        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();

        // Create payment method
        var paymentMethod = PaymentMethodBuilder.Random()
            .WithUser(user)
            .WithCreditCard()
            .Build();

        // Create payment with payment method
        var payment = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(100m, "USD")
            .WithPaymentMethod(paymentMethod)
            .WithStripeCard()
            .Build();

        // Act - Persist all entities in correct order
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.PaymentMethodWriter.AddAsync(paymentMethod);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verify all entities are persisted
        var persistedUser = await _unitOfWork.UserReader.GetByIdAsync(user.Id);
        var persistedOrder = await _unitOfWork.OrderReader.GetByIdAsync(order.Id);
        var persistedPaymentMethod = await _unitOfWork.PaymentMethodReader.GetByIdAsync(paymentMethod.Id);
        var persistedPayment = await _unitOfWork.PaymentReader.GetByIdAsync(payment.Id);

        persistedUser.Should().NotBeNull();
        persistedOrder.Should().NotBeNull();
        persistedPaymentMethod.Should().NotBeNull();
        persistedPayment.Should().NotBeNull();

        // Verify relationships
        persistedPayment.UserId.Should().Be(user.Id);
        persistedPayment.OrderId.Should().Be(order.Id);
        persistedPayment.PaymentMethodId.Should().Be(paymentMethod.Id);
        persistedPayment.Amount.Should().Be(100m);
        persistedPayment.Currency.Should().Be("USD");
        persistedPayment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task PaymentSuccessWorkflow_ShouldUpdateStatusAndPersist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();
        var payment = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(50m, "USD")
            .WithStripeCard()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Act - Mark payment as succeeded
        const string transactionId = "pi_test_succeeded";
        payment.MarkAsSucceeded(transactionId);
        await _unitOfWork.PaymentWriter.UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedPayment = await _unitOfWork.PaymentReader.GetByIdAsync(payment.Id);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentStatus.Succeeded);
        updatedPayment.TransactionId.Should().Be(transactionId);
        updatedPayment.UpdatedAt.Should().BeAfter(updatedPayment.CreatedAt);

        // Verify transaction ID lookup works
        var paymentByTransaction = await _unitOfWork.PaymentReader.GetByTransactionIdAsync(transactionId);
        paymentByTransaction.Should().NotBeNull();
        paymentByTransaction!.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task PaymentFailureWorkflow_ShouldUpdateStatusWithReason()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();
        var payment = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(75m, "USD")
            .WithStripeCard()
            .Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Act - Mark payment as failed
        const string failureReason = "Your card was declined";
        payment.MarkAsFailed(failureReason);
        await _unitOfWork.PaymentWriter.UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var updatedPayment = await _unitOfWork.PaymentReader.GetByIdAsync(payment.Id);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentStatus.Failed);
        updatedPayment.ErrorMessage.Should().Be(failureReason);
        updatedPayment.UpdatedAt.Should().BeAfter(updatedPayment.CreatedAt);
    }

    [Fact]
    public async Task MultiplePaymentsForOrder_ShouldAllBePersisted()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();

        // Create multiple payments for the same order (retry scenario)
        var payment1 = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(100m, "USD")
            .WithStripeCard()
            .Build();

        var payment2 = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(100m, "USD")
            .WithStripeCard()
            .Build();

        // Mark first payment as failed, second as succeeded
        payment1.MarkAsFailed("Card declined");
        payment2.MarkAsSucceeded("pi_test_retry_success");

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment1);
        await _unitOfWork.PaymentWriter.AddAsync(payment2);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var paymentsForOrder = await _unitOfWork.PaymentReader.GetByOrderIdAsync(order.Id);
        paymentsForOrder.Should().HaveCount(2);

        var failedPayment = paymentsForOrder.First(p => p.Status == PaymentStatus.Failed);
        var successfulPayment = paymentsForOrder.First(p => p.Status == PaymentStatus.Succeeded);

        failedPayment.Should().NotBeNull();
        failedPayment.ErrorMessage.Should().Be("Card declined");

        successfulPayment.Should().NotBeNull();
        successfulPayment.TransactionId.Should().Be("pi_test_retry_success");
    }

    [Fact]
    public async Task PaymentWithPaymentMethod_ShouldMaintainRelationship()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();
        
        var paymentMethod = PaymentMethodBuilder.Random()
            .WithUser(user)
            .WithCreditCard()
            .WithDisplayName("My Visa Card")
            .Build();

        var payment = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(125m, "USD")
            .WithPaymentMethod(paymentMethod)
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.PaymentMethodWriter.AddAsync(paymentMethod);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var persistedPayment = await _unitOfWork.PaymentReader.GetByIdAsync(payment.Id);
        persistedPayment.Should().NotBeNull();
        persistedPayment!.PaymentMethodId.Should().Be(paymentMethod.Id);

        // Verify we can find payments by payment method
        var paymentsByMethod = await _unitOfWork.PaymentReader.GetByPaymentMethodIdAsync(paymentMethod.Id);
        paymentsByMethod.Should().HaveCount(1);
        paymentsByMethod.First().Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task PaymentStatusFiltering_ShouldReturnCorrectPayments()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order1 = OrderBuilder.Random().WithUser(user).Build();
        var order2 = OrderBuilder.Random().WithUser(user).Build();
        var order3 = OrderBuilder.Random().WithUser(user).Build();

        var pendingPayment = PaymentBuilder.Random().WithOrder(order1).WithUser(user).Build();
        var succeededPayment = PaymentBuilder.Random().WithOrder(order2).WithUser(user).Build();
        var failedPayment = PaymentBuilder.Random().WithOrder(order3).WithUser(user).Build();

        succeededPayment.MarkAsSucceeded("pi_test_success");
        failedPayment.MarkAsFailed("Insufficient funds");

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order1);
        await _unitOfWork.OrderWriter.AddAsync(order2);
        await _unitOfWork.OrderWriter.AddAsync(order3);
        await _unitOfWork.PaymentWriter.AddAsync(pendingPayment);
        await _unitOfWork.PaymentWriter.AddAsync(succeededPayment);
        await _unitOfWork.PaymentWriter.AddAsync(failedPayment);
        await _unitOfWork.SaveChangesAsync();

        // Act & Assert
        var pendingPayments = await _unitOfWork.PaymentReader.GetByStatusAsync(PaymentStatus.Pending);
        pendingPayments.Should().HaveCount(1);
        pendingPayments.First().Id.Should().Be(pendingPayment.Id);

        var succeededPayments = await _unitOfWork.PaymentReader.GetByStatusAsync(PaymentStatus.Succeeded);
        succeededPayments.Should().HaveCount(1);
        succeededPayments.First().Id.Should().Be(succeededPayment.Id);

        var failedPayments = await _unitOfWork.PaymentReader.GetByStatusAsync(PaymentStatus.Failed);
        failedPayments.Should().HaveCount(1);
        failedPayments.First().Id.Should().Be(failedPayment.Id);
    }

    [Fact]
    public async Task PaymentExternalReferenceWorkflow_ShouldEnableTracking()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();
        const string externalRef = "stripe_intent_pi_12345";

        var payment = PaymentBuilder.Random()
            .WithOrder(order)
            .WithUser(user)
            .WithAmount(200m, "USD")
            .WithExternalReference(externalRef)
            .WithStripeCard()
            .Build();

        // Act
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.PaymentWriter.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var paymentByReference = await _unitOfWork.PaymentReader.GetByExternalReferenceAsync(externalRef);
        paymentByReference.Should().NotBeNull();
        paymentByReference!.Id.Should().Be(payment.Id);
        paymentByReference.ExternalReference.Should().Be(externalRef);
        paymentByReference.Provider.Should().Be(PaymentProvider.Stripe);
    }

    [Fact]
    public async Task RecentPaymentsQuery_ShouldReturnInCorrectOrder()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var payments = new List<Domain.Payments.Payment>();

        // Create payments one at a time with individual saves to ensure different timestamps
        for (int i = 0; i < 5; i++)
        {
            var order = OrderBuilder.Random().WithUser(user).Build();
            var payment = PaymentBuilder.Random()
                .WithOrder(order)
                .WithUser(user)
                .WithAmount(100m + i * 10, "USD")
                .WithStripeCard()
                .Build();

            payments.Add(payment);
            await _unitOfWork.OrderWriter.AddAsync(order);
            await _unitOfWork.PaymentWriter.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            
            // Add delay to ensure different creation timestamps
            await Task.Delay(50);
        }

        // Act
        var recentPayments = await _unitOfWork.PaymentReader.GetRecentPaymentsAsync(3);

        // Assert
        recentPayments.Should().HaveCount(3);
        
        // Should be ordered by creation time descending (most recent first)
        var orderedPayments = recentPayments.ToList();
        for (int i = 0; i < orderedPayments.Count - 1; i++)
        {
            orderedPayments[i].CreatedAt.Should().BeOnOrAfter(orderedPayments[i + 1].CreatedAt);
        }

        // Most recent payment should be the last one we created
        orderedPayments.First().Amount.Should().Be(140m); // 100 + 4*10
    }

    [Fact]
    public async Task ConcurrentPaymentCreation_ShouldHandleCorrectly()
    {
        // Arrange
        await ResetDatabaseAsync();

        var user = UserBuilder.Random().WithVerifiedEmail().Build();
        var order = OrderBuilder.Random().WithUser(user).Build();

        await _unitOfWork.UserWriter.AddAsync(user);
        await _unitOfWork.OrderWriter.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Act - Create multiple payments concurrently
        var tasks = Enumerable.Range(1, 3).Select(async i =>
        {
            using var scope = ServiceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var payment = PaymentBuilder.Random()
                .WithOrder(order)
                .WithUser(user)
                .WithAmount(i * 25m, "USD")
                .WithStripeCard()
                .Build();

            await unitOfWork.PaymentWriter.AddAsync(payment);
            await unitOfWork.SaveChangesAsync();
            
            return payment.Id;
        });

        var paymentIds = await Task.WhenAll(tasks);

        // Assert
        paymentIds.Should().HaveCount(3);
        paymentIds.Should().OnlyHaveUniqueItems();

        var allPayments = await _unitOfWork.PaymentReader.GetByOrderIdAsync(order.Id);
        allPayments.Should().HaveCount(3);
        allPayments.Select(p => p.Amount).Should().BeEquivalentTo(new[] { 25m, 50m, 75m });
    }
}