using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding.Seeders;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Extensions;

public static class SeedingServiceExtensions
{
    public static IServiceCollection AddSeedingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure seeding options
        services.Configure<SeedingSettings>(configuration.GetSection(SeedingSettings.SectionName));

        // Register orchestrator
        services.AddScoped<ISeedingOrchestrator, SeedingOrchestrator>();

        // Register all seeders
        services.AddScoped<UserSeeder>();
        services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<UserSeeder>());
        // Future seeders will be added here:
        // services.AddScoped<IDataSeeder, CategorySeeder>();
        // services.AddScoped<IDataSeeder, AttributeSeeder>();
        // services.AddScoped<IDataSeeder, ProductSeeder>();
        // services.AddScoped<IDataSeeder, OrderSeeder>();

        return services;
    }
}
