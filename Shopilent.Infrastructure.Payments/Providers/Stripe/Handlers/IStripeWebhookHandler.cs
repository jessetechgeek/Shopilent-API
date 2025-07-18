using Shopilent.Application.Abstractions.Payments;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;

internal interface IStripeWebhookHandler
{
    Task<WebhookResult> HandleAsync(Event stripeEvent, WebhookResult result, CancellationToken cancellationToken);
}