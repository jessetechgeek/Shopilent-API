using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;

internal class StripeWebhookHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StripeWebhookHandlerFactory> _logger;

    public StripeWebhookHandlerFactory(IServiceProvider serviceProvider, ILogger<StripeWebhookHandlerFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IStripeWebhookHandler? GetHandler(string eventType)
    {
        return eventType switch
        {
            "payment_intent.succeeded" => _serviceProvider.GetRequiredService<PaymentIntentSucceededHandler>(),
            "payment_intent.payment_failed" => _serviceProvider.GetRequiredService<PaymentIntentFailedHandler>(),
            "payment_intent.requires_action" => _serviceProvider.GetRequiredService<PaymentIntentRequiresActionHandler>(),
            "payment_intent.canceled" => _serviceProvider.GetRequiredService<PaymentIntentCanceledHandler>(),
            "charge.succeeded" => _serviceProvider.GetRequiredService<ChargeSucceededHandler>(),
            "charge.dispute.created" => _serviceProvider.GetRequiredService<ChargeDisputeCreatedHandler>(),
            "customer.created" => _serviceProvider.GetRequiredService<CustomerCreatedHandler>(),
            "customer.updated" => _serviceProvider.GetRequiredService<CustomerUpdatedHandler>(),
            "payment_method.attached" => _serviceProvider.GetRequiredService<PaymentMethodAttachedHandler>(),
            "setup_intent.succeeded" => _serviceProvider.GetRequiredService<SetupIntentSucceededHandler>(),
            "setup_intent.requires_action" => _serviceProvider.GetRequiredService<SetupIntentRequiresActionHandler>(),
            "setup_intent.canceled" => _serviceProvider.GetRequiredService<SetupIntentCanceledHandler>(),
            _ => null
        };
    }
}