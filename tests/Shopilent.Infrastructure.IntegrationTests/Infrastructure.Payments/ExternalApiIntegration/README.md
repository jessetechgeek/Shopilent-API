# External API Integration Tests

This folder contains tests that make real API calls to external payment services.

## Purpose

These tests verify:
- ✅ **Real API connectivity** - Actual HTTP calls to payment providers
- ✅ **Authentication** - API key validation and authorization
- ✅ **Network behavior** - Timeouts, retries, rate limiting
- ✅ **Contract validation** - API response format changes
- ✅ **Webhook signatures** - Real signature validation
- ✅ **Error scenarios** - Actual API error responses

## Current Status

🚧 **TODO**: These tests are not yet implemented. They are placeholder files showing what should be tested.

## Implementation Guidelines

### Stripe Integration Tests
- Use real Stripe test API keys (sk_test_..., pk_test_...)
- Use Stripe test card numbers (4242424242424242, 4000000000000002, etc.)
- Test actual webhook signature validation
- Verify real API error responses

### Test Environment Setup
- Configure test API keys in test configuration
- Ensure tests don't affect production data
- Clean up test data after test runs
- Use Stripe's test mode only

### Test Categories
1. **Payment Processing** - Real payment intent creation
2. **Customer Management** - Customer creation and updates
3. **Webhook Handling** - Signature validation and event processing
4. **Error Scenarios** - Network failures, invalid requests
5. **Rate Limiting** - API throttling behavior

## Running These Tests

When implemented, these tests should:
- Be marked with appropriate test categories
- Run separately from unit/database tests
- Require network connectivity
- Use real but safe test credentials
- Clean up any created test data

## Example Test Structure

```csharp
[Fact]
public async Task ProcessPayment_WithRealStripeApi_ShouldSucceed()
{
    // Arrange - Real test configuration
    var stripeProvider = new StripePaymentProvider(realTestConfig);
    
    // Act - Real API call
    var result = await stripeProvider.ProcessPaymentAsync(new PaymentRequest
    {
        Amount = Money.Create(100m, "USD"),
        PaymentMethodToken = "pm_card_visa" // Stripe test card
    });
    
    // Assert - Real response validation
    result.IsSuccess.Should().BeTrue();
    result.Value.TransactionId.Should().StartWith("pi_");
    
    // Cleanup - Remove test data
    // ...
}
```