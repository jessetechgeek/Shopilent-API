using Microsoft.Extensions.Logging;
using Npgsql;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Configuration;

public class PostgresConnectionConfig
{
    private readonly ILogger<PostgresConnectionConfig> _logger;

    public string WriteConnectionString { get; set; } = string.Empty;
    public List<string> ReadConnectionStrings { get; set; } = new List<string>();

    private int _currentReadReplicaIndex = 0;

    public PostgresConnectionConfig(ILogger<PostgresConnectionConfig> logger = null)
    {
        _logger = logger;
    }

    public string GetReadConnectionString()
    {
        if (ReadConnectionStrings == null || !ReadConnectionStrings.Any())
        {
            _logger?.LogInformation("Using write connection as no read replicas configured");
            return WriteConnectionString;
        }

        // Try to find a healthy connection
        for (int i = 0; i < ReadConnectionStrings.Count; i++)
        {
            var index = (_currentReadReplicaIndex + i) % ReadConnectionStrings.Count;
            var connectionString = ReadConnectionStrings[index];

            // Extract server/host information for logging without sensitive details
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var serverInfo = $"Host={builder.Host}, Port={builder.Port}, Database={builder.Database}";

            if (IsConnectionHealthy(connectionString))
            {
                _logger?.LogInformation("Using read replica {ReplicaIndex}: {ServerInfo}", index, serverInfo);
                _currentReadReplicaIndex = (index + 1) % ReadConnectionStrings.Count;
                return connectionString;
            }
            else
            {
                _logger?.LogWarning("Read replica {ReplicaIndex} is unhealthy: {ServerInfo}", index, serverInfo);
            }
        }

        // If no healthy read replica, fall back to write connection
        _logger?.LogWarning("All read replicas are unhealthy, falling back to write connection");
        return WriteConnectionString;
    }

    private bool IsConnectionHealthy(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.ExecuteScalar();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking connection health");
            return false;
        }
    }
}