using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Payments;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;

internal class PaymentMethodAttachedHandler : IStripeWebhookHandler
{
    private readonly ILogger<PaymentMethodAttachedHandler> _logger;

    public PaymentMethodAttachedHandler(ILogger<PaymentMethodAttachedHandler> logger)
    {
        _logger = logger;
    }

    public async Task<WebhookResult> HandleAsync(Event stripeEvent, WebhookResult result, CancellationToken cancellationToken)
    {
        var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
        if (paymentMethod == null)
        {
            result.ProcessingMessage = "Invalid PaymentMethod data in webhook";
            return result;
        }

        result.CustomerId = paymentMethod.CustomerId;
        result.EventData.Add("payment_method_id", paymentMethod.Id);
        result.EventData.Add("payment_method_type", paymentMethod.Type);

        if (paymentMethod.Card != null)
        {
            result.EventData.Add("card_brand", paymentMethod.Card.Brand);
            result.EventData.Add("card_last4", paymentMethod.Card.Last4);
            result.EventData.Add("card_exp_month", paymentMethod.Card.ExpMonth);
            result.EventData.Add("card_exp_year", paymentMethod.Card.ExpYear);
        }

        result.ProcessingMessage = $"Payment method attached: {paymentMethod.Id}";
        result.IsProcessed = true;

        _logger.LogInformation("Payment method attached: {PaymentMethodId} to customer: {CustomerId}",
            paymentMethod.Id, paymentMethod.CustomerId);

        return result;
    }
}