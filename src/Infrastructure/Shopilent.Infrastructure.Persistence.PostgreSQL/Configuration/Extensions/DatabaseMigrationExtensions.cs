using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopilent.Infrastructure.Persistence.PostgreSQL.Context;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration.Extensions;

public static class DatabaseMigrationExtensions
{
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var databaseOptions = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (!databaseOptions.AutoMigrate)
        {
            logger.LogInformation("Database auto-migration is disabled. Skipping migration check.");
            return app;
        }

        try
        {
            logger.LogInformation("Database auto-migration is enabled. Checking for pending migrations...");

            var context = services.GetRequiredService<ApplicationDbContext>();
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (!pendingMigrations.Any())
            {
                logger.LogInformation("Database is up to date. No pending migrations found.");
                return app;
            }

            logger.LogInformation("Found {MigrationCount} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();

            logger.LogInformation("Successfully applied {MigrationCount} migration(s) to the database.",
                pendingMigrations.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while applying database migrations. Application startup will continue, but database may be in an inconsistent state.");
        }

        return app;
    }
}
