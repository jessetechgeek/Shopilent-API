using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Payments;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;

internal class CustomerCreatedHandler : IStripeWebhookHandler
{
    private readonly ILogger<CustomerCreatedHandler> _logger;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task<WebhookResult> HandleAsync(Event stripeEvent, WebhookResult result, CancellationToken cancellationToken)
    {
        var customer = stripeEvent.Data.Object as Customer;
        if (customer == null)
        {
            result.ProcessingMessage = "Invalid Customer data in webhook";
            return result;
        }

        result.CustomerId = customer.Id;
        result.EventData.Add("email", customer.Email ?? string.Empty);

        if (customer.Metadata != null)
        {
            result.EventData.Add("metadata", customer.Metadata);
        }

        result.ProcessingMessage = $"Customer created: {customer.Id}";
        result.IsProcessed = true;

        _logger.LogInformation("Customer created: {CustomerId}", customer.Id);

        return result;
    }
}