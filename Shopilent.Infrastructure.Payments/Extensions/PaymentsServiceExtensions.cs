using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Services;
using Shopilent.Infrastructure.Payments.Configuration;
using Shopilent.Infrastructure.Payments.Providers;
using Shopilent.Infrastructure.Payments.Services;

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

        return services;
    }
}