using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Payments;
using Shopilent.Infrastructure.Payments.Abstractions;
using Shopilent.Infrastructure.Payments.Providers.Stripe;
using Shopilent.Infrastructure.Payments.Providers.Stripe.Handlers;
using Shopilent.Infrastructure.Payments.Services;
using Shopilent.Infrastructure.Payments.Settings;

namespace Shopilent.Infrastructure.Payments.Extensions;

public static class PaymentsServiceExtensions
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register payment service
        services.AddScoped<IPaymentService, PaymentService>();
        
        // Configure Stripe settings
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        
        // Register payment providers
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        // services.AddScoped<IPaymentProvider, PayPalPaymentProvider>();

        // Register Stripe webhook handlers
        services.AddScoped<PaymentIntentSucceededHandler>();
        services.AddScoped<PaymentIntentFailedHandler>();
        services.AddScoped<PaymentIntentRequiresActionHandler>();
        services.AddScoped<PaymentIntentCanceledHandler>();
        services.AddScoped<ChargeSucceededHandler>();
        services.AddScoped<ChargeDisputeCreatedHandler>();
        services.AddScoped<CustomerCreatedHandler>();
        services.AddScoped<CustomerUpdatedHandler>();
        services.AddScoped<PaymentMethodAttachedHandler>();
        services.AddScoped<StripeWebhookHandlerFactory>();

        return services;
    }
}