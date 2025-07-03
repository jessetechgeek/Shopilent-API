using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Application.Abstractions.Email;
using Shopilent.Application.Abstractions.Events;
using Shopilent.Application.Abstractions.Imaging;
using Shopilent.Application.Abstractions.Outbox;
using Shopilent.Application.Settings.Email;
using Shopilent.Application.Settings.Outbox;
using Shopilent.Domain.Common;
using Shopilent.Infrastructure.BackgroundServices.Outbox;
using Shopilent.Infrastructure.OpenApi;
using Shopilent.Infrastructure.Services;
using Shopilent.Infrastructure.Services.Common;
using Shopilent.Infrastructure.Services.Email;
using Shopilent.Infrastructure.Services.Events;
using Shopilent.Infrastructure.Services.Imaging;
using Shopilent.Infrastructure.Services.Outbox;

namespace Shopilent.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDomainEventService, DomainEventService>();

        services.AddSingleton<IDateTimeProvider, DateTimeProviderService>();

        services.AddScoped<IEmailService, EmailService>();
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddScoped<IImageService, ImageService>();

        services.AddOpenApi("v1", options => { options.AddDocumentTransformer<OpenApiDocumentTransformer>(); });
        services.AddOpenApi("v2", options => { options.AddDocumentTransformer<OpenApiDocumentTransformer>(); });

        services.AddScoped<IOutboxService, OutboxService>();
        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));

        services.AddHostedService<OutboxProcessingService>();

        return services;
    }
}