namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;

public interface IDataSeeder
{
    string SeederName { get; }
    int Order { get; }
    Task<SeedingResult> SeedAsync(CancellationToken cancellationToken = default);
}
