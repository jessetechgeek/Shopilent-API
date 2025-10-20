using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding.Seeders;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Extensions;

public static class DatabaseSeedingExtensions
{
    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ISeedingOrchestrator>>();
        var seedingOptions = services.GetRequiredService<IOptions<SeedingSettings>>().Value;

        // Always ensure default admin user exists, regardless of seeding configuration
        try
        {
            var userSeeder = services.GetRequiredService<UserSeeder>();
            await userSeeder.EnsureDefaultAdminUserAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure default admin user exists");
        }

        if (!seedingOptions.AutoSeed)
        {
            logger.LogInformation("Database auto-seeding is disabled. Skipping seeding.");
            return app;
        }

        try
        {
            logger.LogInformation("Database auto-seeding is enabled. Starting seeding process...");

            var orchestrator = services.GetRequiredService<ISeedingOrchestrator>();
            var results = await orchestrator.SeedAllAsync();

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);

            if (failureCount > 0)
            {
                logger.LogWarning("Database seeding completed with {FailureCount} failures and {SuccessCount} successes",
                    failureCount, successCount);
            }
            else
            {
                logger.LogInformation("Database seeding completed successfully. All {Count} seeders executed successfully",
                    successCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "A fatal error occurred during database seeding. Application startup will continue, but database may not be properly seeded.");
        }

        return app;
    }
}
