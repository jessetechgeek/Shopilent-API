using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Services;
using Shopilent.Infrastructure.Payments.Services;

namespace Shopilent.Infrastructure.Payments.Extensions;

public static class PaymentsServiceExtensions
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        // services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        // services.AddScoped<IPaymentProvider, PayPalPaymentProvider>();

        return services;
    }
}