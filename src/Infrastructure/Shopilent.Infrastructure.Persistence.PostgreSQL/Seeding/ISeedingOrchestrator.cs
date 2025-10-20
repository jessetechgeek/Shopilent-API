namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;

public interface ISeedingOrchestrator
{
    Task<IEnumerable<SeedingResult>> SeedAllAsync(CancellationToken cancellationToken = default);
}
