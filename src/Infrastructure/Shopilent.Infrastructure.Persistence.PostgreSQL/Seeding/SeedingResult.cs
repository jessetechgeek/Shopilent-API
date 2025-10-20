namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Seeding;

public class SeedingResult
{
    public bool Success { get; set; }
    public string SeederName { get; set; } = string.Empty;
    public int RecordsCreated { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }

    public static SeedingResult SuccessResult(string seederName, int recordsCreated, TimeSpan duration, string message = null)
    {
        return new SeedingResult
        {
            Success = true,
            SeederName = seederName,
            RecordsCreated = recordsCreated,
            Duration = duration,
            Message = message ?? $"Successfully seeded {recordsCreated} records"
        };
    }

    public static SeedingResult FailureResult(string seederName, Exception exception, string message = null)
    {
        return new SeedingResult
        {
            Success = false,
            SeederName = seederName,
            RecordsCreated = 0,
            Duration = TimeSpan.Zero,
            Message = message ?? exception.Message,
            Exception = exception
        };
    }
}
