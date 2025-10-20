namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;

public class SeedingSettings
{
    public const string SectionName = "Seeding";

    public bool AutoSeed { get; set; }
    public bool CleanBeforeSeed { get; set; }
    public SeedCounts Counts { get; set; } = new();
}

public class SeedCounts
{
    public int Admins { get; set; }
    public int Managers { get; set; }
    public int Customers { get; set; }
}
