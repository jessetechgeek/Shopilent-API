using Microsoft.Extensions.Logging;
using Shopilent.Application.Abstractions.Payments;
using Stripe;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;

internal class ChargeDisputeCreatedHandler : IStripeWebhookHandler
{
    private readonly ILogger<ChargeDisputeCreatedHandler> _logger;

    public ChargeDisputeCreatedHandler(ILogger<ChargeDisputeCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task<WebhookResult> HandleAsync(Event stripeEvent, WebhookResult result, CancellationToken cancellationToken)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute == null)
        {
            result.ProcessingMessage = "Invalid Dispute data in webhook";
            return result;
        }

        result.TransactionId = dispute.ChargeId;
        result.EventData.Add("dispute_id", dispute.Id);
        result.EventData.Add("dispute_reason", dispute.Reason);
        result.EventData.Add("dispute_amount", dispute.Amount);
        result.EventData.Add("dispute_currency", dispute.Currency);

        result.OrderId = dispute.Metadata["orderId"];
        result.ProcessingMessage = $"Dispute created for charge: {dispute.ChargeId}";
        result.IsProcessed = true;

        _logger.LogWarning("Dispute created for charge: {ChargeId}, Reason: {Reason}",
            dispute.ChargeId, dispute.Reason);

        return result;
    }
}